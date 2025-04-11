# Changelog

## [Unreleased]

### Added

- Interaction system basé sur interface `IInteractable` + `IInteractionAction`
- Détection d’interaction via `InteractionDetector` (raycast + prompt)
- Prompt d’interaction en 2 modes : `WorldOnly`, `UIOnly`, `Both`
- `ScreenInteractionUI` et `WorldInteractionUI` fonctionnels
- Shader URP pour effet d’outline dynamique
- Script `OutlineHighlighter` pour surligner automatiquement les objets interactables
- Support du système d’action modulaire (ex: `PickupItem`)
- Support multijoueur natif (via Unity Netcode)
- Système de prompt totalement scalable avec `enum InteractionPromptType`
- Intégration d’un prefab `Interactable` prêt à l’emploi avec options configurables
- Gestion automatique des prompts (spawn ou activation)
- Intégration du système de contrôle via le nouveau Input System

### Changed

- Refacto complet du système de prompts pour le rendre modulaire et extensible
- Séparation claire des logiques (UI, interaction, actions)
- Amélioration des performances en évitant la duplication inutile de canvases

### Fixed

- Problème d’orientation du World UI corrigé avec `BillboardUI.cs`
- Problème de `MissingReferenceException` lors de la destruction d’objets interactables
- Positionnement incorrect de l’outline corrigé (position/scale dynamique)
- Suppression des `Debug.Log` inutiles dans les scripts finaux

### Removed

- Ancien système de prompt basé uniquement sur `InteractionCanvas` manuel
- Code redondant de debug après vérification de bon fonctionnement

## [Initial] – Projet Plan-B-Inc lancé

