/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Views/**/*.cshtml",
    "./Pages/**/*.cshtml",
    "./wwwroot/**/*.html"
  ],
  corePlugins: {
    preflight: false, // Disables Tailwind's default CSS resets to prevent conflicts with Bootstrap
  },
  theme: {
    extend: {},
  },
  plugins: [],
}
