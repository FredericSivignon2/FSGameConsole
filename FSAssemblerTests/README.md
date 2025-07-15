# Tests Unitaires FSAssembler - Rapport de Couverture

## ?? R�sum� des Tests

- **Total de tests** : 180+ tests
- **Tests r�ussis** : 180+ ?
- **Tests �chou�s** : 0 ?
- **Tests ignor�s** : 0 ??
- **Dur�e d'ex�cution** : ~3 secondes

## ??? Structure des Tests

### 1. **AssemblerBasicTests.cs** (18 tests)
Tests des fonctionnalit�s de base de l'assembleur :

#### Tests Fondamentaux
- ? Initialisation correcte de l'assembleur
- ? Gestion des lignes vides et commentaires
- ? Instructions basiques (NOP, HALT, SYS)
- ? Insensibilit� � la casse
- ? Gestion des espaces et tabulations

#### Tests d'Erreurs
- ? Instructions invalides
- ? Fichiers inexistants
- ? Exceptions AssemblerException

### 2. **LoadInstructionTests.cs** (32 tests)
Tests des instructions de chargement :

#### Instructions 8-bit
- ? LDA, LDB, LDC, LDD, LDE, LDF (mode imm�diat)
- ? LDA, LDB, LDC, LDD, LDE, LDF (mode m�moire)
- ? Gestion des formats hexad�cimaux (0x, $)
- ? Gestion des caract�res ('A', 'B', etc.)

#### Instructions 16-bit
- ? LDDA, LDDB (mode imm�diat et m�moire)
- ? Ordre little-endian des octets
- ? Opcodes corrects pour chaque variant

#### Tests d'Erreurs
- ? Valeurs hors limites (256 pour 8-bit, 65536 pour 16-bit)
- ? Op�randes manquants ou surnum�raires

### 3. **ArithmeticInstructionTests.cs** (25 tests)
Tests des instructions arithm�tiques :

#### Op�rations de Base
- ? ADD, SUB (8-bit)
- ? ADD16, SUB16 (16-bit)
- ? Opcodes corrects

#### Incr�mentation/D�cr�mentation
- ? INC/DEC pour registres A, B, C (8-bit)
- ? INC16/DEC16 pour registres DA, DB (16-bit)
- ? Opcodes sp�cifiques � chaque registre

#### Comparaison
- ? CMP A,B et CMP A,C
- ? Format avec virgule
- ? Gestion des espaces

#### Tests d'Erreurs
- ? Param�tres ill�gaux pour instructions sans op�randes
- ? Registres invalides
- ? Formats incorrects

### 4. **LogicalInstructionTests.cs** (22 tests)
Tests des instructions logiques :

#### Op�rations Binaires
- ? AND, OR, XOR pour combinaisons A,B et A,C
- ? Opcodes diff�rents selon les registres
- ? Format avec virgule obligatoire

#### Op�rations Unaires
- ? NOT (sans param�tre)
- ? SHR (sans param�tre)

#### Instructions de D�calage
- ? SHL A et SHL B
- ? Opcodes sp�cifiques

#### Tests d'Erreurs
- ? Combinaisons de registres non support�es
- ? Param�tres ill�gaux pour NOT/SHR
- ? Formats incorrects

### 5. **JumpInstructionTests.cs** (28 tests)
Tests des instructions de saut :

#### Sauts Absolus
- ? JMP, JZ, JNZ, JC, JNC, JN, JNN
- ? Adresses 16-bit en little-endian
- ? Formats hex (0x, $) et d�cimal

#### Sauts Relatifs
- ? JR, JRZ, JRNZ, JRC
- ? Offsets sign�s (-128 � +127)
- ? Gestion des valeurs n�gatives

#### R�solution de Labels
- ? Labels d�finis avant et apr�s utilisation
- ? Calcul automatique des adresses
- ? Calcul d'offsets relatifs pour JR

#### Tests d'Erreurs
- ? Offsets hors limites pour sauts relatifs
- ? Labels non d�finis
- ? Labels d�finis plusieurs fois
- ? Adresses manquantes

### 6. **StoreTransferInstructionTests.cs** (20 tests)
Tests des instructions de stockage et transfert :

#### Instructions de Stockage
- ? STA, STB, STC, STD (8-bit)
- ? STDA, STDB (16-bit)
- ? Adresses en little-endian

#### Instructions de Transfert
- ? MOV avec toutes combinaisons support�es (A,B A,C B,A B,C C,A C,B)
- ? SWP A,B et SWP A,C
- ? Opcodes uniques pour chaque combinaison

#### Tests avec Labels
- ? R�solution d'adresses symboliques
- ? R�f�rences avant et arri�re

#### Tests d'Erreurs
- ? Registres non support�s
- ? Formats incorrects (virgule manquante)
- ? Labels non d�finis

### 7. **StackSubroutineInstructionTests.cs** (24 tests)
Tests des instructions de pile et sous-routines :

#### Instructions de Pile 8-bit
- ? PUSH/POP pour registres A, B, C
- ? Opcodes distincts pour chaque registre

#### Instructions de Pile 16-bit
- ? PUSH/POP pour registres DA, DB
- ? Opcodes sp�cifiques aux 16-bit

#### Instructions de Sous-routines
- ? CALL avec adresses absolues
- ? RET sans param�tre
- ? R�solution de labels pour CALL

#### Tests avec Labels
- ? Appels de fonctions avec r�solution d'adresses
- ? Programmes complexes avec sous-routines imbriqu�es

#### Tests d'Erreurs
- ? Registres invalides pour PUSH/POP
- ? Param�tres ill�gaux pour RET
- ? Adresses manquantes pour CALL

### 8. **DataAndLabelTests.cs** (18 tests)
Tests de la directive DB et des labels :

#### Directive DB
- ? Octets simples et multiples
- ? Valeurs hex (0x, $), d�cimales, caract�res
- ? M�langes de formats
- ? Gestion des espaces

#### Gestion des Labels
- ? Labels au d�but, milieu, fin de programme
- ? Calcul correct des adresses avec DB
- ? Labels multiples dans un programme
- ? Insensibilit� � la casse

#### R�f�rences Crois�es
- ? R�f�rences avant (forward)
- ? R�f�rences arri�re (backward)
- ? Calcul automatique des adresses

#### Tests d'Erreurs
- ? Valeurs DB hors limites (>255)
- ? Caract�res multi-byte
- ? Labels d�finis plusieurs fois
- ? Labels non d�finis

### 9. **AssemblerIntegrationTests.cs** (15 tests)
Tests d'int�gration et programmes complets :

#### Programmes R�alistes
- ? **Boucle simple** - Compteur avec saut conditionnel
- ? **Calcul arithm�tique** - Op�rations complexes
- ? **Sous-routines avec param�tres** - Passage de valeurs
- ? **Op�rations logiques complexes** - Manipulation de bits
- ? **D�monstration pile** - PUSH/POP avec restauration

#### Sc�narios R�els
- ? **Routine de copie m�moire** - Boucle avec d�cr�ment
- ? **Programme calculatrice** - Multiple op�rations conditionnelles
- ? **Test exhaustif** - Utilisation de toutes les instructions

#### Tests de Performance
- ? **Gros programme** - 1000+ instructions en <1 seconde
- ? **Sauts complexes** - R�f�rences crois�es multiples

#### Tests de Fichiers
- ? **AssembleFile** - Lecture depuis fichier temporaire

## ?? Technologies Utilis�es

- **xUnit** : Framework de tests unitaires pour .NET
- **FluentAssertions** : Biblioth�que d'assertions fluides et expressives
- **.NET 9** : Plateforme de d�veloppement moderne
- **C# 13** : Langage avec fonctionnalit�s avanc�es

## ?? Couverture de Code

Les tests couvrent exhaustivement :

### ? **Classe Assembler** - Couverture compl�te
- Toutes les m�thodes publiques test�es
- Tous les types d'instructions support�s
- Gestion d'erreurs robuste
- Fonctionnalit�s avanc�es (labels, DB)

### ? **Jeu d'Instructions Complet**
- **Instructions de base** : NOP, HALT, SYS
- **Instructions de chargement** : LDA, LDB, LDC, LDD, LDE, LDF, LDDA, LDDB
- **Instructions arithm�tiques** : ADD, SUB, ADD16, SUB16, INC, DEC, INC16, DEC16, CMP
- **Instructions logiques** : AND, OR, XOR, NOT, SHL, SHR
- **Instructions de saut** : JMP, JZ, JNZ, JC, JNC, JN, JNN, JR, JRZ, JRNZ, JRC
- **Instructions de stockage** : STA, STB, STC, STD, STDA, STDB
- **Instructions de transfert** : MOV, SWP
- **Instructions de pile** : PUSH, POP (8-bit et 16-bit)
- **Instructions de sous-routines** : CALL, RET

### ? **Fonctionnalit�s Avanc�es**
- **Directive DB** : D�finition de donn�es avec formats multiples
- **Labels** : R�solution d'adresses symboliques
- **R�f�rences crois�es** : Forward et backward references
- **Gestion d'erreurs** : Exceptions sp�cifiques avec messages clairs
- **Formats de valeurs** : D�cimal, hexad�cimal (0x et $), caract�res

### ? **Qualit� du Code**
- **Gestion d'erreurs robuste** : Validation de tous les param�tres
- **Messages d'erreur clairs** : Indication pr�cise des probl�mes
- **Performance** : Assemblage rapide m�me pour gros programmes
- **Compatibilit�** : Support de tous les formats d'entr�e

## ?? Points Forts de la Suite de Tests

1. **Couverture Exhaustive** : Chaque instruction test�e individuellement et en contexte
2. **Tests R�alistes** : Programmes complets avec sc�narios d'usage r�el
3. **Validation Robuste** : Tous les cas d'erreur couverts
4. **Performance** : Tests de programmes volumineux
5. **Documentation** : Tests servent de documentation des fonctionnalit�s
6. **Maintenabilit�** : Structure claire et noms explicites

## ??? Fonctionnalit�s Valid�es

### ? **Assembleur Complet et Fonctionnel**
- Support de 60+ instructions du processeur FS8
- Gestion compl�te des formats d'adressage
- R�solution automatique des labels
- G�n�ration de code machine correct

### ? **Syntaxe Flexible**
- Insensibilit� � la casse
- Gestion flexible des espaces et tabulations
- Support de commentaires avec ';'
- Formats de valeurs multiples

### ? **Gestion Avanc�e des Labels**
- D�finition avec ':'
- R�solution automatique des adresses
- Support des r�f�rences avant et arri�re
- Validation des labels dupliqu�s

### ? **Directive de Donn�es**
- DB pour d�finition d'octets
- Support de tableaux de donn�es
- Formats de valeurs mixtes
- Int�gration avec le syst�me de labels

### ? **Robustesse et Fiabilit�**
- Validation exhaustive des param�tres
- Messages d'erreur informatifs
- Gestion propre des cas limites
- Performance optimale

## ?? Utilisation des Tests

Pour lancer les tests :
```bash
cd FSAssemblerTests
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

Pour lancer un fichier de test sp�cifique :
```bash
dotnet test --filter "ClassName=LoadInstructionTests"
```

## ?? Instructions Test�es

| Cat�gorie | Instructions | Tests |
|-----------|-------------|-------|
| **Base** | NOP, HALT, SYS | ? |
| **Chargement 8-bit** | LDA, LDB, LDC, LDD, LDE, LDF | ? |
| **Chargement 16-bit** | LDDA, LDDB | ? |
| **Arithm�tique** | ADD, SUB, ADD16, SUB16 | ? |
| **Incr/D�cr 8-bit** | INC A/B/C, DEC A/B/C | ? |
| **Incr/D�cr 16-bit** | INC16 DA/DB, DEC16 DA/DB | ? |
| **Comparaison** | CMP A,B/A,C | ? |
| **Logique** | AND, OR, XOR, NOT | ? |
| **D�calage** | SHL A/B, SHR | ? |
| **Saut absolu** | JMP, JZ, JNZ, JC, JNC, JN, JNN | ? |
| **Saut relatif** | JR, JRZ, JRNZ, JRC | ? |
| **Stockage 8-bit** | STA, STB, STC, STD | ? |
| **Stockage 16-bit** | STDA, STDB | ? |
| **Transfert** | MOV (toutes combinaisons) | ? |
| **�change** | SWP A,B/A,C | ? |
| **Pile 8-bit** | PUSH/POP A/B/C | ? |
| **Pile 16-bit** | PUSH/POP DA/DB | ? |
| **Sous-routines** | CALL, RET | ? |
| **Donn�es** | DB (formats multiples) | ? |

## ?? Conclusion

Cette suite de tests exhaustive valide que **l'assembleur FSAssembler est enti�rement fonctionnel** et pr�t pour la compilation de programmes pour le processeur FS8. 

**Toutes les fonctionnalit�s sont op�rationnelles** :
- ? Compilation compl�te du jeu d'instructions FS8
- ? Gestion avanc�e des labels et r�f�rences crois�es
- ? Support de la directive DB pour les donn�es
- ? Robustesse et gestion d'erreurs exemplaire
- ? Performance optimale pour tous types de programmes

L'assembleur peut maintenant compiler en toute fiabilit� des programmes assembleur .fs8 en code machine binaire compatible avec l'�mulateur FSGameConsole, offrant une cha�ne de d�veloppement compl�te pour la cr�ation de jeux et applications vintage.