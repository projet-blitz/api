# Back-end de Blitz

Cette API est conçu en .NET et a pour objectif de fournir les horaires de bus en temps réel en utilisant les données GTFS Realtime. 
L'API fournit également des informations sur les lignes de bus et leurs arrêts. 

## Prérequis

Avant de pouvoir exécuter le back-end de l'API, assurez-vous de disposer des éléments suivants :

- [.NET 6.0 Core SDK](https://dotnet.microsoft.com/download) : Assurez-vous d'installer la version compatible de .NET Core SDK sur votre machine.

- Clé d'API STM : Vous devez obtenir une clé d'API de la STM pour accéder aux données des horaires de bus en temps réel. Vous pouvez vous en procurer une en vous inscrivant sur le [portail développeur de la STM](https://www.stm.info/fr/a-propos/developpeurs).

## Installation

1. Clonez le projet sur votre machine locale :

```bash
git clone https://github.com/projet-blitz/api.git
```

2. Ouvrez le projet dans votre environnement de développement préféré (par exemple, Visual Studio).

3. Exécutez le projet pour démarrer l'API.

## Configuration
1. Créez un fichier `.env` à la racine du projet et ajoutez votre clé d'API STM de la manière suivante :
```bash
STM_API_KEY=VOTRE_CLÉ_API_STM
```

2. Téléchargez les fichiers GTFS static (GTFS planifié) dans le dossier `gtfs/`. Ils sont nécessaires pour le fonctionnement de l'API. Pour ce faire, vous pouvez utiliser l'endpoint `GET /gtfs` (voir la section utilisation) ou bien vous pouvez les télécharger manuellement sur le site de la STM : [Téléchargez le GTFS planifié](https://www.stm.info/sites/default/files/gtfs/gtfs_stm.zip).

## Utilisation
L'API fournit les endpoints suivants pour accéder aux données des horaires de bus en temps réel, des lignes de bus et des arrêts :
- `GET /routes` : Obtiens le numéro, le nom et les directions de toutes les lignes de bus.
- `GET /stops/{routeId}/{direction}` : Obtiens l'ordre, le code et le nom des arrêts pour une ligne de bus selon sa direction.
- `GET /horaires/{routeId}/{stopId}` : Obtiens les horaires en temps réel pour un arrêt.
#### Débuggage et tests :
- `GET /all` : Retourne la réponse JSON complète de la STM.
- `GET /gtfs` : Télécharge les fichiers GTFS static (utilisés pour les routes et leurs arrêts) et les place dans le dossier `gtfs/`.
