# MedicalCatalogService

Reference data service for ICD?10, LOINC, and ATC/DDD. Owns schema `catalog` in the shared SQL database with independent EF Core migrations (history table `catalog.__EFMigrationsHistory`).

Key features

- Exact LOINC schema (2.81) with full?text index on LongCommonName, Component, and ShortName.
- Streaming CSV/TSV importers with robust parsing, file?level dedupe, and DB?level uniqueness.
- Fast purge: TRUNCATE with safe whitelist and batched DELETE fallback.
- Batched writes (~5k rows per batch) and release logging in `catalog.release` (with Description).
- Diagnostics for schema, migrations, counts, and spot lookups.

Auth

- In Development/Test, import endpoints are open (policy CatalogImport allows anonymous). In other environments, a valid JWT is required.

Endpoints

- GET /api/catalog/icd10?q=
- POST /api/catalog/import/icd10?version=YYYY-MM-DD[&purge=true]
- POST /api/catalog/import/loinc?version=2.81[&purge=true] (multipart: file=@Loinc.csv)
- POST /api/catalog/import/atc?version=2024-07-31[&purge=true] (multipart: file=@WHO-ATC-DDD.csv)
- POST /api/catalog/import/loinc-mapto?version=2.81[&purge=true] (multipart: file=@MapTo.csv)
- POST /api/catalog/import/loinc-answers?version=2.81[&purge=true] (multipart: answerList=@AnswerList.csv, listLink=@LoincAnswerListLink.csv)
- POST /api/catalog/import/loinc-panels-and-forms?version=2.81[&purge=true] (multipart: file=@PanelsAndForms.csv)
- POST /api/catalog/import/loinc-consumer-names?version=2.81[&purge=true] (multipart: file=@ConsumerName.csv)
- GET /api/catalog/diag/migrations
- GET /api/catalog/diag/schema
- GET /api/catalog/diag/loinc-stats
ATC

- GET /api/catalog/atc?q=
- GET /api/catalog/diag/loinc-mapto/{code}
- GET /api/catalog/diag/loinc-answers/{code}
- GET /api/catalog/diag/loinc-panel/{code}

Import behavior

- Auto?detects delimiter (tab/comma), handles quoted fields, skips empty rows and in?file duplicates.
- Purge deletes existing rows in the target table using TRUNCATE (or batched DELETE fallback) before import.
- Writes in batches (~5k) to reduce transaction pressure; upserts only when needed where applicable.
- Records a `catalog.release` row for system="loinc"/"icd10" with the provided `version`.

LOINC dataset mapping

- Main: `Loinc_2.81/LoincTable/Loinc.csv` ? catalog.loinc (exact fields).
- MapTo: `Loinc_2.81/LoincTable/MapTo.csv` ? catalog.loinc_map_to.
- Answers: `Loinc_2.81/AccessoryFiles/AnswerFile/AnswerList.csv` and `LoincAnswerListLink.csv` ? catalog.loinc_answer_list + loinc_answer_link.
- Panels: `Loinc_2.81/AccessoryFiles/PanelsAndForms/PanelsAndForms.csv` ? catalog.loinc_panel + loinc_panel_item (Ordinal, Optionality captured).
- Consumer names: `Loinc_2.81/AccessoryFiles/ConsumerName/ConsumerName.csv` ? catalog.loinc_consumer_name.

Usage examples (Windows CMD)

- Quote URLs with `&` to avoid shell parsing; for multipart fields use the shown names.
- Main LOINC (purge):
  
	```cmd
	curl -sS -X POST "http://localhost:8083/api/catalog/import/loinc?version=2.81&purge=true" -F "file=@d:/path/Loinc.csv"
	```

- MapTo:
  
	```cmd
	curl -sS -X POST "http://localhost:8083/api/catalog/import/loinc-mapto?version=2.81&purge=true" -F "file=@d:/path/MapTo.csv"
	```

- Answers:
  
	```cmd
	curl -sS -X POST "http://localhost:8083/api/catalog/import/loinc-answers?version=2.81&purge=true" -F "answerList=@d:/path/AnswerList.csv" -F "listLink=@d:/path/LoincAnswerListLink.csv"
	```

- Panels & Forms:
  
		```cmd
		curl -sS -X POST "http://localhost:8083/api/catalog/import/loinc-panels-and-forms?version=2.81&purge=true" -F "file=@d:/path/PanelsAndForms.csv"
		```

- Consumer names:
  
		```cmd
		curl -sS -X POST "http://localhost:8083/api/catalog/import/loinc-consumer-names?version=2.81&purge=true" -F "file=@d:/path/ConsumerName.csv"
		```

	- ATC/DDD:

		```cmd
		curl -sS -X POST "http://localhost:8083/api/catalog/import/atc?version=2024-07-31&purge=true" -F "file=@d:/path/WHO ATC-DDD 2024-07-31.csv"
		```

Docker

- Service listens on 8083 (compose service: medical-catalog-service). On startup in non?prod it applies migrations automatically.

Test data

- See `backend-container/MedicalCatalogService/test-data/` for small sample clips of each LOINC file.

Notes

- Full?text index is created via migration outside transactions.
- Some large text columns are NVARCHAR(MAX); `System` is NVARCHAR(512).
