CREATE DATABASE autenticacion;
use autenticacion;

-- Crear la tabla ConfiguracionEmpresa
CREATE TABLE ConfiguracionEmpresa (
    IDEmpresa INT PRIMARY KEY IDENTITY(1,1),
    NombreEmpresa NVARCHAR(100)
);

-- Crear la tabla Usuarios con una clave for�nea que hace referencia a ConfiguracionEmpresa
CREATE TABLE Usuarios (
    IDUsuario INT PRIMARY KEY IDENTITY(1,1),
    NombreUsuario NVARCHAR(50),
    Contrase�a NVARCHAR(100),
    Habilitado BIT,
    IntentosFallidosInicioSesion INT,
    UltimaFechaInicioSesion DATETIME,
    IDEmpresa INT,  -- Agregar columna para la clave for�nea
    FOREIGN KEY (IDEmpresa) REFERENCES ConfiguracionEmpresa(IDEmpresa) -- Definir la clave for�nea
);


INSERT INTO ConfiguracionEmpresa (NombreEmpresa)
VALUES 
('Cl�nica Veterinaria PetCare'),
('Hospital Animalia'),
('Veterinaria Amigos Peludos'),
('Centro de Salud Animal HappyPaws'),
('Cuidado Animal VetVida');


INSERT INTO Usuarios (NombreUsuario, Contrase�a, Habilitado, IntentosFallidosInicioSesion, UltimaFechaInicioSesion, IDEmpresa)
VALUES 
('Luis', '12345', 1, 0, GETDATE(), 1),
('Felipe', '123', 1, 0, GETDATE(), 2), 
('Esteban', '54321', 1, 0, GETDATE(), 3); 
    






SELECT * FROM Usuarios;
Select * from ConfiguracionEmpresa;

DELETE FROM Usuarios;
DELETE FROM ConfiguracionEmpresa;

