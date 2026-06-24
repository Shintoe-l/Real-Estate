const fs = require('fs');
const path = require('path');

const sourceDir = 'C:\\Users\\Shintoe\\.gemini\\antigravity\\brain\\dafabc1a-da66-42d8-a9d3-747a5613d41f';
const destDir = path.join(__dirname, 'frontend', 'public', 'assets');

if (!fs.existsSync(destDir)) {
    fs.mkdirSync(destDir, { recursive: true });
}

const villaSource = path.join(sourceDir, 'villa_sunset_1779187122092.png');
if (fs.existsSync(villaSource)) {
    fs.copyFileSync(villaSource, path.join(destDir, 'villa_sunset.png'));
    console.log('Copied villa_sunset.png');
}

const logoSource = path.join(sourceDir, 'logo_real_estate_1779187195661.png');
if (fs.existsSync(logoSource)) {
    fs.copyFileSync(logoSource, path.join(destDir, 'logo.png'));
    console.log('Copied logo.png');
}
