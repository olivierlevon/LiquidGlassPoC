# Liquid Glass PoC for Windows

PoC native Windows qui recrée une interface inspirée de **Liquid Glass** avec les primitives de Windows 11 :

- WinUI 3 et Windows App SDK 2.5.1 ;
- Mica, Mica Alt et Desktop Acrylic interchangeables à l’exécution ;
- surfaces Acrylic internes avec teinte et luminosité réglables ;
- contour spéculaire multicouche ;
- reflet radial suivant le pointeur ;
- animations Composition au survol et à la pression ;
- barre de titre personnalisée ;
- icône multi-résolution commune à la barre de titre, à l’exécutable et à la barre des tâches ;
- navigation Atelier interactive avec quatre sections et actions contextuelles ;
- thèmes clair/sombre, contraste élevé et repli opaque ;
- builds x64 et ARM64.

Le projet cible **Visual Studio 2026**, **.NET 10** et Windows 11.

## Prérequis

- Windows 11 version 21H2 ou ultérieure ;
- Visual Studio 2026 ;
- workload **Windows application development** avec .NET 10 ;
- Windows SDK 10.0.26100 ou ultérieur.

Le projet référence `Microsoft.Windows.SDK.BuildTools` 10.0.28000.2705 via NuGet. Le runtime Windows App SDK est embarqué dans la sortie grâce à `WindowsAppSDKSelfContained`.

## Ouvrir et exécuter

1. Extraire l’archive dans **un nouveau dossier**. Ne pas l’écraser sur une ancienne copie : Visual Studio pourrait conserver d’anciens fichiers XAML dans `.vs` et `obj`.
2. Ouvrir `LiquidGlassPoC.slnx` dans Visual Studio 2026. Une solution classique `LiquidGlassPoC.sln` est aussi fournie en secours.
3. Choisir `x64` ou `ARM64`.
4. Définir `LiquidGlassPoC` comme projet de démarrage si nécessaire.
5. Lancer avec `F5`.

Le fichier `LiquidGlassPoC.slnx` est généré depuis `LiquidGlassPoC.sln` avec `dotnet sln migrate`. Il contient les mappings de projet explicites `*|x64 → x64` et `*|ARM64 → ARM64`, en plus des plateformes de solution. Les quatre couples `Debug|x64`, `Debug|ARM64`, `Release|x64` et `Release|ARM64` sont donc résolus vers des configurations réellement déclarées par le projet.

La première restauration NuGet télécharge Windows App SDK et les outils Windows SDK.

Si Visual Studio affiche encore un diagnostic provenant d’une ancienne version, fermer Visual Studio, exécuter `scripts\clean.ps1`, puis rouvrir la solution. Ce script supprime uniquement `.vs`, `bin` et `obj` du projet.

## Build PowerShell

Depuis la racine du projet :

```powershell
.\scripts\build.ps1 -Configuration Debug -Platform x64 -Run
```

Build ARM64 Release :

```powershell
.\scripts\build.ps1 -Configuration Release -Platform ARM64
```

## Structure

```text
LiquidGlassPoC.slnx
LiquidGlassPoC.sln
src/LiquidGlassPoC/
├── App.xaml                         ressources et matériaux
├── MainWindow.xaml                  interface de démonstration
├── MainWindow.xaml.cs               backdrops et réglages interactifs
├── Controls/LiquidGlassCard.cs      comportement pointeur/Composition
└── Themes/Generic.xaml              template du verre multicouche
scripts/
├── build.ps1
└── clean.ps1
```

## Recette visuelle

Chaque `LiquidGlassCard` assemble :

1. un `AcrylicBrush` pour le flou, la teinte et la luminosité ;
2. un halo radial dont le centre suit le pointeur ;
3. un contour en dégradé pour le reflet spéculaire ;
4. une animation Composition centrée sur la carte ;
5. un `FallbackColor` opaque utilisé si les transparences sont coupées.

Les couleurs d’Acrylic sont définies par des valeurs WinUI valides puis adaptées au thème dans `ApplyMaterialThemeColors()`. Le projet n’utilise ni le type XAML WPF `<Color>` ni la propriété UWP `AcrylicBrush.BackgroundSource`, qui ne sont pas disponibles sur `Microsoft.UI.Xaml.Media.AcrylicBrush`.

Le fond général de la fenêtre utilise Mica par défaut. Le panneau **Laboratoire** permet de passer à Mica Alt ou Desktop Acrylic et de modifier le matériau en direct.

## Limites du PoC

Le rendu reproduit la hiérarchie, la translucidité, les reflets et le mouvement du verre, mais n’effectue pas de véritable réfraction optique. Une réfraction locale demanderait une étape supplémentaire avec Win2D/Direct2D ou un shader, en capturant uniquement le contenu appartenant à l’application.

## Notes d’accessibilité et de performance

- Le bouton **Forcer le repli opaque** permet de contrôler le rendu sans transparence.
- Acrylic utilise automatiquement sa couleur de repli lorsque Windows désactive les effets de transparence.
- Le dictionnaire `HighContrast` élimine les orbes décoratifs et augmente le contraste.
- Les animations peuvent être désactivées depuis le panneau Laboratoire.
- Éviter d’étendre le nombre de surfaces Acrylic à toute l’application : réserver le verre aux zones de commande et aux panneaux flottants.

## Licence

Code de démonstration sous licence MIT. « Liquid Glass » est une désignation d’Apple ; ce projet n’est ni affilié à Apple ni une implémentation de ses API privées.
