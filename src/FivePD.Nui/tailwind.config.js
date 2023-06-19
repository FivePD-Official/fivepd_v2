/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './src/**/*.{html,ts}',
  ],
  theme: {
    extend: {
      fontFamily: {
        sans: ['Satoshi'],
      },
      colors: {
        primary: '#178EE9'
      },
      animation: {
        slideIn: 'slideIn .2s forwards',
        slideOut: 'slideOut .2s forwards',
      },
      keyframes: {
        slideIn: {
          '0%': {transform: 'translateX(100%)'},
          '100%': {transform: 'translateX(0)'},
        },
        slideOut: {
          '0%': {transform: 'translateX(0)'},
          '100%': {transform: 'translateX(100%)'},
        }
      }
    },
    screens: {
      'fhd': '1920px',
    }
  },
  plugins: [],
}
