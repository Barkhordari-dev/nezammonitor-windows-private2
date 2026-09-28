# Changelog — NezamMonitor (Windows)

تمام تغییرات مهم این پروژه در این فایل مستند می‌شود.
فرمت بر پایه [Keep a Changelog](https://keepachangelog.com/) و نسخه‌گذاری بر پایه [Semantic Versioning](https://semver.org/).

## [1.0.0] — 2026-09-28

### Added
- مستندسازی کامل فارسی پروژه (`README.md`) بر اساس سورس واقعی، شامل معماری، جداول دیتابیس، منطق Snapshot/Scan/LastScanResults، منطق واقعی موعد گزارش و ستون «مانده» (X روز مانده / امروز / X روز گذشته)، گروه‌های A و B (بازه ۴ و ۳ ماه)، Generator، Export (Excel/Android/VCF/Backup)، Configuration، Authentication، Networking، Caching و محدودیت‌های Windows.
- بخش «مشخصات لازم برای بازطراحی Android» برای استفاده در Google AI Studio.
- بسته انتشار ویندوز (self-contained) برای `win-x64`:
  - `NezamMonitor-Windows-x64-v1.0.0.zip`
  - ساخته‌شده با .NET SDK `8.0.404` و TargetFramework `net8.0-windows`.

### Notes
- هیچ تغییر در منطق تجاری (business logic) سورس انجام نشده است.
- بسته منتشرشده فاقد داده شخصی، دیتابیس واقعی، فایل VCF، اکسل حاوی اطلاعات شخصی، توکن/رمز و فایل‌های موقت است.

[1.0.0]: https://github.com/Barkhordari-dev/nezammonitor-windows-private2/releases/tag/v1.0.0
