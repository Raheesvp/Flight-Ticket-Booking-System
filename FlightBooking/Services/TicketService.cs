using FlightBooking.Models.Domain;
using iTextSharp.text;
using iTextSharp.text.pdf;
using QRCoder;
using System;
using System.IO;
using System.Threading.Tasks;


namespace FlightBooking.Services
{
    public class TicketService :ITicketService
    {
        public async Task<byte[]> GenerateTicketPdfAsync(Booking booking)
        {
            if (booking == null) throw new ArgumentNullException(nameof(booking));

            return await Task.Run(() =>
            {
                using (var memoryStream = new MemoryStream())
                {
                    // Initialize PDF Document with custom margins
                    Document document = new Document(PageSize.A4, 36, 36, 36, 36);
                    PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                    document.Open();

                    // Font Configurations
                    Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, BaseColor.White);
                    Font sectionFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, new BaseColor(26, 26, 46)); // Navy
                    Font boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.Black);
                    Font textFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.Black);

                    // 1. Header Block Matrix
                    PdfPTable headerTable = new PdfPTable(1) { WidthPercentage = 100 };
                    PdfPCell headerCell = new PdfPCell(new Phrase("FLIGHT E-TICKET RECEIPT", titleFont))
                    {
                        BackgroundColor = new BaseColor(26, 26, 46), // Deep Navy
                        Padding = 15f,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Border = Rectangle.NO_BORDER
                    };
                    headerTable.AddCell(headerCell);
                    document.Add(headerTable);

                    document.Add(new Paragraph(" ")); // Spacer

                    // 2. Flight Meta & QR Code Grid (2 Columns)
                    PdfPTable metaTable = new PdfPTable(new float[] { 3f, 1f }) { WidthPercentage = 100 };

                    // Left Column: Flight Core Details
                    PdfPCell detailsCell = new PdfPCell() { Border = Rectangle.NO_BORDER };
                    detailsCell.AddElement(new Paragraph($"PNR REFERENCE: {booking.PNR}", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, new BaseColor(240, 90, 34)))); // Orange
                    detailsCell.AddElement(new Paragraph($"Flight: {booking.Flight.Airline.AirlineName} ({booking.Flight.FlightNumber})", boldFont));
                    detailsCell.AddElement(new Paragraph($"Route: {booking.Flight.FromAirport.City} ({booking.Flight.FromAirport.IATACode}) → {booking.Flight.ToAirport.City} ({booking.Flight.ToAirport.IATACode})", textFont));
                    detailsCell.AddElement(new Paragraph($"Departure: {booking.Flight.DepartureTime:dd MMM yyyy HH:mm}", textFont));
                    detailsCell.AddElement(new Paragraph($"Arrival: {booking.Flight.ArrivalTime:dd MMM yyyy HH:mm}", textFont));
                    detailsCell.AddElement(new Paragraph($"Total Fare: INR {booking.TotalAmount:N2}", boldFont));
                    metaTable.AddCell(detailsCell);

                    // Right Column: Programmatic QR Code
                    using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                    {
                        string qrDataString = $"PNR:{booking.PNR}|Flight:{booking.Flight.FlightNumber}|User:{booking.UserId}";
                        QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrDataString, QRCodeGenerator.ECCLevel.Q);
                        PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                        byte[] qrCodeAsPngByteArr = qrCode.GetGraphic(20);

                        Image qrImage = Image.GetInstance(qrCodeAsPngByteArr);
                        qrImage.ScaleAbsolute(90f, 90f);

                        PdfPCell qrCell = new PdfPCell(qrImage)
                        {
                            HorizontalAlignment = Element.ALIGN_RIGHT,
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            Border = Rectangle.NO_BORDER
                        };
                        metaTable.AddCell(qrCell);
                    }
                    document.Add(metaTable);

                    document.Add(new Paragraph(" ")); // Spacer
                    document.Add(new Paragraph("PASSENGER MANIFEST & SEATING CONFIGURATION", sectionFont));
                    document.Add(new Paragraph("—", textFont));

                    // 3. Passenger List Matrix Table
                    PdfPTable passengerTable = new PdfPTable(4) { WidthPercentage = 100 };
                    passengerTable.SetWidths(new float[] { 2f, 1f, 1f, 2f });

                    // Headers
                    passengerTable.AddCell(new PdfPCell(new Phrase("Passenger Name", boldFont)) { BackgroundColor = BaseColor.LightGray, Padding = 6f });
                    passengerTable.AddCell(new PdfPCell(new Phrase("Age", boldFont)) { BackgroundColor = BaseColor.LightGray, Padding = 6f });
                    passengerTable.AddCell(new PdfPCell(new Phrase("Gender", boldFont)) { BackgroundColor = BaseColor.LightGray, Padding = 6f });
                    passengerTable.AddCell(new PdfPCell(new Phrase("Assigned Seat", boldFont)) { BackgroundColor = BaseColor.LightGray, Padding = 6f });

                    // Dynamically Map Passengers Bound to the Booking Record
                    foreach (var passenger in booking.Passengers)
                    {
                        passengerTable.AddCell(new PdfPCell(new Phrase(passenger.FullName, textFont)) { Padding = 5f });
                        passengerTable.AddCell(new PdfPCell(new Phrase(passenger.Age.ToString(), textFont)) { Padding = 5f });
                        passengerTable.AddCell(new PdfPCell(new Phrase(passenger.Gender, textFont)) { Padding = 5f });
                        passengerTable.AddCell(new PdfPCell(new Phrase(passenger.SeatNumber, boldFont)) { Padding = 5f });
                    }
                    document.Add(passengerTable);

                    document.Add(new Paragraph(" "));

                    // 4. Terms & Conditions Summary Footer
                    PdfPTable footerTable = new PdfPTable(1) { WidthPercentage = 100 };
                    PdfPCell footerCell = new PdfPCell()
                    {
                        BackgroundColor = new BaseColor(245, 245, 245),
                        Padding = 10f,
                        Border = Rectangle.BOX
                    };
                    footerCell.AddElement(new Paragraph("IMPORTANT INFORMATION", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.DarkGray)));
                    footerCell.AddElement(new Paragraph("1. Please check in at least 2 hours prior to standard scheduled domestic flight departures.\n2. Carry a valid government-issued photographic identification marker matching your manifest credentials.\n3. Seating changes, cancellations, or modifications are bound by standard structural airline pricing profiles.", FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.DarkGray)));
                    footerTable.AddCell(footerCell);
                    document.Add(footerTable);

                    document.Close();
                    return memoryStream.ToArray();
                }
            });


        }
    }
}
