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

## Mot de passe oublié : code reçu par e-mail

Sur l'écran de connexion, cliquez sur **Mot de passe oublié ?**, saisissez l'adresse du gérant,
puis **Envoyer un code par e-mail**. Le code à six chiffres expire au bout de 10 minutes ;
cinq erreurs au maximum sont autorisées et deux demandes doivent être espacées d'une minute.
Un code utilisé ne peut pas être réutilisé. Le nouveau mot de passe doit respecter la politique
existante (au moins 10 caractères, majuscule, minuscule, chiffre et symbole).

**Configuration indispensable avant utilisation :** la machine doit avoir accès à Internet et
à un serveur SMTP avec STARTTLS (port 587 par défaut). Renseignez les variables d'environnement
dans le profil Windows ou le mécanisme de déploiement, puis relancez l'application :

- `DAROUICH_SMTP_HOST` : nom d'hôte SMTP du fournisseur
- `DAROUICH_SMTP_PORT` : port (par défaut `587`)
- `DAROUICH_SMTP_USER` : utilisateur SMTP
- `DAROUICH_SMTP_PASSWORD` : mot de passe d'application / jeton SMTP
- `DAROUICH_SMTP_FROM` : adresse d'expédition autorisée (facultatif, par défaut SMTP_USER)

N'ajoutez **jamais** le secret SMTP à GitHub, dans un fichier `.env` versionné, ni dans
les sources. Utilisez des identifiants dédiés à l'envoi avec des droits restreints. Un
fournisseur qui n'accepte pas STARTTLS sur le port choisi ne fonctionne pas avec cette
implémentation. Cette fonctionnalité dépend d'Internet ; le reste du logiciel demeure hors ligne.

Le hash du code (et non le code en clair), son expiration et le nombre d'essais sont stockés
dans SQLite. Le code est supprimé après une réinitialisation réussie. Sur un ordinateur
partagé, protégez également l'accès au fichier SQLite et au profil Windows.

**État :** cette fonctionnalité est implémentée dans le code, mais la compilation Windows
et la délivrabilité SMTP doivent être vérifiées sur l'environnement de déploiement.
