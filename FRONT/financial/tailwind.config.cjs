/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      fontFamily: {
        display: ['SF Pro Display', 'system-ui', '-apple-system', 'sans-serif'],
        text: ['SF Pro Text', 'system-ui', '-apple-system', 'sans-serif'],
        mono: ['SF Mono', 'monospace'],
      },
      colors: {
        primary: '#0066cc',
        'primary-focus': '#0071e3',
        'primary-on-dark': '#2997ff',
        ink: '#1d1d1f',
        'body-muted': '#cccccc',
        'ink-muted-80': '#333333',
        'ink-muted-48': '#7a7a7a',
        'divider-soft': '#f0f0f0',
        hairline: '#e0e0e0',
        'surface-tile-1': '#272729',
        'surface-tile-2': '#2a2a2c',
        'surface-tile-3': '#252527',
        'surface-black': '#000000',
        'surface-chip-translucent': '#d2d2d7',
      },
      spacing: {
        xs: '4px',
        sm: '8px',
        md: '16px',
        lg: '24px',
        xl: '32px',
        xxl: '48px',
      },
      borderRadius: {
        sm: '4px',
        md: '8px',
        lg: '12px',
        xl: '16px',
      },
    },
  },
  plugins: [],
};
