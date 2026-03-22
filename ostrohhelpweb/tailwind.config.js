/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: 'class',
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          50: '#f0f4ff',
          100: '#e6edff',
          200: '#c7d9ff',
          300: '#a8c5ff',
          400: '#7aa0ff',
          500: '#667eea',
          600: '#5967d8',
          700: '#4c56c7',
          800: '#3f45b6',
          900: '#2d2e7f',
        },
        secondary: {
          50: '#faf5ff',
          100: '#f3e8ff',
          200: '#e9d5ff',
          300: '#d946ef',
          400: '#c026d3',
          500: '#764ba2',
          600: '#6b21a8',
          700: '#581c87',
          800: '#3f0f5c',
          900: '#240a34',
        },
        success: '#10b981',
        warning: '#f59e0b',
        danger: '#ef4444',
        neutral: '#6b7280',
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
      },
      spacing: {
        '128': '32rem',
        '144': '36rem',
      },
    },
  },
  plugins: [],
}
