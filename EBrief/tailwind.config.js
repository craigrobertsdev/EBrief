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
                jade: "#04b33b",
                "jade-hover": "#02a134",
                turquoise: "#028299",
                "turquoise-hover": "#017185",
                ceramic: "#3d0101",
                "ceramic-hover": "#A84F42",
                peach: "#DD8E75",
                "peach-hover": "#C97C5D",
                gunmetal: "#272D2D",
                disabled: "#F2EDE9",
                text: "#DDD"
            },
            fontFamily: {
                'sans': ['Monserrat', ...defaultTheme.fontFamily.sans]
            },
            boxShadow: {
                'menu': "0px 8px 16px 0px rgba(0,0,0,0.2)"
            },
            screens: {
                'laptop': '1360px'
            },
            fontSize: {
                'md': ['15px', '22px']
            }
        },
    },
    plugins: [],
}

