delete from districts

-- Step 1: Insert into Districts without specifying Id
INSERT INTO Districts (Name) VALUES
('Origin'),
('Vortex'),
('Epsilon'),
('Summit'),
('Halo'),
('Nova'),
('Tarkov'),
('Forge'),
('Mirage'),
('Zenith'),
('Drift'),
('Crux'),
('Aegis');

-- Step 2: Map generated District IDs to names using a table variable
DECLARE @DistrictIds TABLE (
    Name NVARCHAR(50),
    Id INT
);

INSERT INTO @DistrictIds (Name, Id)
SELECT Name, DistrictId FROM Districts WHERE Name IN (
    'Origin', 'Vortex', 'Epsilon', 'Summit', 'Halo', 'Nova',
    'Tarkov', 'Forge', 'Mirage', 'Zenith', 'Drift', 'Crux', 'Aegis'
);

-- Step 3: Insert bounding points using the correct DistrictIds
INSERT INTO Districts_BoundingPoints (DistrictId, Latitude, Longitude)
SELECT Id, Latitude, Longitude FROM (
    -- Origin
    SELECT 'Origin' AS Name, 54.3 AS Latitude, 23.02 AS Longitude UNION ALL
    SELECT 'Origin', 52.7, 23.9 UNION ALL
    SELECT 'Origin', 52.27, 23.15 UNION ALL
    SELECT 'Origin', 53.47, 21.64 UNION ALL

    -- Vortex
    SELECT 'Vortex', 53.47, 21.64 UNION ALL
    SELECT 'Vortex', 54.3, 23.02 UNION ALL
    SELECT 'Vortex', 54.82, 18.3 UNION ALL
    SELECT 'Vortex', 53.5, 17.4 UNION ALL

    -- Epsilon
    SELECT 'Epsilon', 53.5, 17.4 UNION ALL
    SELECT 'Epsilon', 54.82, 18.3 UNION ALL
    SELECT 'Epsilon', 53.89, 14.23 UNION ALL
    SELECT 'Epsilon', 51.377, 15 UNION ALL

    -- Summit
    SELECT 'Summit', 51.377, 15 UNION ALL
    SELECT 'Summit', 50.14, 16.64 UNION ALL
    SELECT 'Summit', 51.16, 18.16 UNION ALL
    SELECT 'Summit', 52.16, 17.02 UNION ALL

    -- Halo
    SELECT 'Halo', 50.14, 16.64 UNION ALL
    SELECT 'Halo', 50.14, 19.43 UNION ALL
    SELECT 'Halo', 50.53, 19.90 UNION ALL
    SELECT 'Halo', 51.16, 18.16 UNION ALL

    -- Nova
    SELECT 'Nova', 50.14, 17.66 UNION ALL
    SELECT 'Nova', 49.42, 19.04 UNION ALL
    SELECT 'Nova', 50.03, 20.63 UNION ALL
    SELECT 'Nova', 50.14, 20.36 UNION ALL

    -- Pulse
    SELECT 'Tarkov', 50.03, 20.63 UNION ALL
    SELECT 'Tarkov', 50.20, 20.97 UNION ALL
    SELECT 'Tarkov', 49.99, 21.26 UNION ALL
    SELECT 'Tarkov', 49.90, 20.985 UNION ALL

    -- Forge
    SELECT 'Forge', 49.42, 19.04 UNION ALL
    SELECT 'Forge', 50.03, 20.63 UNION ALL
    SELECT 'Forge', 49.90, 20.985 UNION ALL
    SELECT 'Forge', 49.137, 22.72 UNION ALL

    -- Mirage
    SELECT 'Mirage', 49.137, 22.72 UNION ALL
    SELECT 'Mirage', 49.90, 20.985 UNION ALL
    SELECT 'Mirage', 49.99, 21.26 UNION ALL
    SELECT 'Mirage', 50.97, 23.93 UNION ALL

    -- Zenith
    SELECT 'Zenith', 50.97, 23.93 UNION ALL
    SELECT 'Zenith', 49.99, 21.26 UNION ALL
    SELECT 'Zenith', 50.20, 20.97 UNION ALL
    SELECT 'Zenith', 52.27, 23.15 UNION ALL

    -- Drift
    SELECT 'Drift', 52.27, 23.15 UNION ALL
    SELECT 'Drift', 50.20, 20.97 UNION ALL
    SELECT 'Drift', 53.5, 17.4 UNION ALL
    SELECT 'Drift', 53.47, 21.64 UNION ALL

    -- Crux
    SELECT 'Crux', 52.18000, 18.8596300 UNION ALL
    SELECT 'Crux', 53.5, 17.4 UNION ALL
    SELECT 'Crux', 51.377, 15 UNION ALL
    SELECT 'Crux', 52.16, 17.02 UNION ALL

    -- Aegis
    SELECT 'Aegis', 52.18000, 18.8596300 UNION ALL
    SELECT 'Aegis', 52.16, 17.02 UNION ALL
    SELECT 'Aegis', 51.16, 18.16 UNION ALL
    SELECT 'Aegis', 50.86000, 20.27655
) AS Points
JOIN @DistrictIds d ON d.Name = Points.Name;
