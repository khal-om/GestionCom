select "MigrationId" from "__EFMigrationsHistory" order by "MigrationId";
select table_name, column_name, data_type, numeric_precision, numeric_scale, is_nullable from information_schema.columns where (table_name='Produits' and column_name='PrixUnitaire') or (table_name='VenteLignes' and column_name in ('PrixUnitaire','RemisePercent')) order by table_name, column_name;
