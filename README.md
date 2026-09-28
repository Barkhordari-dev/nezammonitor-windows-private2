# NezamMonitor (Windows) — سامانه پایش پرونده‌های نظارت ساختمان

مستندات کامل این مخزن، بر اساس **کد واقعی پروژه** تهیه شده است. هر ادعای این سند از یک فایل و خط مشخص در سورس قابل اثبات است. هیچ منطق تجاری (business logic) برای مستندسازی تغییر داده نشده است.

> این سند برای استفاده در **بازطراحی نسخه اندروید** و بازخوانی در **Google AI Studio** تدوین شده است.

---

## ۱. معماری

نرم‌افزار یک اپلیکیشن دسکتاپ **WPF (.NET 8 / net8.0-windows)** با معماری MVVM است.

ساختار پروژه‌ها (از فایل‌های `.csproj` واقعی):

- `NezamMonitor.App` — لایه رابط کاربری WPF (TargetFramework: `net8.0-windows`، `UseWPF=true`، `OutputType=WinExe`، ارجاع به `CommunityToolkit.Mvvm` نسخه ۸.۴.۲ و `ProjectReference` به Core).
- `NezamMonitor.Core` — منطق دامنه و سرویس‌ها (`net8.0`)، ارجاع به `DocumentFormat.OpenXml` 3.1.0 و `Microsoft.Data.Sqlite` 3.0.1.
- `NezamMonitor.Tests` — تست‌های واحد (`net8.0`).
- `Benchmark` — پروژه بنچمارک (`net8.0`).

لایه‌بندی داخلی Core (از ساختار پوشه‌ها):

- `Api/` — `NezamApiClient.cs`, `ApiExtractor.cs`
- `Data/` — `NezamDatabase.cs`
- `Diff/` — `SnapshotComparer.cs`
- `Sync/` — `SyncResult.cs`
- `Parsing/` — `Parsers.cs`
- `Reports/` — `StageReportGenerator.cs`
- `Excel/` — `ExcelExporter.cs`
- `Export/` — `AndroidExportEngine.cs`, `VCardExporter.cs`, `OwnerVCardExporter.cs`
- `Backup/` — `BackupEngine.cs`
- `Models/` — `CaseModels.cs`
- `Theme/` — `BuiltInThemes.cs`, `ThemeColors.cs`, `ThemeDimensions.cs`
- ابزارهای نرمال‌سازی: `DigitNormalizer.cs`, `TextNormalizer.cs`, `PersianDateHelper.cs`

سرویس‌های لایه App (`Services/`): `DatabaseService.cs`, `ExtractionController.cs`, `NavigationService.cs`, `ThemeManager.cs`

ViewModels (`ViewModels/`): `MainViewModel`, `GeneratorViewModel`, `CasesViewModel`, `ChangesViewModel`, `DashboardViewModel`, `EngineersViewModel`, `ExcelExportViewModel`, `FeesViewModel`, `FollowUpViewModel`, `HistoryViewModel`, `AndroidExportViewModel`, `ReportsViewModel`, `SettingsViewModel`, `UpdateViewModel` به‌همراه `ViewModelBase`, `RelayCommands`, `FilterHelper`, `GeneratorHelpers`, `NavigationCommandHelper`.

Views (`Views/`): `DashboardView`, `CasesView`, `ChangesView`, `EngineersView`, `FeesView`, `ReportsView`, `GeneratorView`, `FollowUpView`, `HistoryView`, `ExcelExportView`, `AndroidExportView`, `SettingsView`, `UpdateView` به‌همراه `WelcomeWindow` و `MainWindow`.

---

## ۲. منوها / نماها (Views)

بر اساس نام فایل‌های Views و ViewModelهای متناظر:

- **داشبورد** (`DashboardView` + `DashboardViewModel`)
- **پرونده‌ها** (`CasesView` + `CasesViewModel`)
- **ناظرین** (`EngineersView` + `EngineersViewModel`)
- **حق‌الزحمه** (`FeesView` + `FeesViewModel`)
- **گزارش‌ها** (`ReportsView` + `ReportsViewModel`)
- **تولید گزارش** (`GeneratorView` + `GeneratorViewModel`)
- **پیگیری** (`FollowUpView` + `FollowUpViewModel`)
- **تغییرات** (`ChangesView` + `ChangesViewModel`)
- **تاریخچه** (`HistoryView` + `HistoryViewModel`)
- **خروجی اکسل** (`ExcelExportView` + `ExcelExportViewModel`)
- **خروجی اندروید** (`AndroidExportView` + `AndroidExportViewModel`)
- **تنظیمات** (`SettingsView` + `SettingsViewModel`)
- **بروزرسانی** (`UpdateView` + `UpdateViewModel`)

---

## ۳. Database

پیاده‌سازی در `NezamMonitor.Core/Data/NezamDatabase.cs` (کلاس `NezamDatabase : IDisposable`، SQLite از طریق `Microsoft.Data.Sqlite`).

در سازنده: مسیر پوشه ساخته می‌شود، اتصال باز می‌شود و `Initialize()` فراخوانی می‌گردد.

جداول (از دستورات `CREATE TABLE IF NOT EXISTS` واقعی):

- **Snapshots**: `Id`, `CreatedAt`, `CaseCount`, `Status` (پیش‌فرض `'pending'`).
- **Cases**: `Id`, `SnapshotId`, `CaseNumber`, `Serial`, `Owner`, `OwnerMobile`, `Responsibility`, `CapacityDate`, `Office`, `ReportDate1`, `ReportDate2`, `ReportDate3`.
- **Specifications**: `Id`, `CaseId`, `BuildingGroup`, `RenovationCode`, `PlanInstructionNo`, `PlanInstructionType`, `PlanInstructionDate`, `StructureType`, `BlockTitle`, `BlockCount`, `Floors`, `Units`, `Issuer`, `PermitNumber`, `PermitDate`, `ReleaseDate`, `LandArea`, `ParafArea`, `Address`, `PlanZone`, `UsageType`, `CapacityArea`.
- **Engineers**: `Id`, `CaseId`, `Discipline`, `Name`, `Role` + (مهاجرت) `Phone`, `DesignLevel`, `SupervisionLevel`, `ExecutionLevel`.
- **Fees**: `Id`, `CaseId`, `Discipline`, `ServiceType`, `Stage`, `StartDate`, `EndDate`, `Amount`, `PayStatus`, `ConfirmStatus`, `AmountType`, `Description`.
- **Reports**: `Id`, `CaseId`, `RowNo`, `ReportType`, `Stage`, `Engineer`, `Discipline`, `VisitDate`, `CeilingCount`, `Indicator`, `HasFile`.
- **Changes**: `Id`, `SnapshotId`, `CaseNumber`, `Owner`, `ChangeType`, `FieldName`, `OldValue`, `NewValue`.
- **Settings**: `Key` (PRIMARY KEY), `Value`.
- **ActiveSnapshot**: `Id` (CHECK Id=1), `SnapshotId`.
- **GeneratedReports**: `Id`, `CaseNumber`, `Stage`, `TemplateName`, `OutputPath`, `CreatedAt`, `Status` (پیش‌فرض `'generated'`), `SentAt`, `OwnerName`, `NumberFormat` + (مهاجرت) `FileExists`, `FolderName`, `ActionLog`, `FileHash`.
- **FollowUpEdits**: `Id`, `CaseNumber` (UNIQUE), `Description`, `CaseNumberEdit`, `OwnerEdit`, `AddressEdit`, `OwnerMobileEdit`, `CreatedAt`, `UpdatedAt`.
- **EngineerRegistry**: `Id`, `FirstName`, `LastName`, `FullName`, `Phone`, `Discipline`, `DesignLevel`, `SupervisionLevel`, `ExecutionLevel`.
- **ScanCache**: `Id`, `ReportKey` (UNIQUE), `HasFile`, `ScanDate`.
- **LastScanResults**: `CaseNumber`, `Stage`, `Owner`, `FullAddress`, `PermitNumber`, `Companion`, `StageName`, `Status`, `GeneratedAt`, `Deadline`, `DaysRemaining`, `DeadlineStatus`, `BuildingGroup`, `HasReport`, `ScanDate` با `UNIQUE(CaseNumber, Stage)`.

مهاجرت‌ها با `ALTER TABLE ... ADD COLUMN` داخل `try/catch` انجام می‌شوند (ستون‌های جدید در صورت نبود اضافه می‌شوند).

مسیر فایل دیتابیس: در `DatabaseService.cs` پوشه `data` کنار فایل اجرایی ساخته می‌شود:
`Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data")`.

---

## ۴. Snapshot

مدل اسنپ‌شات در جدول `Snapshots` با ستون `Status` (`'pending'` هنگام ساخت، `'done'` پس از نهایی‌سازی).

متدهای مرتبط در `NezamDatabase`:

- `CreateSnapshot(DateTime now)` — درج رکورد جدید با وضعیت `'pending'` و بازگرداندن شناسه.
- `FinalizeSnapshot(long id, int caseCount)` — به‌روزرسانی `Status='done'` و ثبت تعداد پرونده.
- `GetLastValidSnapshotId()` — آخرین اسنپ‌شات با `Status='done'`.
- `LoadCases(long snapshotId)` — بارگذاری پرونده‌های یک اسنپ‌شات.

جدول `ActiveSnapshot` (فقط یک ردیف با `Id=1`) اسنپ‌شات فعال را نگه می‌دارد. مقایسه بین اسنپ‌شات‌ها در `NezamMonitor.Core/Diff/SnapshotComparer.cs` انجام می‌شود و نتایج در جدول `Changes` ذخیره می‌گردد.

---

## ۵. Scan

منطق اسکن در `NezamMonitor.App/ViewModels/GeneratorViewModel.cs` متد `Scan()`.

گام‌های واقعی:

1. پاک‌سازی `_allReportItems` و `ReportStatus`، ساخت پوشه `_outputPath`.
2. بارگذاری پرونده‌ها: `_db.LoadCases(_db.GetActiveSnapshotId())`.
3. مراحل ثابت: `var stages = new[] { 1, 2, 3 };`.
4. بارگذاری کش قبلی از دیتابیس (یک‌بار): `_previousScanState = _db.LoadScanCache();` با پرچم `_hasLoadedFromDb`.
5. خواندن گزارش‌های تولیدشده از دیتابیس: `_db.GetGeneratedReports()` و ساخت مجموعه کلید `"{CaseNumber}|{Stage}"` و دیکشنری تاریخ تولید.
6. **اسکن فایل‌های واقعی روی دیسک**: پیمایش پوشه‌های زیر `_outputPath`. نام پوشه به شکل `"01 - نام مالک - 514-1404"` تجزیه می‌شود (`Split(" - ", 3)`)، بخش سوم به `"1404/514"` تبدیل و هم فرمت لاتین و هم فارسی آن ساخته می‌شود؛ سپس برای هر فایل `.docx` اگر نام شامل `"مرحله {نام‌مرحله}"` باشد، کلید در `actualFiles` ثبت می‌شود.
7. برای هر پرونده: محاسبه تعداد مراحل لازم، گروه ساختمانی، تاریخ پروانه و فاصله زمانی (بخش ۷).
8. برای هر مرحله تا `maxStage`: ساخت کلید `reportKey`، تعیین `hasReport` از `actualFiles`، مقایسه با اسکن قبلی برای `isChanged`، محاسبه مهلت/روزهای مانده، و افزودن `CaseReportItem`.
9. ذخیره کش: `_previousScanState = currentScanState; _db.SaveScanCache(currentScanState);`
10. ذخیره نتایج آخرین اسکن: `_db.SaveLastScanResults(scanResults);`

تعیین `maxStage` (تعداد مراحل نیازمند گزارش) بر اساس حق‌الزحمه:

```
var feeStages = c.Fees.Where(f => f.Stage != "0" && f.Stage != "۰")
                       .Select(f => f.Stage)
                       .Distinct()
                       .Count();
var maxStage = feeStages > 0 ? feeStages : 3; // پیش‌فرض ۳ مرحله
```

---

## ۶. LastScanResults

جدول `LastScanResults` **فقط نتیجه آخرین اسکن** را نگه می‌دارد و کلید یکتای آن `UNIQUE(CaseNumber, Stage)` است.

در انتهای `Scan()`، از روی `_allReportItems` یک لیست `LastScanResultItem` ساخته و ذخیره می‌شود. فیلد `HasReport` از متن وضعیت استخراج می‌شود:

```
HasReport = item.Status.Contains("تولید شده")
```

هنگام بارگذاری اولیه (`LoadSavedScanResults`)، اگر `_db.LoadLastScanResults()` خالی نباشد، آیتم‌ها از دیتابیس بازسازی می‌شوند. شماره ردیف (`RowNumber`) با یک نگاشت یک‌باره و بر پایه ترتیب پرونده‌ها محاسبه می‌شود:

```
rowMap[cases[i].CaseNumber] = i + 1; // 1-based, matches CasesView order
```

---

## ۷. Generator و Group A/B و منطق مرحله

### ۷.۱ Group A و ۳ مرحله / Group B و ۴ مرحله

مقادیر واقعی بازه (فاصله) در `GeneratorViewModel.cs`:

```
private int _intervalGroupA = 4; // ماه - گروه الف
private int _intervalGroupB = 3; // ماه - گروه ب
```

این مقادیر در `Settings` با کلیدهای `report_interval_group_a` و `report_interval_group_b` ذخیره می‌شوند.

تعیین گروه از روی `BuildingGroup` انجام می‌شود:

```
var buildingGroup = c.Specification?.BuildingGroup ?? "";
var isGroupB = buildingGroup.Contains("ب");
```

یعنی اگر «گروه ساختمانی» شامل حرف **«ب»** باشد → گروه B (بازه ۳ ماه)، در غیر این صورت → گروه A (بازه ۴ ماه).

تعداد مراحل قابل انجام، از تعداد مراحل متمایز در حق‌الزحمه (`Fees.Stage` بدون `0`) به‌دست می‌آید و در صورت نبود، پیش‌فرض ۳ است (بخش ۵). گروه B با بازه کوتاه‌تر (۳ ماه) می‌تواند مراحل بیشتری (تا ۴) را در همان بازه پروانه پوشش دهد.

### ۷.۲ منطق واقعی موعد گزارش (Deadline)

متد `CalculateDeadline(string permitDate, int stage, int intervalMonths)`:

1. تاریخ پروانه به‌صورت شمسی `YYYY/MM/DD` تجزیه می‌شود.
2. با `PersianCalendar.ToDateTime` به میلادی تبدیل می‌گردد.
3. تعداد ماه‌های افزوده = `intervalMonths * stage`.
4. نتیجه دوباره به شمسی برگردانده و به شکل `YYYY/MM/DD` (ماه با دو رقم) بازگردانده می‌شود.

فرمول: **موعد مرحله n = تاریخ صدور پروانه + (بازه × n) ماه**

### ۷.۳ منطق واقعی ستون «مانده»

متد `CalculateDaysRemaining(string deadline)`:

- تبدیل موعد شمسی به میلادی و محاسبه `diff = (deadlineGregorian - today).Days` با `today = DateTime.Today`.

نتیجه بسته به مقدار `diff`:

- `diff < 0` → `" N روز گذشته"` (N = قدر مطلق اختلاف)
- `diff == 0` → `"⚠️ امروز"`
- `diff <= 7` → `"⚠️ N روز"`
- در غیر این صورت → `" N روز"`

وضعیت مهلت (`GetDeadlineStatus`):

- اگر گزارش تولید شده باشد → `"✅"`
- اگر خالی → `""`
- اگر شامل «گذشته» یا «امروز» → `""`
- اگر عدد روز ≤ ۷ → `"⚠️"`
- در غیر این صورت → `""`

### ۷.۴ Stage / Status / Owner / RowNumber

- **Stage**: `int`؛ نام فارسی با `StageReportRules.StageName(stage)` → ۱=اول، ۲=دوم، ۳=سوم، ۴=چهارم. عدد فارسی با `ToPersianNum` → ۱..۴.
- **Status**: `hasReport ? "✅ تولید شده" : "⏳ در انتظار"`.
- **Owner**: از `c.Owner` (نام مالک).
- **RowNumber**: `GetRowNumber(caseNumber)` = ایندکس پرونده در لیست اسنپ‌شات فعال + ۱؛ اگر پیدا نشود `-1`.

### ۷.۵ قالب و تولید فایل

در `StageReportGenerator` (Core/Reports):

- نام پوشه: `"{row:D2} - {مالک} - {شماره‌پرونده‌معکوس}"`.
- نام فایل: `"گزارش مرحله {نام‌مرحله} - {تاریخ‌شمسی}.docx"` (با پسوند `- LTR` در حالت `useLtr`).
- جانشانی متغیرها با کلیدهایی مانند `«شماره_پرونده»`, `«کد_نوسازی»`, `«تاریخ_گزارش»`, `«مالک»`, `«نوع_کاربری»`, `«ناظر_معماری»`, `«تلفن_ناظر_...»`, `«صلاحیت_ناظر_...»` و ... انجام می‌شود.
- اعتبارسنجی قالب (`ValidateTemplate`) حضور متغیرهای الزامی را بررسی می‌کند: `کد_نوسازی`, `تاریخ_گزارش`, `شماره_پرونده`, `مالک`, `نوع_کاربری`.
- برای تبدیل اعداد به فارسی: `DigitNormalizer.ToPersianDigits(DigitNormalizer.ToAsciiDigits(s))`.
- در حالت LTR، اجزای مسیرهای اسلش‌دار معکوس می‌شوند (`ReverseSlashComponents`).
- انتخاب قالب بر اساس مرحله انجام می‌شود و **در صورت نبود قالب مناسب، بازگشتی (fallback) انجام نمی‌شود** و مقدار `null` برمی‌گردد.

---

## ۸. Export

- **Excel** — `NezamMonitor.Core/Excel/ExcelExporter.cs` (خروجی اکسل از طریق ClosedXML).
- **Android** — `NezamMonitor.Core/Export/AndroidExportEngine.cs`: یک فایل بسته با پسوند `.nzmdata` می‌سازد که در واقع یک **ZIP** شامل `manifest.json` + `data.json` + `metadata.json` و پوشه خالی `attachments/` است.
  - نام فرمت: `NezamMonitor.AndroidData`، نسخه فرمت `1.0`.
  - `manifest` شامل: `format`, `version`, `formatVersion`, `createdAt`, `sourceApplication` (`NezamMonitor Windows`), `sourceApplicationVersion`, `recordCount`, `schemaVersion`, `checksum`, و `exportedEntities` = `["cases","specifications","engineers","fees","reports","followUpEdits"]`.
  - `metadata` شامل: `versionApp`, `versionPlatform` (`Windows`), `exportTime`, `currency` (`IRR`), `dateStandard` (`Persian Solar Hijri`), و شمارش کل `totalCases`, `totalEngineers`, `totalFees`, `totalReports`.
  - داده‌ها با `JsonNamingPolicy.CamelCase` و `WriteIndented=true` و `UnsafeRelaxedJsonEscaping` سریالایز می‌شوند.
- **VCF** — `VCardExporter.cs` و `OwnerVCardExporter.cs` (خروجی مخاطبین).
- **Backup** — `NezamMonitor.Core/Backup/BackupEngine.cs`؛ در `SettingsViewModel` با رمز `"NezamBackup2024"` برای ساخت/بازیابی/ریست پشتیبان فراخوانی می‌شود.

---

## ۹. Configuration

- مسیر **دیتابیس**: کنار فایل اجرایی، زیرپوشه `data`.
- مسیر **قالب‌ها**: `Path.Combine(exeDir, "templates")` که `exeDir = AppDomain.CurrentDomain.BaseDirectory`.
- مسیر **خروجی‌ها**: `Path.Combine(exeDir, "outputs")` (قابل تغییر از دیالوگ انتخاب پوشه).
- مسیر خروجی استخراج از تنظیمات خوانده می‌شود: `db.GetSetting("output_path") ?? ...`.
- تنظیمات در جدول `Settings` به‌صورت key/value ذخیره می‌شوند (نمونه کلیدها: `report_interval_group_a`, `report_interval_group_b`, `output_path`).

---

## ۱۰. Authentication

در `NezamMonitor.Core/Api/NezamApiClient.cs`:

- آدرس پایه: `http://service.yazdnezam.ir:8033`.
- ورود: `POST {BaseUrl}/panel/api/login` با بدنه JSON: `{ ozv_num, ozv_pass, ozv_type = 0 }`.
- توکن از فیلد `token` پاسخ خوانده و در هدر پیش‌فرض `Authorization` قرار می‌گیرد.
- پس از ورود، `LoadUserInfoAsync()` فراخوانی می‌شود: `GET {BaseUrl}/panel/api/user` → خواندن `user.id` و `user.user_shahrestan` (شناسه کاربر و شهرستان).
- وضعیت احراز هویت با `IsAuthenticated` (وجود توکن) مشخص می‌شود.

---

## ۱۱. Networking

متدهای واقعی `NezamApiClient`:

- `GetCasesRawAsync()` → `GET /panel/api/showParvandeNezaratMeybod/{userId}/{cityId}`
- `GetEngineersRawAsync(int dbId)` → `GET /panel/api/showNazer/{dbId}/{cityId}` (پشتیبانی از دو قالب آرایه یا شیء با کلید `dbId`)
- `GetFeesRawAsync(int dbId)` → `POST /panel/api/showMali` با `{ db_id, sha_id, ozv_id }`
- `GetReportsRawAsync(int dbId)` → `POST /panel/api/getGozareshat` با `{ db_id, sha_id }`

مه‌لت `HttpClient` برابر ۳۰ ثانیه است. هدر `User-Agent` پیش‌فرض `Mozilla/5.0 NezamMonitor/1.0` است.

---

## ۱۲. Caching

- **ScanCache** (جدول + حافظه): `Dictionary<string,bool>` با کلید `"{CaseNumber}|{Stage}"`. هنگام اسکن با وضعیت قبلی مقایسه می‌شود و متغیر `isChanged` تعیین می‌گردد. کش جدید با `_db.SaveScanCache(currentScanState)` ذخیره می‌شود.
- **LastScanResults**: فقط آخرین اسکن نگهداری می‌شود و با هر اسکن بازنویسی می‌گردد.

---

## ۱۳. محدودیت‌های Windows

- اپلیکیشن **فقط Windows** است (`net8.0-windows` و `UseWPF=true`).
- مسیرها بر پایه `AppDomain.CurrentDomain.BaseDirectory` هستند و پوشه‌های `data`/`templates`/`outputs` کنار فایل اجرایی ساخته/استفاده می‌شوند.
- انتخاب پوشه/فایل از طریق دیالوگ‌های WPF انجام می‌شود (مثلاً `InitialDirectory` برای قالب و خروجی).
- سرویس BackEnd روی `http://` (بدون TLS) در پورت `8033` می‌باشد؛ برای اندروید باید سیاست cleartext و دسترسی شبکه لحاظ شود.
- فایل‌های Word (`.docx`) با `DocumentFormat.OpenXml` تولید/ویرایش می‌شوند؛ این وابستگی صرفاً دسکتاپ است.
- SQLite از `Microsoft.Data.Sqlite` استفاده می‌کند (پوشه `data` محلی).

---

## ۱۴. مشخصات لازم برای بازطراحی Android

این بخش دقیقاً از کد و الگوهای داده‌ای موجود استخراج شده است.

### ۱۴.۱ مدل داده (نگاشت از Core)

- **Case / پرونده**: `CaseNumber`, `Serial`, `Owner`, `OwnerMobile`, `Responsibility`, `CapacityDate`, `Office`, `ReportDate1..3`, `OwnerFather`, `OwnerNationalCode`, `OwnerAddress`, `OwnerZip`, `OwnerTel`, `OwnerBirthLoc` + `Specification`, `Engineers`, `Fees`, `Reports`. کلید پایدار: `CaseKey(CaseNumber, Serial)`.
- **CaseSpecification / مشخصات بلوک**: `BuildingGroup`, `RenovationCode`, `PlanInstructionNo`, `PlanInstructionType`, `PlanInstructionDate`, `LandArea`, `ParafArea`, `CapacityArea`, `StructureType`, `BlockTitle`, `BlockCount`, `Floors`, `Units`, `Issuer`, `PermitNumber`, `PermitDate`, `ReleaseDate`, `PlanZone`, `Address`, `UsageType`.
- **Engineer / ناظر**: `Discipline`, `Name`, `Role`, `Phone`, `DesignLevel`, `SupervisionLevel`, `ExecutionLevel`. کلید یکتای منطقی: `Normalize(Discipline)|Normalize(Name)|Normalize(Role)`.
- **Fee / حق‌الزحمه**: `Discipline`, `ServiceType`, `Stage`, `StartDate`, `EndDate`, `Amount`, `PayStatus`, `ConfirmStatus`, `AmountType`, `Description`.
- **ReportRecord / گزارش**: `RowNo`, `ReportType`, `Stage`, `Engineer`, `Discipline`, `VisitDate`, `CeilingCount`, `Indicator`, `HasFile`.

### ۱۴.۲ APIها (قابل مصرف مستقیم در اندروید)

- `POST /panel/api/login` → `{ ozv_num, ozv_pass, ozv_type:0 }` → `token`
- `GET /panel/api/user` → `user.id`, `user.user_shahrestan`
- `GET /panel/api/showParvandeNezaratMeybod/{userId}/{cityId}`
- `GET /panel/api/showNazer/{dbId}/{cityId}`
- `POST /panel/api/showMali` → `{ db_id, sha_id, ozv_id }`
- `POST /panel/api/gozareshat` → `POST /panel/api/getGozareshat` → `{ db_id, sha_id }`
- Base URL: `http://service.yazdnezam.ir:8033`

### ۱۴.۳ منطق مهلت و «مانده»

- موعد مرحله n = تاریخ پروانه شمسی + (بازه × n) ماه؛ بازه گروه A = ۴، گروه B = ۳؛ تشخیص گروه از وجود حرف «ب» در `BuildingGroup`.
- «مانده»: اختلاف روز تا موعد — گذشته `" N روز گذشته"`، صفر `"⚠️ امروز"`، ≤۷ `"⚠️ N روز"`، سایر `" N روز"`.

### ۱۴.۴ بسته داده تبادلی

- فرمت `.nzmdata` (ZIP) با `manifest.json` + `data.json` + `metadata.json`؛ `dataStandard = Persian Solar Hijri`، `currency = IRR`، `entities = cases, specifications, engineers, fees, reports, followUpEdits`.

### ۱۴.۵ نکات پیاده‌سازی

- نرمال‌سازی متن فارسی: یکسان‌سازی ی/ک عربی و تبدیل ارقام لاتین/فارسی (کلاس‌های `TextNormalizer`, `DigitNormalizer`).
- تاریخ شمسی: نگاشت با `PersianCalendar` (`PersianDateHelper`).
- تفاوت‌ها: ذخیره اسنپ‌شات و مقایسه برای «تغییرات» (معادل `SnapshotComparer` و جدول `Changes`).
- گزارش‌ها به‌صورت فایل `.docx` تولید می‌شوند؛ در اندروید معادل آن تولید PDF/متن یا فایل Word سازگار لازم است.
- احراز هویت توکنی و هدر `Authorization` برای همه درخواست‌های پس از ورود لازم است.

---

## وضعیت مخزن

این مخزن در حال حاضر فقط شامل زیرمجموعه‌ای از سورس (لایه `NezamMonitor.App`، یعنی فایل‌های WPF، Viewها، ViewModelها و سرویس‌ها) به‌همراه `app.ico` و `app_logo.png` است و پروژه‌های `NezamMonitor.Core`، `NezamMonitor.Tests`، `Benchmark` و فایل `.sln` در آن موجود نیستند.

نسخه نرم‌افزار در `NezamMonitor.App.csproj` طبق فایل پروژه ثبت شده است.
