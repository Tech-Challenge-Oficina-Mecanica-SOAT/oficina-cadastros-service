// Os cenários compartilham um único container de Postgres: rodam um de cada vez.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
