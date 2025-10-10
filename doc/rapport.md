# Rapport *Plot those dots*

**Eleve:** Romain Denis

**Client:** Xavier Carrel

**Dates:** 29.08.2025 - 09.01.2026

## Introduction

### Objectifs du produit et pédagogiques
- Concevoir un logiciel pour afficher des graphiques sur des données

### Description du domaine
- P_Fun: Plot Those Lines
- J'ai décidé de me baser sur les donnés de la NBA
- Mes donnés sont venus de [BasketballReference](https://www.basketball-reference.com/) et ChatGPT
- Mes donnés sont dans */doc/data.csv*

- Limites et périmètre du projet

---

## Planification
- [GitHub Project](https://github.com/users/romaindenis1/projects/5)
- [Issues GitHub] (https://github.com/romaindenis1/plot-those-lines/issues?q=is%3Aissue)
- Les étapes clés du projet
    - Planification
    - Réalisation
    - Rapport
- Organisation du travail (planning, deadlines)

---

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

