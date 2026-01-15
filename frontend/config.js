// API Configuration
const CONFIG = {
    API_BASE_URL: window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1'
        ? 'http://localhost:5000'  // Development
        : 'https://trivia-api-quad-giepie-cbfjg6cxcyhcgnca.westeurope-01.azurewebsites.net'  // Production
};
