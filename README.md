# Darouich Travaux — application desktop .NET MAUI

Application Windows native, privée et hors ligne de gestion BTP. L'interface XAML suit MVVM : les vues ne contiennent aucune règle métier. Les couches Domain, Application et Infrastructure restent indépendantes de MAUI afin de préparer une future cible Android.

## Fonctionnalités livrées dans ce socle

- connexion du gérant unique et configuration locale initiale, sans inscription distante ou publique ;
- mots de passe PBKDF2-SHA512 avec sel aléatoire de 256 bits, 600 000 itérations et comparaison en temps constant ;
- base SQLite `darouich-travaux.db` dans `FileSystem.AppDataDirectory`, utilisable entièrement hors ligne ;
- tableau de bord, recherche/création rapide de clients et navigation vers les modules ERP ;
- changement de mot de passe et sauvegarde/restauration SQLite avec contrôle d'intégrité ;
- noms de sauvegarde `darouich-travaux-backup-YYYY-MM-DD-HHmm.db` ;
- service de nommage PDF produisant `Facture(Ahmed_El_Mansouri)-FAC-2026-018.pdf` ;
- dépendances QuestPDF et ClosedXML prêtes pour les documents de la phase suivante.

Les entrées du menu non encore implémentées constituent la feuille de route et ne sont pas présentées comme fonctionnelles.

## Prérequis développeur

- Windows 10 19041 ou plus récent / Windows 11 ;
- SDK .NET 10 avec la charge de travail MAUI : `dotnet workload install maui-windows` ;
- Visual Studio 2026 avec « Développement d’applications .NET MAUI » ou la CLI .NET.

Aucun serveur, PostgreSQL ou Node.js n'est requis. Au premier lancement, l'écran privé de configuration crée l'unique compte local. Le mot de passe n'est jamais journalisé ni stocké en clair.

## Restaurer, compiler et tester

```powershell
dotnet restore DarouichTravaux.slnx
dotnet build DarouichTravaux.slnx -c Release
dotnet run --project src/DarouichTravaux.Maui -f net10.0-windows10.0.19041.0
```

## Publication Windows x64 self-contained

```powershell
dotnet publish src/DarouichTravaux.Maui/DarouichTravaux.Maui.csproj `
  -f net10.0-windows10.0.19041.0 -c Release -r win-x64 `
  --self-contained true -p:PublishSingleFile=false `
  -o artifacts/publish
```

Le dossier contient `DarouichTravaux.exe` et toutes ses dépendances : l'utilisateur final n'installe pas .NET.

## Installateur Windows

1. Installer Inno Setup sur le poste de build.
2. Publier dans `artifacts/publish` avec la commande précédente.
3. Compiler `installer/DarouichTravaux.iss`.
4. Distribuer `installer/output/DarouichTravauxSetup.exe`.

L'installateur x64 crée les entrées Menu Démarrer et Désinstallation et propose un raccourci Bureau.

## Données et sauvegardes

La base active et le dossier `Sauvegardes` sont placés dans le répertoire applicatif propre à l'utilisateur Windows. Une sauvegarde utilise l'API SQLite, après checkpoint WAL. Avant restauration, l'application exécute `PRAGMA integrity_check` et conserve une copie de sécurité `.before-restore`. Il est recommandé de copier régulièrement les sauvegardes vers un support chiffré externe.

## Android ultérieur

Les services n'utilisent aucune API Windows. Pour Android, ajouter `net10.0-android` aux frameworks, le manifeste Android et une implémentation d'ouverture du dossier adaptée. La base et les ViewModels sont réutilisables.
