# Penjelasan Code Hari 2

Berikut adalah penjelasan per blok dan baris kode untuk file-file utama (`Program.cs` dan `ProductsController.cs`). Disertai dengan cuplikan kodenya agar urutan alurnya lebih mudah dipahami.

---

## 1. Penjelasan File: `Program.cs`

File ini adalah *entry point* (titik awal) aplikasi berjalan. Di sini kita memuat variabel lingkungan, mengatur Dependency Injection, serta mengonfigurasi alur web (Middleware pipeline).

### Inisialisasi & Setup Awal
```csharp
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;

// Load environment variables dari file .env
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);
```
- **Baris 1-2 (`using ...`)**: Mengimpor modul library Entity Framework Core dan namespace tempat koneksi Data lokal (seperti `AppDbContext`) kita berada.
- **Baris 5 (`DotNetEnv.Env.Load()`)**: Menginstruksikan aplikasi membaca file `.env` di latar belakang, sehingga variabel kredensial (misal: password dan pengaturan root database) bisa dimuat ke app secara aman.
- **Baris 7 (`var builder ...`)**: Membuat sebuah pola konfigurasi bernama `builder` yang berfungsi untuk merangkai daftar *layanan (services)* yang bisa dipakai API aplikasi kita.

### Mendaftarkan Layanan (Services)
```csharp
// 1. Tambahkan Controllers
builder.Services.AddControllers();

// 2. Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
```
- **Baris 12 (`AddControllers()`)**: Mengaktifkan modul Controller agar `.NET` mengerti dari mana rute berasal, ini vital untuk API berbasis Controller seperti file kita (`ProductsController`).
- **Baris 15-16**: Mengaplikasikan fitur *Swagger*. Fitur ini akan otomatis men-*generate* laman UI agar semua spesifikasi JSON endpoint kita terdokumentasi dan dapat diuji langsung dari browser.

### Konfigurasi Database (PostgreSQL)
```csharp
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") 
                       ?? builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
```
- **Baris 20-21**: Berusaha menarik string koneksi akses *Connection String* dari *environment variables*. Tanda `??` (*Null-coalescing*) memberi pengecualian: "Bila kosong (`null`), baru ambil string koneksi dari file *appsettings.json* ".
- **Baris 23-24 (`AddDbContext<AppDbContext>`)**: Memasukkan `AppDbContext` secara global via koneksi Dependency Injection. Tujuannya supaya *Controllers* kita mudah memanggil DB. `UseNpgsql(...)` mendefinisikan secara pasti bahwa aplikasi berikatan ke *driver* spesifik PostgreSQL.

### Meluncurkan Middleware & Menyalakan App
```csharp
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
```
- **Baris 26 (`builder.Build()`)**: Mengunci status modifikasi *builder* services dan mencetak hasil jadiannya yakni objek implementasi bernama `app`.
- **Baris 31-35**: Mengamankan aplikasi dan mengevaluasi mode jalannya. Jika API dieksekusi di server lokal development untuk di-coding, maka sediakan fitur halaman antar-muka *Swagger*. Ia dinon-aktifkan bila server online / Production untuk alasan privasi publik.
- **Baris 38 (`UseAuthorization()`)**: Menyusun filter layer autentikasi akses dalam request pipeline.
- **Baris 41 (`MapControllers()`)**: Melacak rute API HTTP dinamis (e.g., `/api/products`) untuk masuk ke Class Controller masing-masing.
- **Baris 43 (`app.Run()`)**: Kestrel (Web Server default milik .NET) dihidupkan, diam menunggu interaksi _Request_ web dari Client tanpa berhenti.

---

## 2. Penjelasan File: `Controllers/ProductsController.cs`

File ini di khususkan mengelola seluruh lalu lintas request pada objek / entitas *Product*. Desainnya berpola API RESTful klasik (C-R-U-D).

### Atribut Class API & Inisiasi Database Koneksi
```csharp
using Microsoft.AspNetCore.Mvc;
using ProductApi.Data;
using ProductApi.Models;

namespace ProductApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }
```
- **Baris 1-3**: Mengimpor struktur MVC di `Microsoft.AspNetCore`, Folder Database Models serta akses folder koneksi DB `AppDbContext`.
- **Baris 7 (`[ApiController]`)**: Menandakan Class C# ini menaati tingkah laku *Web API*, yaitu memiliki pengecekan form Payload otomatis (menyemburkan status gagal Bad Request [400] kalau parameter / isian JSON nya cacat).
- **Baris 8 (`[Route("api/[controller]")]`)**: Sebuah magic string *Route* dinamis dari framework yang melekat di ujung url. Di mana kata parameter `[controller]` mengambil label awalan class `Products`, Jadinya `/api/Products`.
- **Baris 11 (`_context`)**: Wadah objek data lokal (`AppDbContext`) dibuat read-only supaya dijamin murni, tidak tereksekusi tanpa sengaja.
- **Baris 13-16 (Constructors)**: Pada saat controller diakses dan terbangun, argumen `context` koneksi database akan dikirim masuk ke wadah `_context` secara ajaib karena `Dependency Injector` telah disetel di `Program.cs` tadi.

---

### Metoda REST: 1. HTTP GET (Banyak Data & Pencarian)
```csharp
[HttpGet]
public IActionResult Get([FromQuery] string? search, [FromQuery] decimal? minPrice, 
                         [FromQuery] decimal? maxPrice, [FromQuery] int page = 1, 
                         [FromQuery] int pageSize = 10)
{
    var query = _context.Products.AsQueryable();

    if (!string.IsNullOrWhiteSpace(search))
        query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));

    if (minPrice.HasValue)
        query = query.Where(p => p.Price >= minPrice.Value);

    if (maxPrice.HasValue)
        query = query.Where(p => p.Price <= maxPrice.Value);
```
- **Baris 23-30**: Menangani HTTP GET `/api/products`. Di sini ada label argumen parameter bernama `[FromQuery]`, yang tugasnya mencegat teks *Query Parameter* setelah penunjuk tanda tanya rute (Misal:  `?search=kabel&minprice=25000&pageSize=10`). 
- **Baris 31 (`AsQueryable()`)**: Ia melatih jalur eksekusi antrian EntityFramework ini untuk ditambal oleh deretan filter WHERE tanpa eksekusi mutlak dulu (`Lazy loading`).
- **Baris 34-48**: Fungsi opsional yang menjaring logika argumen. Contoh: kalau isi `search` *gak null*, rangkai modifikasi SQL agar mensyaratkan kolom String `Name` cocok dengan rentang pencarian huruf kecil `.ToLower()`. Berlaku juga untuk filter harga `minPrice` dan `maxPrice`.

```csharp
    var totalItems = query.Count();
    var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

    var products = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

    return Ok(new { data = products, pagination = new { currentPage = page, ... } });
}
```
- **Baris 51-52**: Menjalankan eksekusi query SQL mentah `SELECT COUNT(*)` terhadap *Product* ke Postgre untuk laporan total baris, yang lalu dibagi dengan *pageSize* terus dibulatkan ke atas untuk dapat angka total keseluruhan halaman _pagination_ (`Math.Ceiling`).
- **Baris 55-58 (`Skip...Take...ToList`)**: Menerapkan paging dengan `Skip()` (Melompati daftar rentang sebelumnya) dan `Take()` (Sajikan sebanyak `pageSize` limitnya). Dan ditutup `.ToList()` - pemantik utamanya yang mentranskripsi barisan *Query Builder* ini jadi tarikan data array fisik ke memori aplikasi.
- **Baris 61-71**: Mem-parsing dan *me-return* balasan standar kode server sukses (Status `200 OK`) bermuatan object gabungan tipe JSON; Ada entitas `data` (segenap baris Produk) dan informasi `pagination`.

---

### Metoda REST: 2. HTTP GET ID (Satu Item)
```csharp
[HttpGet("{id}")]
public IActionResult GetById(int id)
{
    var product = _context.Products.Find(id);

    if (product == null)
        return NotFound(new { message = $"Product dengan ID {id} tidak ditemukan" });

    return Ok(product);
}
```
- **Baris 78 (`"{id}"`)**: Menyematkan tangkapan rentang rute ID di buntut URL layaknya  `/api/products/7` untuk merujuk parameter `id` (value-nya 7).
- **Baris 81 (`Find(id)`)**: Metode khusus instan pada ORM / _Entity Framework Core_ untuk mencari baris referensi via satu nilai `Primary Key`.
- **Baris 83-84 (`NotFound(...)`)**: Jika `Find` tak menghasilkan _object referensi_ apapun dari row di database (`null`), lontarkan response kosong tipe _error_  `404 Not Found`.

---

### Metoda REST: 3. HTTP POST (Menambahkan Item Baru)
```csharp
[HttpPost]
public IActionResult Create(Product product)
{
    if (!ModelState.IsValid) return BadRequest(ModelState);

    _context.Products.Add(product);
    _context.SaveChanges();

    return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
}
```
- **Baris 94 (`Create(Product product)`)**: Sistem Model Binder mencocokkan kiriman teks format JSON (*Request Body*) menjadi object turunan bahasa C# `Product` dan dialiri dalam parameter metoda aksi ini.
- **Baris 96**: Blok pengaman lapis dua `ModelState.IsValid` (Meski `[ApiController]` sudah mengatasinya di latar) kalau misal input data harganya salah ketik pakai Huruf, ia mengembalikan balasan `Error 400 Bad Request`.
- **Baris 99 (`Add(product)`)**: Entitas dimasukkan / diagendakan di *tracker* memori `EF Core` secara sepihak buat di-"INSERT"-kan kelak.
- **Baris 100 (`SaveChanges()`)**: Operasi *Syncing* Data ke PostgreSQL secara pasti. Barulah SQL murni Query eksekusi menempel di Database.
- **Baris 102 (`CreatedAtAction`)**: Standar *Best Practice* pengembalian Data Rest API - Menerbitkan status kembalian tipe *"201 - Created"*, rute perujukan buat me _review_ Action metode `GetById`, sambil melempar Object Model yang baru dibuat di body Respons.

---

### Metoda REST: 4. HTTP PUT (Mengganti Item Lama Penuh)
```csharp
[HttpPut("{id}")]
public IActionResult Update(int id, Product product)
{
    var data = _context.Products.Find(id);

    if (data == null)
        return NotFound(new { message = $"Product dengan ID {id} tidak ditemukan" });

    data.Name = product.Name;
    data.Price = product.Price;
    _context.SaveChanges();

    return Ok(data);
}
```
- **Baris 109**: Rute metode tangkapan buat request tipe PUT khusus `id` tertentu.
- **Baris 115**: Lakukan pengecekkan referensi ke db `Find(id)`. 
- **Baris 120-121**: Menempel / menimpakan modifikasi kolom baru (`Name` dan `Price` dari payload JSON argumen `product`) pada variable representasi (`data`) database internal. 
- **Baris 122 (`SaveChanges()`)**: Setelah itu perubahan disimpan selamanya di DB memakai query *`UPDATE Products...`*.
- **Baris 124 (`Ok(data)`)**: Lapor ke client dengan response OK (Status 200 HTTP) memuat utuh obyek anyar ini.

---

### Metoda REST: 5. HTTP DELETE (Menghapus Data)
```csharp
[HttpDelete("{id}")]
public IActionResult Delete(int id)
{
    var data = _context.Products.Find(id);

    if (data == null)
        return NotFound(new { message = $"Product dengan ID {id} tidak ditemukan" });

    _context.Products.Remove(data);
    _context.SaveChanges();

    return Ok(new { message = $"Product '{data.Name}' berhasil dihapus" });
}
```
- **Baris 131**: Spesifik endpoint bertugas dalam merespon HTTP DELETE yang masuk.
- **Baris 134-137**: Cari *existence* entitasnya (buat dijaga dari resiko error ID invalid); kembali `404 Not Found` ketika nihil target.
- **Baris 139 (`Remove(data)`)**: Tandai status baris entitasnya ke `EF Core` buat di-"*DROP / REMOVE*".
- **Baris 140**: Sinkronisasikan eksekusi perintah ke postgre *`DELETE FROM Products...`*.
- **Baris 142**: Informasikan kelancaran penghapusan file lewat balasan Status kembalian OK (200), terbalut bersama sebuah respon JSON message konfirmasi kustom.
