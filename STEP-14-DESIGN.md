# Step 14 implementation notes

Step 14 deliberately avoids adding a managed metadata library or Mono.Cecil. Managed `.dll`/`.exe` candidates are streamed as raw bytes; the CLR metadata root marker `BSJB` distinguishes managed candidates, and selected UTF-8/ASCII metadata strings are matched as compatibility indicators.

This keeps the first compatibility-inventory boundary small and AOT-friendly. It is intentionally less precise than later metadata/IL inspection. Later numbered steps may introduce dedicated metadata/Cecil tooling only after this inventory shows what needs focused analysis.

Native candidates are recognized from Mach-O/fat, ELF and PE/COFF headers plus known native-library/platform paths. No native binary is loaded.
