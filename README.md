# Darouich Travaux

Mini-ERP BTP privé pour le gérant de Darouich Travaux. Cette première livraison pose l'architecture propre et le socle exécutable de la phase 1 : Identity, clients, chantiers, factures, paiements, dépenses et indicateurs.

## Prérequis et configuration

- Développement : SDK .NET 10 et PostgreSQL 18.
- Production : Docker et un reverse proxy HTTPS. Ne publiez jamais PostgreSQL sur Internet.
- Variables obligatoires : `ConnectionStrings__PostgreSQL` et `POSTGRES_PASSWORD`. Le compte gérant et son mot de passe temporaire doivent être injectés par un secret de déploiement, jamais versionnés.

```bash
export ConnectionStrings__PostgreSQL='Host=localhost;Database=darouich;Username=darouich;Password=...'
dotnet ef database update --project src/DarouichTravaux.Infrastructure --startup-project src/DarouichTravaux.Web
dotnet run --project src/DarouichTravaux.Web
```

Identity impose 10 caractères, majuscule, minuscule, chiffre et caractère spécial, verrouille après cinq échecs et utilise un cookie `HttpOnly`, `Secure`, `SameSite=Strict`. Toutes les routes sont privées par défaut ; `/login` est explicitement anonyme. Il n'existe aucune inscription publique.

## Build, tests et déploiement

```bash
dotnet restore
dotnet test
docker compose up --build -d
```

Le proxy (Caddy, Traefik ou Nginx) doit terminer TLS, rediriger HTTP vers HTTPS et transmettre les en-têtes `X-Forwarded-*`. Sauvegarde quotidienne : `pg_dump -Fc darouich > darouich.dump`. Restauration testée : `pg_restore --clean --if-exists -d darouich darouich.dump`.

## Publication Windows

Le client Windows dédié est prévu dans la prochaine itération. La commande de publication self-contained sera :

```powershell
dotnet publish src/DarouichTravaux.Windows -c Release -r win-x64 --self-contained true
```

Inno Setup produira `DarouichTravauxSetup.exe`, avec raccourcis Bureau/Menu Démarrer et désinstallation. Le client appellera exclusivement l'API hébergée en HTTPS : ni SDK, ni PostgreSQL, ni Node.js ne seront requis sur le PC.

## Feuille de route

La phase 1 est livrée de façon incrémentale : ce socle contient les modèles et règles critiques, mais les écrans CRUD, migrations générées, réinitialisation email, PDF QuestPDF, paramètres complets et seed sécurisé restent à terminer avant d'entamer la phase 2. Chaque secret doit vivre dans le gestionnaire de secrets de l'hébergeur.
