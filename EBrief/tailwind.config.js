/** @type {import('tailwindcss').Config} */
const defaultTheme = require("tailwindcss/defaultTheme");

module.exports = {
    content: ["./**/*.{razor,html,cshtml}", "../EBrief.Shared/**/*"],
    theme: {
        extend: {
            colors: {
                primary: "#F0F0F0",
                blue: "#2360DA",
                jade: "#00A767",
                gunmetal: "#272D2D"
            },
            fontFamily: {
                'sans': ['Monserrat', ...defaultTheme.fontFamily.sans]
            }
        },
    },
    plugins: [],
}

