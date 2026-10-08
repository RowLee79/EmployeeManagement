import { updateGallery } from './gallery.mjs';
try { console.log(`README updated with ${await updateGallery()} screenshots.`); }
catch (error) { console.error(error.message); process.exitCode = 1; }
