import { access, readFile, writeFile } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
export const root = fileURLToPath(new URL('../../', import.meta.url));
export const screens = [
  { name: 'login', title: 'Sign in', route: '/Account/Login', authenticated: false },
  { name: 'dashboard', title: 'Dashboard', route: '/Dashboard/Index' },
  { name: 'employees', title: 'Employee management', route: '/Employees/Index' },
  { name: 'attendance', title: 'Attendance', route: '/Attendance/Index' },
  { name: 'leave', title: 'Leave management', route: '/Leave/Index' },
  { name: 'leave-allocations', title: 'Leave allocations', route: '/LeaveBalances/Index' },
  { name: 'payroll', title: 'Payroll', route: '/Payroll/Index' },
  { name: 'reports', title: 'Employee reports', route: '/Reports/Employees' }
];
export async function updateGallery() {
  const sections = [];
  for (const screen of screens) {
    const relative = `docs/screenshots/${screen.name}.png`;
    try { await access(path.join(root, relative)); }
    catch (error) { if (error.code === 'ENOENT') continue; throw error; }
    sections.push(`### ${screen.title}\n\n![${screen.title}](${relative})`);
  }
  if (!sections.length) throw new Error('No screenshots found. Capture the app UI or add your PNG files first.');
  const readmePath = path.join(root, 'README.md');
  const readme = await readFile(readmePath, 'utf8');
  const start = '<!-- UI_SCREENSHOTS_START -->';
  const end = '<!-- UI_SCREENSHOTS_END -->';
  const a = readme.indexOf(start); const b = readme.indexOf(end);
  if (a < 0 || b <= a) throw new Error('README screenshot markers are missing.');
  await writeFile(readmePath, readme.slice(0, a + start.length) + '\n' + sections.join('\n\n') + '\n' + readme.slice(b));
  return sections.length;
}
