-- Arama Geçmişi ve Popüler Kelimeler için Tablolar
USE sozluk;
GO

-- Arama Geçmişi Tablosu
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AramaGecmisi')
BEGIN
    CREATE TABLE AramaGecmisi (
        Id INT PRIMARY KEY IDENTITY(1,1),
        KullaniciId INT NULL, -- NULL olabilir (giriş yapmamış kullanıcılar için)
        Kelime NVARCHAR(100) NOT NULL,
        AramaTarihi DATETIME NOT NULL DEFAULT GETDATE(),
        IpAdresi NVARCHAR(50) NULL, -- Opsiyonel: İstatistik için
        INDEX IX_KullaniciId (KullaniciId),
        INDEX IX_Kelime (Kelime),
        INDEX IX_AramaTarihi (AramaTarihi)
    );
    PRINT 'AramaGecmisi tablosu oluşturuldu.';
END
ELSE
BEGIN
    PRINT 'AramaGecmisi tablosu zaten mevcut.';
END
GO

-- Popüler Kelimeler için View (Gerçek zamanlı istatistik)
IF EXISTS (SELECT * FROM sys.views WHERE name = 'PopulerKelimeler')
BEGIN
    DROP VIEW PopulerKelimeler;
END
GO

CREATE VIEW PopulerKelimeler AS
SELECT 
    Kelime,
    COUNT(*) AS AramaSayisi,
    MAX(AramaTarihi) AS SonArama,
    COUNT(DISTINCT KullaniciId) AS FarkliKullaniciSayisi
FROM AramaGecmisi
WHERE AramaTarihi >= DATEADD(DAY, -30, GETDATE()) -- Son 30 gün
GROUP BY Kelime;
GO

PRINT 'PopulerKelimeler view oluşturuldu.';
GO

-- Temizlik: 1 yıldan eski kayıtları silmek için stored procedure
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'EskiAramalariTemizle')
BEGIN
    DROP PROCEDURE EskiAramalariTemizle;
END
GO

CREATE PROCEDURE EskiAramalariTemizle
AS
BEGIN
    DELETE FROM AramaGecmisi 
    WHERE AramaTarihi < DATEADD(YEAR, -1, GETDATE());
    
    PRINT CAST(@@ROWCOUNT AS NVARCHAR) + ' adet eski kayıt silindi.';
END
GO

PRINT 'EskiAramalariTemizle procedure oluşturuldu.';
GO