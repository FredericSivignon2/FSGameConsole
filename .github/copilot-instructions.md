# Projet Emulateur console de jeux vintage

## Préliminaires :

BIEN UTILISER CE QUI EST DECRIT DANS LES PRELIMINAIRES !!!

1) Dans les commandes Powershell que tu donnes à executer, n'utilise jamais "&&" pour concaténer l'execution de 2 commandes, car cela ne fonctionne pas. Créés plutôt 2 commandes séparées.
   Prête particulièrement attention à ce détail. Donc, je me répète : Quand tu as plusieurs commandes à executer dans un shell (DOS ou Powershell), execute les une par une et n'essaye pas de les concaténer.
   Par exemple, il ne faut pas faire : "cd FSCPUTests && dotnet test", mais le faire en 2 temps : "cd FSCPUTests", puis, une fois que le "cd" est exécuté, faire "dotnet test". Sinon,
   Sinon, il y a une erreur dans la console et impossible de te faire continuer, tout est bloqué (je  pense que tu ne détectes pas de code de retour, du coup tu attends en boucle sans timeout).
2) Avant toute modification du code, assures toi de déjà bien connaitre l'existant, après analyse complète de tout le projet. De plus, ne supprime aucune fonction/méthode existante sans me demander confirmation
   et bien savoir pourquoi tu la ou les supprimes. En règle général, tout ajout de code ou de logique à l'existant ne doit pas casser les tests unitaires. Donc, sauf cas spécial, il ne devrait pas être nécessaire
   de modifier les tests unitaires existant avec l'ajout de nouvelles fonctionnalités. Cela ne vaut que si une fonctionalité existante doit être modifiée, ce qui sera de plus en plus rare.
3) Le projet utilise .NET 9, donc prend bien en compte les dernières nouveautés de cette version dans ce que tu codes.
4) N'hésites pas à commenter abondamment le projet pour qu'il soit facilement compréhensible.
5) Tous les commentaires doivent être rédigés en Anglais. Si tu trouves des commentaires en français, renommes les en Anglais.
6) Quand tu créés un code assembleur, fait attention aux instructions qui utilisent les même registres : Par exemple, si tu déclares un compteur de boucle en utilisant le registre A, que tu vas décrémenté jusqu'à
   0 pour terminer la boucle, n'utilise pas une autre instruction dans la boucle qui va aussi utiliser A, comme LDAIX1+ pour donner un exemple. Donc, tu dois t'assurer de la cohérence des instructions utilisées.
7) **IMPORTANT** : Les expressions d'adressage indexé comme `(IDX+5)` ou `(IDY-10)` ne peuvent pas être utilisées avec des instructions comme JMP ou CALL. Ces expressions sont uniquement
   supportées par les instructions LDA, LDB, STA, STB avec leur syntaxe spécialisée d'adressage indexé. Ne pas confondre avec les labels normaux.
8) **GESTION DES ERREURS** : Quand tu implémentes des validations d'erreurs dans l'assembleur, attention au fait que les `AssemblerException` sont encapsulées dans des exceptions de type
   "Ligne X" lors de l'assemblage. Pour les tests, il faut vérifier le message de l'inner exception, pas le message de premier niveau.
9) Quand de nouvelles instructions sont ajoutées pour le CPU8Bit, il faut :
   * Mettre à jour le fichier InstructionCycles.cs en conséquence : Analayse bien toutes les instructions (opcode) dans CPU8Bit, et assure toi que chacun des opcode est également représenté dans InstructionCycles. Assigne un nombre de cycle "plausible" aux nouveaux opcodes. Si tu penses avoir trouvé des opcodes qui existent dans InstructionCycles mais qui ne sont pas (ou plus) dans CPU8Bit, demande moi quoi faire avant de continuer.
   * Ensuite, il faut mettre à jour les tests unitaires liés à CPU8Bit. Utilises si possible un fichier de test existant (en fonction de la nature des nouvelles instructions), ne touche surtout pas aux tests existant, ne fait que rajouter les tests nécesaires pour couvrir les nouvelles instructions.
   * Après, mettre à jour Assembler.cs pour que les instructions assembleurs puissent couvrir les nouvelles instructions. Idem, assures toi de faire le point sur l'existant, pour ne rajouter que ce qui est nécessaire sans toucher à l'existant. De la même manière, si tu penses trouver des instructions dans Assembler.cs qui n'existe pas dans CPU8Bit, demandes moi.
   * Enfin, créé également des tests unitaires, si possible dans des fichiers existants adaptés au type d'instruction, pour les nouvelles instructions supportées dans Assembler.cs ; pour cela, analyse l'existant avec précision et demande moi si tu détectes quelque chose de non cohérent.
10) Dans tous les cas, créé un fichier par class. Evite de mettre plusieurs classes dans le même fichier, ce qui rend le fichier difficile à lire.

## Architecture globale du projet

Le projet FSGameConsole est un émulateur de console de jeux vintage, s'inspirant du style Amstrad CPC. Il se compose de **6 projets principaux** :

- **FSGameConsole** : Application WinForm principale (.NET 9)
- **FSCPU** (CPU8Bit) : Cœur de l'émulateur - processeur 8 bits, mémoire, périphériques. CPU8Bit.cs doit toujours être la "golden source" des opcodes supportés par l'émulateur. Donc, si tu vois dans d'autres fichiers, des opcodes qui ne sont pas dans CPU8Bit.cs, demande moi avant de faire une modification. Dans la plupart des cas, il faudra supprimer les anciennes références. Et inversement, si il y a des opcodes dans CPU8Bit.cs qui ne sont pas ailleurs où ils devraient être, il faut les rajouter (fichier Assembler.cs, test unitaires, test d'intégration etc.)
- **FSAssembler** : Assembleur pour compiler du code .fs8 en binaire exécutable
- **FSCPUTests** : Suite de tests unitaires complète (120+ tests)
- **FSAssemblerTests** : Suite de tests assembleur complète (280+ tests)
- **FSAssemblerEditorExtension** : Extension Visual Studio pour syntaxe highlighting (.NET Framework 4.7.2)

## Microprocesseur 8 bits FS8 (Enhanced)

### Registres

**Registres 8 bits :**

- A, B, C, D, E, F : 6 registres généraux pour données temporaires
- PC (Program Counter) : Pointeur sur la prochaine instruction (16 bits)
- SP (Stack Pointer) : Pointeur de pile (16 bits)
- SR (Status Register) : Flags Zero, Carry, Overflow, Negative

**Registres 16 bits (nouveauté) :**

- DA, DB : 2 registres 16 bits pour calculs accélérés et stockage étendu

**Registres d'index 16 bits (simplifiés) :**

- IDX, IDY : 2 registres d'index pour adressage avancé (simplifié par rapport à la v1 avec 4 registres)

### Architecture Little-Endian

Le processeur utilise l'organisation little-endian pour les valeurs 16 bits :

- Low byte stocké à l'adresse la plus basse
- High byte stocké à l'adresse + 1
- Exemple : 0x1234 → [0x34][0x12] en mémoire

### Jeu d'instructions étendu (120+ instructions)

Le processeur supporte maintenant **120+ instructions** organisées par catégories :

**Instructions de base (0x00-0x01) :**

- 0x00 : NOP - No Operation
- 0x01 : HALT - Stop processor

**Instructions de chargement 8 bits (0x10-0x15) :**

- 0x10 : LDA #imm - Load A immediate
- 0x11 : LDB #imm - Load B immediate
- 0x12 : LDC #imm - Load C immediate
- 0x13 : LDD #imm - Load D immediate
- 0x14 : LDE #imm - Load E immediate
- 0x15 : LDF #imm - Load F immediate

**Instructions de chargement 16 bits (0x16-0x1E) :**

- 0x16 : LDDA #imm16 - Load DA with 16-bit immediate
- 0x17 : LDDB #imm16 - Load DB with 16-bit immediate
- 0x18 : LDDA addr - Load DA from memory address
- 0x19 : LDDB addr - Load DB from memory address
- 0x1A : LDIDX #imm16 - Load IDX with 16-bit immediate
- 0x1B : LDIDY #imm16 - Load IDY with 16-bit immediate
- 0x1C : LDDC addr - Load C immediate (implémentation actuelle)
- 0x1D : LDDD addr - Load C immediate (bug - devrait charger D)
- 0x1E : LDDE addr - Load E immediate

**Instructions arithmétiques (0x20-0x2F) :**

- 0x20 : ADD A,B - 8-bit addition
- 0x21 : SUB A,B - 8-bit subtraction
- 0x22 : ADD16 DA,DB - 16-bit addition
- 0x23 : SUB16 DA,DB - 16-bit subtraction
- 0x24 : INC16 DA - Increment DA
- 0x25 : DEC16 DA - Decrement DA
- 0x26 : INC16 DB - Increment DB
- 0x27 : DEC16 DB - Decrement DB
- 0x28 : INC A - Increment A
- 0x29 : DEC A - Decrement A
- 0x2A : INC B - Increment B
- 0x2B : DEC B - Decrement B
- 0x2C : CMP A,B - Compare A with B
- 0x2D : INC C - Increment C
- 0x2E : DEC C - Decrement C
- 0x2F : CMP A,C - Compare A with C

**Instructions logiques (0x30-0x39) :**

- 0x30 : AND A,B - Logical AND
- 0x31 : OR A,B - Logical OR
- 0x32 : XOR A,B - Logical XOR
- 0x33 : NOT A - Logical NOT
- 0x34 : SHL A - Shift Left A
- 0x35 : SHR A - Shift Right A
- 0x36 : AND A,C - Logical AND A with C
- 0x37 : OR A,C - Logical OR A with C
- 0x38 : XOR A,C - Logical XOR A with C
- 0x39 : SHL B - Shift Left B

**Instructions de saut (0x40-0x46) :**

- 0x40 : JMP addr - Unconditional jump
- 0x41 : JZ addr - Jump if Zero
- 0x42 : JNZ addr - Jump if Not Zero
- 0x43 : JC addr - Jump if Carry
- 0x44 : JNC addr - Jump if No Carry
- 0x45 : JN addr - Jump if Negative
- 0x46 : JNN addr - Jump if Not Negative

**Instructions de stockage (0x50-0x57) :**

- 0x50 : STA addr - Store A at address
- 0x51 : STDA addr - Store DA at address (16-bit)
- 0x52 : STDB addr - Store DB at address (16-bit)
- 0x53 : STB addr - Store B at address
- 0x54 : STC addr - Store C at address
- 0x55 : STD addr - Store D at address
- 0x56 : STE addr - Store E at address
- 0x57 : STF addr - Store F at address

**Instructions de sous-routines (0x60-0x61) :**

- 0x60 : CALL addr - Call subroutine
- 0x61 : RET - Return from subroutine

**Instructions de pile (0x70-0x7F) :**

- 0x70 : PUSH A - Push A onto stack
- 0x71 : POP A - Pop from stack to A
- 0x72 : PUSH16 DA - Push DA onto stack (16-bit)
- 0x73 : POP16 DA - Pop from stack to DA (16-bit)
- 0x74 : PUSH B - Push B onto stack
- 0x75 : POP B - Pop from stack to B
- 0x76 : PUSH16 DB - Push DB onto stack (16-bit)
- 0x77 : POP16 DB - Pop from stack to DB (16-bit)
- 0x78 : PUSH C - Push C onto stack
- 0x79 : POP C - Pop from stack to C
- 0x7A : PUSH D - Push D onto stack
- 0x7B : POP D - Pop from stack to D
- 0x7C : PUSH E - Push E onto stack
- 0x7D : POP E - Pop from stack to E
- 0x7E : PUSH F - Push F onto stack
- 0x7F : POP F - Pop from stack to F

**Instructions de chargement mémoire (0x90-0x95) :**

- 0x90 : LDA addr - Load A from memory address
- 0x91 : LDB addr - Load B from memory address
- 0x92 : LDC addr - Load C from memory address
- 0x93 : LDD addr - Load D from memory address
- 0x94 : LDE addr - Load E from memory address
- 0x95 : LDF addr - Load F from memory address

**Instructions de transfert entre registres (0xA0-0xB3) :**

- 0xA0 : MOV A,B - Move B to A
- 0xA1 : MOV A,C - Move C to A
- 0xA2 : MOV B,A - Move A to B
- 0xA3 : MOV B,C - Move C to B
- 0xA4 : MOV C,A - Move A to C
- 0xA5 : MOV C,B - Move B to C
- 0xA6 : SWP A,B - Swap A and B
- 0xA7 : SWP A,C - Swap A and C
- 0xA8 : SWP A,D - Swap A and D
- 0xA9 : SWP A,E - Swap A and E
- 0xAA : SWP A,F - Swap A and F
- 0xAB : SWP DA,DB - Swap DA and DB
- 0xAC : MOV DA,DB - Move DB to DA
- 0xAD : MOV DB,DA - Move DA to DB
- 0xAE : SWP DA,IDX - Swap DA and IDX
- 0xAF : SWP DA,IDY - Swap DA and IDY
- 0xB0 : MOV DA,IDX - Move IDX to DA
- 0xB1 : MOV DA,IDY - Move IDY to DA
- 0xB2 : MOV IDX,DA - Move DA to IDX
- 0xB3 : MOV IDY,DA - Move DA to IDY

**Instructions de saut relatif (0xC0-0xC3) :**

- 0xC0 : JR offset - Jump relative with signed 8-bit offset
- 0xC1 : JRZ offset - Jump relative if Zero
- 0xC2 : JRNZ offset - Jump relative if Not Zero
- 0xC3 : JRC offset - Jump relative if Carry

**Instructions auto-increment/decrement (0xC4-0xCB) :**

- 0xC4 : LDAIDX+ - Load A from (IDX), then increment IDX
- 0xC5 : LDAIDY+ - Load A from (IDY), then increment IDY
- 0xC6 : STAIDX+ - Store A at (IDX), then increment IDX
- 0xC7 : STAIDY+ - Store A at (IDY), then increment IDY
- 0xC8 : LDAIDX- - Load A from (IDX), then decrement IDX
- 0xC9 : LDAIDY- - Load A from (IDY), then decrement IDY
- 0xCA : STAIDX- - Store A at (IDX), then decrement IDX
- 0xCB : STAIDY- - Store A at (IDY), then decrement IDY

**Instructions de comparaison immédiate (0xD0-0xD7) - NOUVELLES :**

- 0xD0 : CMP A,#imm - Compare A with immediate value
- 0xD1 : CMP B,#imm - Compare B with immediate value
- 0xD2 : CMP C,#imm - Compare C with immediate value
- 0xD3 : CMP D,#imm - Compare D with immediate value
- 0xD4 : CMP E,#imm - Compare E with immediate value
- 0xD5 : CMP F,#imm - Compare F with immediate value
- 0xD6 : CMP DA,#imm16 - Compare DA with 16-bit immediate value
- 0xD7 : CMP DB,#imm16 - Compare DB with 16-bit immediate value

**Instructions arithmétiques sur registres d'index (0xE0-0xE3, 0xE8, 0xEA) :**

- 0xE0 : INCIDX - Increment IDX
- 0xE1 : DECIDX - Decrement IDX
- 0xE2 : INCIDY - Increment IDY
- 0xE3 : DECIDY - Decrement IDY
- 0xE8 : ADDIDX #imm16 - Add 16-bit immediate to IDX
- 0xEA : ADDIDY #imm16 - Add 16-bit immediate to IDY

**Instructions de transfert d'index (0xF5, 0xF6, 0xF9) :**

- 0xF5 : MVIDXIDY - Move IDX to IDY
- 0xF6 : MVIDYIDX - Move IDY to IDX
- 0xF9 : SWPIDXIDY - Swap IDX and IDY

**Instructions étendues (0xFE-0xFF) :**

- 0xFE : Extended instruction set 1 (réservé pour extensions futures)
- 0xFF : Extended instruction set 2 (réservé pour extensions futures)

## Instructions d'adressage indexé - Guide d'utilisation

### Syntaxes supportées :

1. **Auto-increment/decrement** : `LDAIDX+`, `STAIDY-` - 1 octet
2. **Chargement immédiat** : `LDIDX #0x8000` - 3 octets

### Exemples pratiques :

```assembly
; Copie efficace de tableau avec auto-increment
LDIDX #SOURCE_ARRAY    ; Pointeur source
LDIDY #DEST_ARRAY      ; Pointeur destination
LDB #10                ; Compteur
COPY_LOOP:
    LDAIDX+            ; Charge et incrémente source
    STAIDY+            ; Stocke et incrémente destination
    DEC B              ; Décrémente compteur
    JNZ COPY_LOOP      ; Continue si pas fini
```

### Limites importantes :

- Auto-increment/decrement limité aux registres IDX/IDY avec registre A
- Les expressions complexes comme (IDX+5) ne sont PAS implémentées dans cette version

## Organisation mémoire (Style Amstrad CPC authentique)

Le système utilise un plan mémoire de 64KB organisé comme suit :

- 0x0000-0x7FFF : RAM programme (32KB)
- 0x8000-0xBFFF : Mémoire vidéo bitmap unifiée (16KB) - Style CPC authentique
- 0xC000-0xF3FF : RAM étendue (13KB)
- 0xF400-0xF7FF : ROM BOOT (1KB) - **ZONE ROM PROTÉGÉE**
- 0xF800-0xFFFF : BIOS/Système (2KB)

### Zones importantes :

- **Zone programme** : 0x0000-0x7FFF (programmes utilisateur)
- **Mémoire bitmap** : 0x8000-0xBFFF (écran 320x200, 16 couleurs)
- **ROM BOOT** : 0xF400-0xF7FF (code de démarrage système)
- **Pile** : Démarre à 0xFFFF et descend

## Système vidéo CPC authentique

### Caractéristiques techniques :

- **Résolution** : 320x200 pixels
- **Couleurs** : 16 couleurs simultanées (palette CPC)
- **Modes** : Bitmap unifié + rendu texte 8x8
- **Police** : Amstrad CPC authentique (256 caractères)

### Composants vidéo :

- **VideoController** : Gestion de l'affichage, curseur, couleurs
- **BitmapRenderer** : Rendu optimisé vers WinForm
- **AmstradCPCFont** : Police bitmap 8x8 authentique

## Gestionnaire d'horloge réaliste

Le système intègre un **ClockManager** pour un timing authentique :

### Caractéristiques :

- **Fréquence** : 1 MHz (1 million de cycles/seconde)
- **Timing par instruction** : Chaque opcode a son nombre de cycles
- **Exécution asynchrone** : Timer haute précision
- **Métriques** : CPS (Cycles/sec) et IPS (Instructions/sec) en temps réel

### Cycles par instruction (exemples) :

- NOP, HALT : 1 cycle
- LDA, LDB, etc. : 2 cycles (opcode + data)
- ADD, SUB : 1 cycle (registres internes)
- JMP : 3 cycles (opcode + address 16-bit)
- STA : 4 cycles (opcode + address + write)
- CALL : 5 cycles (fetch addr + push + jump)
- SYS : 8 cycles (system call overhead)
- Instructions d'index : 1-4 cycles selon la complexité

## ROM de démarrage

Le système dispose d'une **ROM BOOT** de 1KB à 0xF400 contenant :

### Contenu actuel :

- Signature système : "FS System v1.0"
- Police Amstrad CPC complète (256 caractères × 8 octets)
- Routines de démarrage
- Gestionnaire PrintChar et PrintString

### Fonctionnalités :

- **Cold Boot** : Effacement complet + rechargement ROM
- **Warm Boot** : Reset CPU seulement
- **Protection ROM** : Zone en lecture seule

## Assembler FSAssembler

L'assembleur compile les fichiers **.fs8** en binaires exécutables :

### Fonctionnalités :

- **Instructions** : Support complet du jeu d'instructions (120+ instructions)
- **Labels** : Résolution des adresses symboliques
- **Données** : Directive DB pour octets bruts
- **Commentaires** : Support des commentaires avec ';'
- **Compilation** : Génération de fichiers .bin
- **Instructions CMP immediate** : Support complet des nouvelles comparaisons

### Usage :

```
FSAssembler input.fs8 output.bin
```

## Tests unitaires complets

### Suite FSCPUTests (120+ tests) :

- **CPU8BitTests** : Tests du processeur principal (28 tests)
- **ExtendedCPU8BitTests** : Tests des instructions étendues (25+ tests)
- **MissingOpcodeCPU8BitTests** : Tests des opcodes manquants (50+ tests)
- **ALU16BitCompareTests** : Tests des comparaisons 16-bit (10+ tests)
- **QuickCompareImmediateTests** : Tests rapides des CMP immediate (5+ tests)
- **MemoryTests** : Tests de la mémoire (14 tests)
- **ALUTests** : Tests de l'unité arithmétique (26 tests)
- **StatusRegisterTests** : Tests des flags (18 tests)
- **AdvancedCPU8BitTests** : Tests d'intégration avancés (10+ tests)

### Suite FSAssemblerTests (280+ tests) :

- **AssemblerBasicTests** : Tests de base de l'assembleur (18 tests)
- **LoadInstructionTests** : Tests instructions de chargement (32 tests)
- **CompareImmediateInstructionTests** : Tests CMP immediate (40+ tests) **NOUVEAU**
- **Push16Pop16Tests** : Tests PUSH16/POP16 (25+ tests)
- **ExtendedInstructionTests** : Tests instructions étendues (60+ tests)
- **MiscellaneousInstructionTests** : Tests instructions diverses (30+ tests)
- **ArithmeticInstructionTests** : Tests arithmétiques (25 tests)
- **LogicalInstructionTests** : Tests logiques (22 tests)
- **JumpInstructionTests** : Tests de sauts (28 tests)
- **StoreTransferInstructionTests** : Tests stockage/transfert (20 tests)
- **StackSubroutineInstructionTests** : Tests pile/sous-routines (24 tests)
- **DataAndLabelTests** : Tests données et labels (18 tests)
- **AssemblerIntegrationTests** : Tests d'intégration assembleur (15 tests)

### Points de test :

- Toutes les instructions individuellement
- Programmes complexes multi-instructions
- Gestion des flags et des erreurs
- Opérations 16 bits
- Sauts conditionnels
- Pile et sous-routines
- Système mémoire authentique
- **Instructions CMP immediate** : Tous les registres 8-bit et 16-bit
- **Correction ALU 16-bit** : Fix du bug des comparaisons 16-bit
- **Programmes réalistes** : ROM boot, routines système

### Notes importantes pour les tests :

Dans les tests unitaires (pas ceux d'intégration), il faut utiliser `_cpu.Start(false);` avec l'argument `false` pour que le timer ne se déclenche pas.
On veut pouvoir contrôler ce qui est executé, donc ne pas risquer de passer à l'instruction suivante alors que l'on veut tester l'execution de l'instruction
définie dans le test.

## Interface utilisateur WinForm

### Fonctionnalités principales :

- **Écran émulé** : Panel 320x200 avec rendu bitmap
- **Contrôles CPU** : Start/Stop/Reset/ColdBoot/Step
- **Affichage registres** : État en temps réel (incluant IDX, IDY)
- **Dump mémoire** : RAM, bitmap, ROM
- **Chargement** : Programmes .bin et démonstrations
- **Métriques** : Performance CPU en temps réel

### Nouveautés interface :

- **Timing réaliste** : Plus de timer manuel, tout géré par ClockManager
- **Rendu optimisé** : BitmapRenderer pour performance
- **Debug avancé** : Affichage multi-zones mémoire
- **System calls** : Démonstrations interactives
- **Registres simplifiés** : Affichage IDX/IDY (simplifié par rapport à 4 registres)

## Extension Visual Studio

Le projet **FSAssemblerEditorExtension** fournit :

### Fonctionnalités :

- **Syntax Highlighting** : Coloration syntaxique pour fichiers .fs8
- **Classification** : Instructions, registres, commentaires, labels
- **Content Type** : Support natif des fichiers .fs8 dans Visual Studio
- **Package VSIX** : Installation simple via Visual Studio

### Technologies :

- **.NET Framework 4.7.2** : Requis par l'API Visual Studio
- **MEF (Managed Extensibility Framework)** : Architecture extensible
- **Visual Studio SDK** : API d'extensibilité

## Problématiques techniques résolues

### 1. Timing authentique

**Problème** : Timing CPU artificiel
**Solution** : ClockManager avec cycles par instruction

### 2. Organisation mémoire

**Problème** : Zones vidéo séparées
**Solution** : Mémoire bitmap unifiée style CPC

### 3. Jeu d'instructions limité

**Problème** : Seulement ~20 instructions
**Solution** : Extension à 120+ instructions avec support 16 bits et registres d'index

### 4. Tests insuffisants

**Problème** : Couverture partielle
**Solution** : 280+ tests assembleur + 120+ tests CPU couvrant tous les aspects

### 5. Comparaisons 16-bit défectueuses

**Problème** : Bug majeur dans ALU.Compare(ushort, ushort) - flags incorrects
**Solution** : Correction de la logique de comparaison 16-bit avec tests exhaustifs

### 6. Instructions CMP immediate manquantes

**Problème** : Pas de comparaisons avec valeurs immédiates
**Solution** : Ajout des opcodes 0xD0-0xD7 avec support assembleur complet

### 7. Extension Visual Studio

**Problème** : Pas de support éditeur pour .fs8
**Solution** : Extension complète avec syntax highlighting et content type

## Développements futurs possibles

### Améliorations planifiées :

1. **Sons** : Chip audio simple 3 canaux
2. **Interruptions** : IRQ/NMI pour timing avancé
3. **Périphériques** : Joystick, clavier étendu
4. **Debugger** : Interface debug avancée
5. **Jeux** : Bibliothèque de jeux de démonstration
6. **Optimisations** : JIT compilation pour performance

### Extensions assembleur :

1. **Macros** : Support des macros assembleur
2. **Modules** : Système d'inclusion de fichiers
3. **Optimisations** : Optimiseur de code
4. **Debugging** : Symboles de debug intégrés

## Architecture du code

### Design patterns utilisés :

- **Facade** : CPU8Bit encapsule ALU, Memory, etc.
- **Observer** : DisplayTimer pour rendu
- **Strategy** : SystemCallManager pour appels système
- **Factory** : RomManager pour gestion ROM
- **Template Method** : InstructionCycles pour timing

### Bonnes pratiques :

- **Tests unitaires** : Couverture >95%

Une chose à faire assez importante quand tu créés du code assembleur (pour les tests unitaires par exemple). Il faut bien indiquer la position en mémoire de chaque
instruction assembleur. En suivant le même formattage que l'exemple suivant :

```csharp
string[] lines =
{                     // pos  size
    "START:",         //  -    0   Label
    "JMP FORWARD1",   //  0    3   Forward reference
    "",
    "BACK1:",         //  -    0   Label (position 3)
    "JMP FORWARD2",   //  3    3   Forward reference
    "NOP",            //  6    1"",
    "BACK2:",         //  -    0   Label (position 7)
    "JMP END",        //  7    3   Forward reference
    "",
    "FORWARD1:",      //  -    0   Label (position 10)
    "JMP BACK1",      // 10    3   Backward reference
    "",
    "FORWARD2:",      //  -    0   Label (position 13)
    "JMP BACK2",      // 13    3   Backward reference
    "",
    "END:",           //  -    0   Label (position 16)
    "HALT"            // 16    1   Halt
};
```

Grâce à cela, il est beaucoup plus simple de debugger les tests ou de modifier le code.

- **Documentation** : Commentaires XML complets
- **Separation of concerns** : Responsabilités bien définies
- **Disposal pattern** : Nettoyage des ressources
- **Error handling** : Gestion d'erreurs robuste
- **Extension architecture** : Support Visual Studio intégré

Cette architecture offre une base solide pour un émulateur vintage performant et extensible, avec un timing authentique, des instructions CMP immediate complètes, une correction des bugs ALU 16-bit, et une excellente couverture de tests avec support éditeur professionnel.
