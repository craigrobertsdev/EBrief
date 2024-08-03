/** @type {import('tailwindcss').Config} */
const defaultTheme = require("tailwindcss/defaultTheme");

module.exports = {
    content: ["./**/*.{razor,html,cshtml}", "../EBrief.Shared/**/*"],
    theme: {
        extend: {
            colors: {
                primary: "#F0F0F0",
                blue: "#2360DA",
                "blue-hover": "#184cb5",
                jade: "#016e44",
                "jade-hover": "#005736",
                turquoise: "#028299",
                "turquoise-hover": "#017185",
                ceramic: "#3d0101",
                "ceramic-hover": "#A84F42",
                peach: "#DD8E75",
                "peach-hover": "#C97C5D",
                gunmetal: "#272D2D",
                disabled: "#F2EDE9"
            },
            fontFamily: {
                'sans': ['Monserrat', ...defaultTheme.fontFamily.sans]
            }
        },
    },
    plugins: [],
}

