# Penjelasan Code Hari 2

Berikut adalah penjelasan per baris kode untuk file-file utama di project Anda (`Program.cs` dan `ProductsController.cs`):

---

## 1. Penjelasan File: `Program.cs`

`Program.cs` adalah titik awal (entry point) aplikasi ASP.NET Core saat dijalankan. Di sini kita men-setup Dependency Injection, mengkonfigurasi pipeline HTTP (Middleware), serta menjalankan server kestrel bawaan .NET.

### Detail Per Line:
- **Baris 1-2 (`using ...`)**: Mengimpor ruang lingkup kode (namespace) yang dibutuhkan (berisi library EntityFrameworkCore dan Data Model lokal).
- **Baris 5 (`DotNetEnv.Env.Load();`)**: Membaca file `.env` untuk mengambil konfigurasi kredensial environment variables (seperti connection string) secara rahasia.
- **Baris 7 (`var builder = WebApplication.CreateBuilder(args);`)**: Membuat pola `builder` di mana kita bisa mendaftarkan kumpulan layanan internal (Services/DI).
- **Baris 12 (`builder.Services.AddControllers();`)**: Memerintahkan web-builder untuk mengaktifkan fitur API Controller yang akan me-routing Request kepada `[ApiController]`.
- **Baris 15-16 (`AddEndpointsApiExplorer` & `AddSwaggerGen`)**: Mengaktifkan Swagger (alat UI untuk ngetes API secara langsung lewat browser sebagai web otomatis dokumentasi API OpenAPI).
- **Baris 20-21 (`var connectionString = ...`)**: Mencari tahu akses Database. Pertama coba ambil prioritas tertinggi dari Environment Variable `"CONNECTION_STRING"`. Jika kosong / gagal, coba fallback ambil dari profil `Default` di dalam `appsettings.json`.
- **Baris 23-24 (`builder.Services.AddDbContext...`)**: Mendaftarkan _Database Context_ `AppDbContext` secara global agar bisa dipanggil ke Controllers. Parameter `UseNpgsql` menjelaskan bahwa framework EntityFramework kita terhubung secara native dengan bahasa query database PostgreSQL.
- **Baris 26 (`var app = builder.Build();`)**: Menutup pembungkusan dari `builder` Service menjadi `app` Middleware web pipeline. Segala yang dieksekusi di bawahnya adalah layer / middleware web HTTP aplikasi.
- **Baris 31-35 (`if (app.Environment.IsDevelopment()) ...`)**: Mengecek mode eksekusi env lokal (Development). Jika benar, ia me-response dengan fitur UI Swagger, sedangkan di produksi (Production/Release) akan disembunyikan demi keamanan.
- **Baris 38 (`app.UseAuthorization();`)**: Menginformasikan agar lalu lintas web yang lewat dilewatkan pada modul Otorisasi (cek ijin autentikasi).
- **Baris 41 (`app.MapControllers();`)**: Menerjemahkan setiap URL rute yang ada (misalnya `/api/products`) ke spesifik Controller bersangkutan.
- **Baris 43 (`app.Run();`)**: Menjalankan host server dan me-*listen* / diam-diam menunggu merespons request client tanpa henti.

---

## 2. Penjelasan File: `Controllers/ProductsController.cs`

File ini mengelola bagaimana *resource* Produk diproses sebagai API. Menggunakan paradigma RESTful (Get, Post, Put, Delete).

### Detail Per Line:
- **Baris 1-3 (`using ...`)**: Impor dependensi dari Microsoft API serta namespace internal untuk mengakses Data (`AppDbContext`) dan Tabel Relasional Model (`Product`).
- **Baris 7 (`[ApiController]`)**: Atribut dasar memberitahu ke .NET bahwa class ini bukan merender tampilan website melainkan data JSON API. Ini memberikan keuntungan seperti HTTP Bad Request validation 400 otomatis.
- **Baris 8 (`[Route("api/[controller]")]`)**: Variabel rute dasar yang men-transformasi path URL nama controller ini: `ProductsController` sehingga endpoint utamanya adalah `/api/products`.
- **Baris 9 (`public class ProductsController : ControllerBase`)**: Deklarasi class `ProductsController` yang inheritensi keturunan khusus dari ASP.NET `ControllerBase`.
- **Baris 11 (`private readonly AppDbContext _context;`)**: Menyediakan kotak _field_ penyimpanan lokal ke akses Database yang bersifat read-only setelah diisi.
- **Baris 13-16 (`public ProductsController(...)`)**: Fungsi Konstruktor; ketika controller ini terpanggil oleh HTTP, Dependency Injector ASP.NET core otomatis akan mendistribusikan / menginfus object koneksi database (`context`) lewat sini untuk disimpan.
- **Baris 23 (`[HttpGet]`)**: Fungsi di bawahnya berfungsi untuk menerima permintaan `GET HTTP` (membaca data).
- **Baris 24-30 (`public IActionResult Get(...)`)**: Method API rute baca yang menangkap berbagai parameter kueri dari URL yaitu `search`, filter harga minimal/maksimal, serta sistem pagination/halaman (`page` & `pageSize`). Semua parameter tersebut diberi atribut parameter bawaan `[FromQuery]`.
- **Baris 31 (`var query = _context.Products.AsQueryable();`)**: Membuka jalur inisiasi akses tabel (`AsQueryable` memungkinkan modifikasi perintah logika WHERE berulang-ulang tanpa mengeksekusi pengambilan Query ke Database secara permanen dulu).
- **Baris 34-37 (`if (!string.IsNullOrWhiteSpace(search)) ...`)**: Menambah modifikasi filter logika WHERE apabila ada teks spesifik yang dikirim lewat link, berupa pembandingan huruf kecil string (`.ToLower().Contains(...)`) alias pencarian kata kunci yang tidak sensitif huruf di kolom "Name".
- **Baris 40-48**: Pengecekan filter "MinPrice" & "MaxPrice" guna membombardir `query` bertahap menyaring Harga dari minimal sampai maksimal jikalau argument angka validnya tidak Null (HasValue bernilai benar).
- **Baris 51-52 (`... totalItems = query.Count()`)**: Langsung mengeksekusi Query Count PostgreSQL guna mencari jumlah baris tabel secara real-time untuk data laporan metadata jumlah seluruh pagination. Serta menghitung total halaman utuh (`totalPages`).
- **Baris 55-58 (`var products = query...`)**: Melakukan Pagination data. `.Skip(...)` berfungsi meloncat membuang deretan angka indeks sebanyak list yg ada lalu `.Take(...)` mengambil hanya _range_ porsinya saja. Kemudian `.ToList()` mengeksekusi akhir di Database dan hasilnya disimpan pada array list lokal aplikasi.
- **Baris 61-71 (`return Ok(...)`)**: Kirim data keluar menuju klien Internet dengan respons Header standard 200 OK berserta serapan output Format JSON gabungan struktur array produk dan data perhitungan metadata pagination.
- **Baris 78-79 (`[HttpGet("{id}")]`)**: Merespon _request endpoint route_ URL Get Detail yang berformat sisipan dinamis misal : `/api/products/1` (menggantikan ID).
- **Baris 81 (`_context.Products.Find(id)`)**: Mencari tunggal objek produk dalam Database menggunakan kolom patokan _Primary Key_ ID entitas.
- **Baris 83-84 (`return NotFound(...)`)**: Cek _safety case_, mengembalikan galat gagal penemuan 404 kalau IDnya nihil.
- **Baris 86**: Kalau menemukan baris entitasnya kirim output produk (HTTP OK).
- **Baris 93 (`[HttpPost]`)**: Menangani proses POST, berguna untuk menambahkan row tabel data produk baru ke system.
- **Baris 94 (`public IActionResult Create(Product product)`)**: Parameter Model `product` bersumber dari tubuh Body data POST dari JSON si pengguna. 
- **Baris 96-97 (`if (!ModelState.IsValid) ...`)**: Validasi Data Input Manual mengeksekusi respon HTTP "Bad Request 400" jika format data JSON ngaco saat dipetakan.
- **Baris 99-100 `: Menaruh record entitas lokal tersebut ke EF Core `Products` lalu diteruskan untuk dieksekusi INSERT langsung dan menetap ke dalam PostgreSQL (`SaveChanges()`).
- **Baris 102 (`return CreatedAtAction(...)`)**: Memberikan balikan header 201 respons success yang mengimplementasikan link tautan ke function `GetById` dengan param ID yang baru spesifik dibuat.
- **Baris 109-111 (`[HttpPut("{id}")] ...`)**: Memenuhi RESTful API HTTP PUT bertujuan mengubah isi _resource_ (update row tabel spesifik dari suatu ID URL dengan payload model barunya).
- **Baris 112-118**: Penjagaan input validasi error standar JSON dan _check existence_, temukan dulu objek relasinya, lalu jaminan jika ID tak valid buang ke HTTP Response NotFound 404.
- **Baris 120-122**: Menimpa entitas yang baru dicungkil _Find_ dengan state atribut values nama serta harga ke memory EF yang baru kemudian dibungkus dan disimpan mutlak di Database SQL DB `_context.SaveChanges()`.
- **Baris 124 (`return Ok(data);`)**: Memberi response 200 HTTP data utuhnya agar terpercaya di sisi User bila berhasil disunting sempurna.
- **Baris 131-132 (`[HttpDelete("{id}")] ...`)**: API Rute HTTP Delete bertujuan membersihkan/menghapus _Resource_ secara permanen melalui identitas parameter id yang dilampirkan via URL Link.
- **Baris 134-137**: Tetap mendeteksi/mengambil entitas aslinya buat dijamin keamanan eksistensinya dulu sebelum dilenyapkan; kembalikan 404 kalau ia tidak ada di tabel.
- **Baris 139-140 (`_context.Products.Remove(data);`)**: Memberitahukan engine EntityFramework Core untuk menstempel flag "Status Dibuang/Deleted", dimana segera sesudah perintah `SaveChanges()` dijalankan, Eksekusi script SQL Query *DELETE FROM* ke Postgre berjalan membasmi objek itu secara permanen.
- **Baris 142 (`return Ok(...)`)**: Mengakhiri respon koneksi Delete dengan Status OK yang membungkus pesan JSON _string message_ berformat bahwa namanya telah raib sukses.
