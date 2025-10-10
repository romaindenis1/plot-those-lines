# Rapport — Plot Those Lines

**Élève :** Romain Denis  
**Client :** Xavier Carrel  
**Période :** 29.08.2025 — 09.01.2026

## Description du domaine

Application C# permettant d'importer et d'afficher des séries temporelles. L'utilisateur peut ajouter de nouveaux fichiers CSV sans perdre les données déjà chargées.

Cas d'usage principal : visualiser le nombre de victoires par équipe (ex. NBA) au fil des années. Le format attendu du CSV est : YEAR, TEAM1, TEAM2, ... ; chaque cellule représente le nombre de victoires pour l'équipe et l'année indiquées (ex. : 2025, 52, 48, ...). Si le format n'est pas respecté, l'application affiche un message d'erreur et refuse l'import.

L'interface permet de naviguer dans le temps et d'ajuster le zoom pour explorer les courbes.

## Objectifs pédagogiques

- Mettre en pratique C# (List, Dictionary, tableaux), LINQ et bonnes pratiques.  
- Organiser un projet : user stories, planification, gestion Git et tests d'acceptance.  
- Traiter des données : parsing, gestion des NaN et comparaisons flottantes (tolérances).

## Objectifs produit

- Visualiser des séries depuis CSV/JSON/API.  
- Comparer plusieurs courbes, tracer des fonctions, afficher légendes et couleurs.  
- Interactions clés : hover, zoom et export simple.

## Domaine d’application

Le thème que j’ai choisi les vicioires des equipes NBA au fil du temps

Le but derrière est de pouvoir visualiser comment certaines equipes ont dominer leurs epoques et pour observer les evenements (lockout 1995, covid)

Cela va donc permettre de comparer les differentes epoques, et leurs meilleur (et pires) equipes

Plus précisément :

-	Les victoires d'une certaines equipes
-	Differentes saisons
-	Liste de nom et d’années à mettre sur le graphique

Les moyens de récupérer ces données seront les suivantes :

-	https://www.basketball-reference.com/
-	ChatGPT

**Dates:** 29.08.2025 - 09.01.2026

## Introduction
## Détails techniques

### Parsing et I/O
La lecture CSV se fait dans `LoadCsvAndPlot` avec `StreamReader` + `CsvReader`. (`csv.ReadHeader()`) est utilisé pour créer la  `data`. Le chemin par défaut `csvFilePath` (dans `Application.StartupPath`) permet de charger un fichier a l'execution.

Pour chaque ligne, la première colonne est parsée comme X (car la premiere collone est l'année) avec `TryParse`; en cas d'échec on stocke `double.NaN`. Les autres colonnes sont parsées comme Y (car se sont les donnes a mettre sur le graphique) avec `TryParse(csv.GetField(pos), out var val)` et ajoutées aux listes du dictionnaire `data`.

### Modèle en mémoire
Les données sont stockées dans `Dictionary<string, List<double>> data` et `List<double> years`. Avant l'affichage, `years` devient `double[] dataX`. Pour l'interaction, chaque série est convertie en `SeriesData { Name, XValues, YValues }` (tableaux) et ajoutée à `List<SeriesData> allSeriesData` pour la lire.

### Affichage
Le affichage utilise ScottPlot : on appelle `formsPlot1.Plot.Add.Scatter(dataX, yValues)` pour chaque série. La couleur est choisie par le tableau `palette`. Les axes et la légende sont activés avec `Plot.XLabel`, `Plot.YLabel` et `Plot.Legend.IsVisible`.

### Hover
Sur `MouseMove`, on obtient la position souris via `GetCoordinates`. On parcourt les points, convertit chaque point en pixel (`Plot.GetPixel`) et calcule la distance pour trouver le point le plus proche dans un aire de 50 px. Ensuite un filtre LINQ (`Math.Abs(p.X - matchedX) < tolerance`) identifie toutes les séries contenant ce point. Les noms sont affichés dans `label1`.

### Import et doublons
Le bouton d'import ouvre `OpenFileDialog`. Le fichier sélectionné est comparé au fichier préchargé a `csvFilePath` existant via `FileCompare` (byte par byte). Si différent, on copie le fichier puis on re appelle `LoadCsvAndPlot`. Sinon, on ne charge pas le fichier (par ce que il est identique) et on alerte avec `MessageBox.Show`.

## Rapport de tests

Tous les tests d'acceptaces ont été vérifiés et sont OK.

| User story | Critères d'acceptation (résumé) | Statut | Lien (issue / user story) |
|---|---:|:---:|---|
| Une ligne par entrée de données | Pour chaque entrée importée, une courbe/ligne est tracée représentant les valeurs | OK | [Lien](https://github.com/romaindenis1/plot-those-lines/issues/1) |
| Même période pour toutes les séries | Toutes les séries partagent le même axe temporel / même plage X lors de l'affichage | OK | [Lien](https://github.com/romaindenis1/plot-those-lines/issues/1) |
| Zoom (Ctrl + molette) | Ctrl + molette effectue un zoom centré et fluide de l'affichage | OK | [Lien](https://github.com/romaindenis1/plot-those-lines/issues/2) |
| Échelles visibles | Échelles X et Y affichées et visibles quand les données sont importées | OK | [Lien](https://github.com/romaindenis1/plot-those-lines/issues/2) |
| Labels colorés | Chaque série a un label/texte affiché dans la couleur correspondante sur le graphique | OK | [Lien](https://github.com/romaindenis1/plot-those-lines/issues/2) |
| Valeur au survol | Au survol d'un point, la valeur exacte et la date sont affichées | OK | [Lien](https://github.com/romaindenis1/plot-those-lines/issues/2) |
| Titre personnalisé | L'utilisateur peut entrer un titre via le champ et l'appliquer au graphique | OK | [Lien](https://github.com/romaindenis1/plot-those-lines/issues/2) |
| Aucun graphique au premier lancement | À la première ouverture, l'application n'affiche pas de graphique par défaut | OK | [Lien](https://github.com/romaindenis1/plot-those-lines/issues/3) |
| Restauration du graphique précédent | Après la première utilisation, le graphique précédemment affiché se recharge à l'ouverture suivante | OK | [Lien](https://github.com/romaindenis1/plot-those-lines/issues/3) |
| Import robuste & gestion d'erreurs | Import CSV : données valides importées; en cas d'erreur (type, vide, pas de changement), l'import est rejeté avec message d'erreur explicite | OK | [Lien](https://github.com/romaindenis1/plot-those-lines/issues/3) |
| Pas d'import en double | Si les mêmes données existent déjà, l'import est refusé et l'utilisateur est informé | OK | [Lien](https://github.com/romaindenis1/plot-those-lines/issues/3) |

## Maquettes

Representation Graphique
![Maquette Representation Graphique](./MaquetteRepGraphique.png)
Pour cette maquette, on cherche a montrer les parametres d'affichage. On montre que le graphique doit avoire un axe x et y, et que il y a une legende avec la couleur et le nom d'une donné. 

Flexibilité D'affichage
![Maquette Flexibilité d'Affichage](./MaqDisplayFlex.png)
Pour cette maquette, on cherche a montrer que il y a un slider pour zoomer et dezoomer le graphique, et que il y a aussi un affichage de la date et de la valeur d'une donné si la souris et mis sur une donne.

Importation de donnes flexible
![Stockage Schema](./MaquetteStockage.PNG) 
![Maquette Stockage 2](./MaquetteStockage2.png)
Pour cette maquette, on cherche a montrer qu'il y a un bouton pour importer.
Dans le schema, on cherche a montrer comment les donnés sont stockés de manières permanentes. Si les donnes sont validés par le programme, le fichier est copié dans le repertoire du programme et ensuite chargé pour etre affiché.

Affichage de Fonctions Mathematiques

![Maquette Mathematique](./MaqMath.png)
Dans cette maquette, on cherche a montrer que en utilisant la boite de texte, on peux entrer une fonction mathematique et la faire apparaitre avec nos autre donnés. Meme si cette fonctionalité n'est pas implémenté dans mon code, je pense que elle est assez claire pour que moi ou un autre developpeur puisse la réaliser dans le futur

## Usage de l’IA dans le projet

- L'AI a seulement été utilisé pour les taches avec aucune valeure ajouté par humain, par example:
    - Le squelette de rapport
    - Donnés
- L'AI a acceleré les taches avec aucune valeur ajouté humain, et la lecture de la documentation (surtout la documentation scottplot qui est compliqué)
- Réflexion critique sur les avantages et limites


---

## Conclusion / Bilan
- Points forts du projet
- Axes d’amélioration possibles
- Compétences acquises
- Perspectives futures

---

