# Back-end de Blitz

Cette API est conçu en .NET et a pour objectif de fournir les horaires de bus en temps réel en utilisant les données GTFS Realtime. 
L'API fournit également des informations sur les lignes de bus et leurs arrêts. 

## Prérequis

Avant de pouvoir exécuter le back-end de l'API, assurez-vous de disposer des éléments suivants :

- [.NET 8.0 Core SDK](https://dotnet.microsoft.com/download) : Assurez-vous d'installer la version compatible de .NET Core SDK sur votre machine.

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
stm_api_key=VOTRE_CLÉ_API_STM
```

2. Générez le réseau de bus. Pour ce faire, vous pouvez utiliser l'endpoint `GET /updateBusNetwork` (voir la section utilisation). Le back-end va télécharger les fichiers du GTFS static (GTFS planifié) et créer un fichier .json contenant le réseau de bus.

## Utilisation
L'API fournit les endpoints suivants pour accéder aux données des horaires de bus en temps réel et du réseau de bus :
- `GET /getBusNetwork` : Obtiens le réseau de bus de la STM en format json.
- `GET /horaires/{routeId}/{stopId}` : Obtiens les horaires en temps réel pour un arrêt.
#### Débuggage et tests :
- `GET /updateBusNetwork` : Télécharge les fichiers GTFS static (utilisés pour les routes et leurs arrêts) et refait le fichier .json contenant le réseau de bus.
