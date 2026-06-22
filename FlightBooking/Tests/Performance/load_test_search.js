// Tests/Performance/load_test_search.js
import http from 'k6/http';
import { check, sleep } from 'k6';

// 1. Configure Execution Options & Ramping Virtual User Profiles
export const options = {
    insecureSkipTLSVerify: true, // Bypass SSL certificate warnings for localhost HTTPS
    stages: [
        { duration: '10s', target: 5 },  // Ramp up from 0 to 5 virtual users concurrently
        { duration: '30s', target: 15 },   // Saturate load further up to 15 users
        { duration: '10s', target: 0 },   // Gracefully ramp down back to zero connections
    ],
    thresholds: {
        http_req_failed: ['rate<0.01'],   // Error tolerance rate constraint threshold: Must stay under 1%
        http_req_duration: ['p(95)<500'], // Performance SLA threshold: 95% of requests must process in under 500ms
    },
};

// 2. Define Execution Operation Steps Loop
export default function () {
    // HTTPS endpoint configured in launchSettings.json (port 7178)
    const baseUrl = 'https://localhost:7178/Flight/Search';

    // Simulate query parameter tokens matching active seed items inside local SQL instance (e.g. From COK=1 to DEL=2)
    const queryParams = '?FromAirportId=1&ToAirportId=2&DepartureDate=2026-06-22&Passengers=1&JourneyClass=Economy';
    const fullTargetUrl = `${baseUrl}${queryParams}`;

    // Dispatch an automated HTTP GET transaction frame
    const response = http.get(fullTargetUrl);

    // 3. Evaluate Framework Behavior Health Rules Assertions
    check(response, {
        'status verification passed (HTTP 200)': (r) => r.status === 200,
        'page data envelope verified (HTML present)': (r) => r.body.includes('Search results') || r.body.includes('flight'),
    });

    // Pause briefly for 1 second before re-executing loop iterations to mimic normal human interaction typing delays
    sleep(1);
}