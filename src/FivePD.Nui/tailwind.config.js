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
    },
  },
  plugins: [],
}
