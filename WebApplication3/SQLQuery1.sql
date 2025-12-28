-- sozluk veritabanında çalıştırılacak tablolar

USE sozluk;
GO

-- Arşiv tablosu (ekleme ve güncelleme kayıtları)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'KelimeArsiv')
BEGIN
    CREATE TABLE KelimeArsiv (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Kelime NVARCHAR(100) NOT NULL,
        EskiAnlam NVARCHAR(MAX) NULL,
        YeniAnlam NVARCHAR(MAX) NOT NULL,
        Islem NVARCHAR(50) NOT NULL, -- 'Eklendi' veya 'Güncellendi'
        Tarih DATETIME NOT NULL
    );
END
GO

-- Örnek cümleler tablosu
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OrnekCumleler')
BEGIN
    CREATE TABLE OrnekCumleler (
        Id INT PRIMARY KEY IDENTITY(1,1),
        KelimeId INT NOT NULL,
        KullaniciId INT NOT NULL,
        Cumle NVARCHAR(500) NOT NULL,
        OnayDurumu NVARCHAR(20) DEFAULT 'Beklemede', -- 'Beklemede', 'Onaylandi', 'Reddedildi'
        Tarih DATETIME NOT NULL,
        FOREIGN KEY (KelimeId) REFERENCES Kelimeler(Id)
    );
END
GO

-- Günün kelimesi tablosu
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GununKelimesi')
BEGIN
    CREATE TABLE GununKelimesi (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Kelime NVARCHAR(100) NOT NULL,
        Anlam NVARCHAR(MAX) NOT NULL,
        Tarih DATE NOT NULL UNIQUE
    );
END
GO

-- KelimeOnerileri tablosuna Durum kolonu ekle (eğer yoksa)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('KelimeOnerileri') AND name = 'Durum')
BEGIN
    ALTER TABLE KelimeOnerileri
    ADD Durum NVARCHAR(20) DEFAULT 'Beklemede'; -- 'Beklemede', 'Onaylandi', 'Reddedildi'
END
GO

-- Kelimeler tablosuna ID kolonu ekle (eğer yoksa - örnek cümleler için gerekli)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Kelimeler') AND name = 'Id')
BEGIN
    -- Önce yeni bir tablo oluştur
    CREATE TABLE Kelimeler_Yeni (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Kelime NVARCHAR(100) NOT NULL UNIQUE,
        Anlam NVARCHAR(MAX) NOT NULL
    );
    
    -- Eski verileri aktar
    INSERT INTO Kelimeler_Yeni (Kelime, Anlam)
    SELECT Kelime, Anlam FROM Kelimeler;
    
    -- Eski tabloyu sil
    DROP TABLE Kelimeler;
    
    -- Yeni tabloyu eski adıyla yeniden adlandır
    EXEC sp_rename 'Kelimeler_Yeni', 'Kelimeler';
END
GO

PRINT 'Tüm tablolar başarıyla oluşturuldu!';