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
        modalCenter: 'modalCenter .2s forwards',
        modalSide: 'modalSide .2s forwards',
        fadeIn: 'fadeIn .2s forwards',
      },
      keyframes: {
        slideIn: {
          '0%': {transform: 'translateX(100%)'},
          '100%': {transform: 'translateX(0)'},
        },
        slideOut: {
          '0%': {transform: 'translateX(0)'},
          '100%': {transform: 'translateX(100%)'},
        },
        modalCenter: {
          '0%': {
            opacity: 80,
            transform: 'scale(0.95)',
          },
          '100%': {
            opacity: 100,
            transform: 'scale(1)',
          },
        },
        modalSide: {
          '0%': {
            transform: 'translateX(100%)',
          },
          '100%': {
            transform: 'translateX(0)',
          },
        },
        fadeIn: {
          '0%': {
            opacity: 0,
          },
          '100%': {
            opacity: 100,
          },
        },
      }
    },
    screens: {
      'fhd': '1920px',
    }
  },
  plugins: [],
}
