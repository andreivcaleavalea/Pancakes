#!/usr/bin/env node

/**
 * Simple performance testing script
 * Run with: node scripts/test-performance.js
 */

import { execSync } from "child_process";
import fs from "fs";
import path from "path";
import { fileURLToPath } from "url";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

console.log("🚀 Performance Testing Script");
console.log("=============================\n");

// Check if we're in the frontend directory
const packageJsonPath = path.join(process.cwd(), "package.json");
if (!fs.existsSync(packageJsonPath)) {
  console.error("❌ Please run this script from the frontend directory");
  process.exit(1);
}

try {
  console.log("📦 Building production bundle...");
  execSync("npm run build", { stdio: "inherit" });

  console.log("\n📊 Analyzing bundle size...");

  // Get build directory info
  const distPath = path.join(process.cwd(), "dist");
  if (fs.existsSync(distPath)) {
    const getDirectorySize = (dirPath) => {
      let totalSize = 0;
      const files = fs.readdirSync(dirPath);

      files.forEach((file) => {
        const filePath = path.join(dirPath, file);
        const stats = fs.statSync(filePath);

        if (stats.isDirectory()) {
          totalSize += getDirectorySize(filePath);
        } else {
          totalSize += stats.size;
        }
      });

      return totalSize;
    };

    const formatBytes = (bytes) => {
      if (bytes === 0) return "0 Bytes";
      const k = 1024;
      const sizes = ["Bytes", "KB", "MB", "GB"];
      const i = Math.floor(Math.log(bytes) / Math.log(k));
      return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + " " + sizes[i];
    };

    const totalSize = getDirectorySize(distPath);
    console.log(`📁 Total bundle size: ${formatBytes(totalSize)}`);

    // List JS chunk sizes
    const assetsPath = path.join(distPath, "assets");
    if (fs.existsSync(assetsPath)) {
      console.log("\n📄 JavaScript chunks:");
      const files = fs.readdirSync(assetsPath);
      const jsFiles = files.filter((file) => file.endsWith(".js")).sort();

      jsFiles.forEach((file) => {
        const filePath = path.join(assetsPath, file);
        const stats = fs.statSync(filePath);
        console.log(`  • ${file}: ${formatBytes(stats.size)}`);
      });
    }
  }

  console.log("\n✅ Build completed successfully!");
  console.log("\n🔧 Next steps:");
  console.log('1. Run "npm run preview" to test the production build');
  console.log("2. Open DevTools → Network tab to see chunk loading");
  console.log("3. Open DevTools → Performance tab to measure loading times");
  console.log("4. Test image lazy loading by scrolling slowly");
  console.log("5. Check console for performance monitoring logs");
} catch (error) {
  console.error("❌ Build failed:", error.message);
  process.exit(1);
}
