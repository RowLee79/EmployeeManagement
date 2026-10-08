import { chromium } from 'playwright';
import { mkdir } from 'node:fs/promises';
import path from 'node:path';
import { createInterface } from 'node:readline/promises';
import { stdin, stdout } from 'node:process';
import { root, screens, updateGallery } from './gallery.mjs';

const base = new URL(process.argv[2] || 'https://localhost:7029');
if (!['http:', 'https:'].includes(base.protocol)) throw new Error('Use an HTTP or HTTPS application URL.');
const output = path.join(root, 'docs/screenshots');
await mkdir(output, { recursive: true });
const terminal = createInterface({ input: stdin, output: stdout });
let browser;
try {
  browser = await chromium.launch({ headless: process.env.CI === 'true' });
  const context = await browser.newContext({ viewport: { width: 1440, height: 1000 }, ignoreHTTPSErrors: true });
  const page = await context.newPage();
  const captured = [];
  for (const screen of screens) {
    if (screen.name === 'dashboard') {
      if (process.env.UI_CAPTURE_EMAIL && process.env.UI_CAPTURE_PASSWORD) {
        await page.locator('input[name="Email"]').fill(process.env.UI_CAPTURE_EMAIL);
        await page.locator('input[name="Password"]').fill(process.env.UI_CAPTURE_PASSWORD);
        await Promise.all([
          page.waitForURL(url => !url.pathname.toLowerCase().includes('/account/login')),
          page.locator('button[type="submit"]').click()
        ]);
      } else {
        console.log('Sign in as an administrator in the browser. Use your demo database.');
        await terminal.question('After sign-in has completed, return here and press Enter: ');
      }
    }
    const url = new URL(screen.route, base);
    const response = await page.goto(url.href, { waitUntil: 'load' });
    const actual = new URL(page.url());
    if (!response?.ok() || actual.pathname.toLowerCase() !== url.pathname.toLowerCase())
      throw new Error(`Cannot capture ${screen.title}: HTTP ${response?.status()} or redirect to ${actual.pathname}. Check sign-in and application errors.`);
    await page.locator(screen.authenticated === false ? 'input[name="Email"]' : '.main-content').waitFor({ state: 'visible' });
    await page.evaluate(async () => {
      await document.fonts.ready;
      await Promise.all([...document.images].filter(image => !image.complete).map(image =>
        new Promise(resolve => { image.addEventListener('load', resolve, { once: true }); image.addEventListener('error', resolve, { once: true }); })
      ));
    });
    // Allow dashboard chart animation to finish before saving pixels.
    await page.waitForTimeout(1500);
    await page.screenshot({ path: path.join(output, `${screen.name}.png`), fullPage: true, animations: 'disabled' });
    captured.push(screen.title);
    console.log(`Captured: ${screen.title}`);
  }
  const count = await updateGallery();
  console.log(`Done. README includes ${count} screenshots. Review the PNGs before committing.`);
} catch (error) {
  console.error(`Screenshot capture stopped: ${error.message}`);
  console.error('Completed PNGs remain in docs/screenshots. Fix the issue and rerun, or run npm run gallery to include only completed files.');
  process.exitCode = 1;
} finally {
  terminal.close();
  await browser?.close();
}
