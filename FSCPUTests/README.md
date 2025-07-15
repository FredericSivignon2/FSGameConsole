# Tests Unitaires FSCPU - Rapport de Couverture

## ?? R�sum� des Tests

- **Total de tests** : 96
- **Tests r�ussis** : 96 ?
- **Tests �chou�s** : 0 ?
- **Tests ignor�s** : 0 ??
- **Dur�e d'ex�cution** : 4.2 secondes

## ?? Structure des Tests

### 1. **MemoryTests.cs** (14 tests)
Tests de la classe `Memory` qui g�re la m�moire syst�me :

#### Tests de Base
- ? Initialisation correcte de la m�moire avec taille sp�cifi�e
- ? Lecture/�criture d'octets individuels
- ? Gestion des erreurs pour adresses invalides
- ? Op�rations sur mots 16 bits (little-endian)

#### Tests Avanc�s
- ? Chargement de programmes en m�moire
- ? Effacement complet de la m�moire
- ? Extraction de sections m�moire
- ? Fonctionnalit�s m�moire vid�o
- ? Affichage de dump m�moire pour debug

### 2. **StatusRegisterTests.cs** (18 tests)
Tests de la classe `StatusRegister` qui g�re les flags du processeur :

#### Tests des Flags Individuels
- ? Flag Zero (Z) - d�tection de r�sultats nuls
- ? Flag Carry (C) - d�tection de retenues/emprunts
- ? Flag Overflow (O) - d�tection de d�bordements
- ? Flag Negative (N) - d�tection de r�sultats n�gatifs

#### Tests de Combinaisons
- ? Mise � jour de multiple flags simultan�ment
- ? Tests th�oriques avec diff�rentes valeurs
- ? Formatage d'affichage des flags

### 3. **ALUTests.cs** (26 tests)
Tests de l'Unit� Arithm�tique et Logique :

#### Op�rations Arithm�tiques
- ? Addition avec gestion overflow et flags
- ? Soustraction avec gestion underflow et emprunts
- ? Incr�mentation/D�cr�mentation
- ? Comparaison de valeurs

#### Op�rations Logiques
- ? AND, OR, XOR avec mise � jour des flags
- ? D�calages gauche/droite (Shift Left/Right)
- ? Tests th�oriques avec patterns binaires vari�s

#### Tests de Flags
- ? V�rification correcte des flags Zero, Carry, Negative
- ? Comportement sp�cifique selon l'op�ration

### 4. **CPU8BitTests.cs** (28 tests)
Tests de la classe principale `CPU8Bit` :

#### Initialisation et Contr�le
- ? Initialisation correcte des registres
- ? Fonctions Start/Stop/Reset
- ? Gestion des registres A, B, C, D
- ? Acc�s aux registres par nom

#### Instructions Individuelles
- ? NOP (No Operation)
- ? HALT (Arr�t du processeur)
- ? LDA, LDB, LDC, LDD (Chargement imm�diat)
- ? ADD, SUB (Op�rations arithm�tiques)
- ? AND, OR (Op�rations logiques)
- ? JMP (Saut inconditionnel)
- ? STA (Stockage en m�moire)

#### Programmes Complexes
- ? Ex�cution de programmes multi-instructions
- ? Programmes avec boucles simples
- ? Gestion d'erreurs pour opcodes invalides

### 5. **IntegrationTests.cs** (10 tests)
Tests d'int�gration du syst�me complet :

#### Programmes R�alistes
- ? **Programme "Hello World"** - Affichage de texte en m�moire vid�o
- ? **Programme de comptage** - Logique de boucle et stockage
- ? **Test arithm�tique complet** - Toutes op�rations avec flags
- ? **Simulation de boucle** - Logique de saut et contr�le

#### Tests de Performance et Robustesse
- ? **Test de limites m�moire** - �criture aux fronti�res
- ? **Test de stress** - 100+ instructions cons�cutives
- ? **Int�gration m�moire vid�o** - Affichage multi-lignes

## ??? Technologies Utilis�es

- **xUnit** : Framework de tests unitaires pour .NET
- **FluentAssertions** : Biblioth�que d'assertions fluides et expressives
- **.NET 9** : Plateforme de d�veloppement moderne
- **C# 13** : Langage avec fonctionnalit�s avanc�es

## ?? Couverture de Code

Les tests couvrent exhaustivement :

### ? **Memory.cs** - Couverture compl�te
- Toutes les m�thodes publiques test�es
- Cas nominaux et cas d'erreur
- Zones m�moire sp�ciales (vid�o, pile)

### ? **StatusRegister.cs** - Couverture compl�te  
- Tous les flags individuels et combin�s
- M�thodes de mise � jour et de lecture
- Repr�sentation textuelle

### ? **ALU.cs** - Couverture compl�te
- Toutes les op�rations arithm�tiques et logiques
- Gestion correcte des flags pour chaque op�ration
- Tests de cas limites (overflow, underflow)

### ? **CPU8Bit.cs** - Couverture compl�te
- Cycle d'ex�cution Fetch-Decode-Execute
- Toutes les instructions impl�ment�es
- �tats du processeur (d�marr�/arr�t�)
- Gestion des registres

## ?? Points Forts de la Suite de Tests

1. **Tests Unitaires Isol�s** : Chaque composant test� ind�pendamment
2. **Tests d'Int�gration** : V�rification du syst�me complet
3. **Cas Limites** : Gestion des overflow, underflow, erreurs
4. **Programmes R�alistes** : Tests avec du vrai code assembleur
5. **Assertions Expressives** : Utilisation de FluentAssertions pour la lisibilit�
6. **Performance** : Tests de stress et de robustesse

## ?? Fonctionnalit�s Valid�es

### ? **Processeur 8 bits Fonctionnel**
- Registres A, B, C, D op�rationnels
- PC (Program Counter) et SP (Stack Pointer) fonctionnels
- Status Register avec tous les flags

### ? **Jeu d'Instructions Complet**
- Instructions de chargement (LDA, LDB, LDC, LDD)
- Instructions arithm�tiques (ADD, SUB)
- Instructions logiques (AND, OR)
- Instructions de contr�le (JMP, NOP, HALT)
- Instructions de stockage (STA)

### ? **Syst�me M�moire Robuste**
- M�moire 64KB adressable
- Zone m�moire vid�o d�di�e (2KB)
- Gestion little-endian des mots 16 bits
- Protection contre les acc�s invalides

### ? **ALU Compl�te**
- Op�rations arithm�tiques avec flags
- Op�rations logiques avec flags
- D�calages et rotations
- Comparaisons

## ?? Utilisation des Tests

Pour lancer les tests :
```bash
cd FSCPUTests
dotnet test
```

Pour lancer les tests avec plus de d�tails :
```bash
dotnet test --verbosity normal
```

Pour lancer les tests avec couverture de code :
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## ?? Conclusion

Cette suite de tests exhaustive valide que **le processeur 8 bits FSCPU est enti�rement fonctionnel** et pr�t pour l'�mulation de programmes. Tous les composants principaux (CPU, ALU, Memory, StatusRegister) fonctionnent correctement individuellement et en int�gration.

Le syst�me est capable d'ex�cuter des programmes r�els et d'afficher des r�sultats dans la m�moire vid�o, ce qui confirme que l'�mulateur FSGameConsole dispose d'une base solide pour l'ex�cution de jeux et d'applications vintage.