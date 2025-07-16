# Project Emulateur console de jeux vintage

## Préliminaires :

1) Dans les commandes Powershell que tu donnes à executer, n'utilise jamais "&&" pour concaténer l'execution de 2 commandes, car cela ne fonctionne pas. Créés plutôt 2 commandes séparées.
   Prête particulièrement attention à ce détail. Donc, je me répète : Quand tu as plusieurs commandes à executer dans un shell (DOS ou Powershell), execute les une par une et n'essaye pas de les concaténer.
   Par exemple, il ne faut pas faire : "cd FSCPUTests && dotnet test", mais le faire en 2 temps : "cd FSCPUTests", puis, une fois que le "cd" est exécuté, faire "dotnet test". Sinon,
   Sinon, il y a une erreur dans la console et impossible de te faire continuer, tout est bloqué (je  pense que tu ne détectes pas de code de retour, du coup tu attends en boucle sans timeout).
2) Avant toute modification du code, assures toi de déjà bien connaitre l'existant, après analyse complète de tout le projet.
3) Le projet utilise .NET 9, donc prend bien en compte les dernières nouveautés de cette version dans ce que tu codes.
4) N'hésites pas à commenter abondamment le projet pour qu'il soit facilement compréhensible.
5) Tous les commentaires doivent être rédigés en Anglais. Si tu trouves des commentaires en français, renommes les en Anglais.

## Architecture globale du projet

Le projet FSGameConsole est un émulateur de console de jeux vintage, s'inspirant du style Amstrad CPC. Il se compose de 4 projets principaux :

- **FSGameConsole** : Application WinForm principale (.NET 9)
- **FSCPU** (CPU8Bit) : Cœur de l'émulateur - processeur 8 bits, mémoire, périphériques
- **FSAssembler** : Assembleur pour compiler du code .fs8 en binaire exécutable
- **FSCPUTests** : Suite de tests unitaires complète (116+ tests)

## Microprocesseur 8 bits FS8 (Enhanced)

### Registres

**Registres 8 bits :**

- A, B, C, D, E, F : 6 registres généraux pour données temporaires
- PC (Program Counter) : Pointeur sur la prochaine instruction (16 bits)
- SP (Stack Pointer) : Pointeur de pile (16 bits)
- SR (Status Register) : Flags Zero, Carry, Overflow, Negative

**Registres 16 bits (nouveauté) :**

- DA, DB : 2 registres 16 bits pour calculs accélérés et stockage étendu

### Architecture Little-Endian

Le processeur utilise l'organisation little-endian pour les valeurs 16 bits :

- Low byte stocké à l'adresse la plus basse
- High byte stocké à l'adresse + 1
- Exemple : 0x1234 → [0x34][0x12] en mémoire

### Jeu d'instructions étendu

Le processeur supporte maintenant **60+ instructions** organisées par catégories :

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

**Instructions de chargement 16 bits (0x16-0x1B) :**

- 0x16 : LDDA #imm16 - Load DA with 16-bit immediate
- 0x17 : LDDB #imm16 - Load DB with 16-bit immediate
- 0x18 : LDDA addr - Load DA from memory address
- 0x19 : LDDB addr - Load DB from memory address
- 0x1A/0x1B : Legacy 8-bit loads for DA/DB (compatibility)

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

**Instructions de stockage (0x50-0x55) :**

- 0x50 : STA addr - Store A at address
- 0x51 : STDA addr - Store DA at address (16-bit)
- 0x52 : STDB addr - Store DB at address (16-bit)
- 0x53 : STB addr - Store B at address
- 0x54 : STC addr - Store C at address
- 0x55 : STD addr - Store D at address

**Instructions de sous-routines (0x60-0x61) :**

- 0x60 : CALL addr - Call subroutine
- 0x61 : RET - Return from subroutine

**Instructions de pile (0x70-0x79) :**

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

**Appels système (0xF0) :**

- 0xF0 : SYS - System call (voir section système d'appels)

## Organisation mémoire (Style Amstrad CPC authentique)

Le système utilise un plan mémoire de 64KB organisé comme suit :
0x0000-0x7FFF : RAM programme (32KB)
0x8000-0xBFFF : Mémoire vidéo bitmap unifiée (16KB) - Style CPC authentique
0xC000-0xF3FF : RAM étendue (13KB)
0xF400-0xF7FF : ROM BOOT (1KB) - **ZONE ROM PROTÉGÉE**
0xF800-0xFFFF : BIOS/Système (2KB)

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

## ROM de démarrage

Le système dispose d'une **ROM BOOT** de 1KB à 0xF400 contenant :

### Contenu actuel :

- Signature système : "FS System v1.0"
- Police Amstrad CPC complète (256 caractères × 8 octets)
- Routines de démarrage
- Gestionnaire PrintChar

### Fonctionnalités :

- **Cold Boot** : Effacement complet + rechargement ROM
- **Warm Boot** : Reset CPU seulement
- **Protection ROM** : Zone en lecture seule

## Assembler FSAssembler

L'assembleur compile les fichiers **.fs8** en binaires exécutables :

### Fonctionnalités :

- **Instructions** : Support complet du jeu d'instructions
- **Labels** : Résolution des adresses symboliques
- **Données** : Directive DB pour octets bruts
- **Commentaires** : Support des commentaires avec ';'
- **Compilation** : Génération de fichiers .bin

### Usage :FSAssembler input.fs8 output.bin

## Tests unitaires complets

La suite de tests **FSCPUTests** contient **116+ tests** couvrant :

### Couverture :

- **CPU8BitTests** : Tests du processeur principal (28 tests)
- **ExtendedCPU8BitTests** : Tests des nouvelles instructions (27 tests)
- **MemoryTests** : Tests de la mémoire (14 tests)
- **ALUTests** : Tests de l'unité arithmétique (26 tests)
- **StatusRegisterTests** : Tests des flags (18 tests)
- **IntegrationTests** : Tests d'intégration (10 tests)

### Points de test :

- Toutes les instructions individuellement
- Programmes complexes multi-instructions
- Gestion des flags et des erreurs
- Opérations 16 bits
- Sauts conditionnels
- Pile et sous-routines
- Système mémoire authentique

  Dans les tests unitaires (pas ceux d'intégration), il faut utiliser "_cpu.Start(false);" avec l'argument "false" pour que le timer ne se déclenche pas.
  On veut pouvoir contrôler ce qui est executé, donc ne pas risquer de passer à l'instruction suivante alors que l'on veut tester l'execution de l'instruction
  définie dans le test.

## Interface utilisateur WinForm

### Fonctionnalités principales :

- **Écran émulé** : Panel 320x200 avec rendu bitmap
- **Contrôles CPU** : Start/Stop/Reset/ColdBoot/Step
- **Affichage registres** : État en temps réel
- **Dump mémoire** : RAM, bitmap, ROM
- **Chargement** : Programmes .bin et démonstrations
- **Métriques** : Performance CPU en temps réel

### Nouveautés interface :

- **Timing réaliste** : Plus de timer manuel, tout géré par ClockManager
- **Rendu optimisé** : BitmapRenderer pour performance
- **Debug avancé** : Affichage multi-zones mémoire
- **System calls** : Démonstrations interactives

## Problématiques techniques résolues

### 1. Timing authentique

**Problème** : Timing CPU artificiel
**Solution** : ClockManager avec cycles par instruction

### 2. Organisation mémoire

**Problème** : Zones vidéo séparées
**Solution** : Mémoire bitmap unifiée style CPC

### 3. Jeu d'instructions limité

**Problème** : Seulement ~20 instructions
**Solution** : Extension à 60+ instructions avec support 16 bits

### 4. Tests insuffisants

**Problème** : Couverture partielle
**Solution** : 116+ tests couvrant tous les aspects

### 5. Rendu vidéo basique

**Problème** : Affichage texte simple
**Solution** : VideoController + BitmapRenderer authentique

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

### Bonnes pratiques :

- **Tests unitaires** : Couverture >95%
  Une chose à faire assez importante quand tu créés du code assembleur (pour les tests unitaires par exemple). Il faut bien indiquer la position en mémoire de chaque
  instruction assembleur. En suivant le même formattage que l'exemple suivant :
  
  string[] lines = 
	{                     // pos  size
		"START:",         //  -    0   Label
		"JMP FORWARD1",   //  0    3   Forward reference
		"",
		"BACK1:",         //  -    0   Label (position 3)
		"JMP FORWARD2",   //  3    3   Forward reference
		"NOP",            //  6    1   
		"",
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
	
	Grâce à cela, il est beaucoup plus simple de debugger les tests ou de modifier le code.
  
- **Documentation** : Commentaires XML complets
- **Separation of concerns** : Responsabilités bien définies
- **Disposal pattern** : Nettoyage des ressources
- **Error handling** : Gestion d'erreurs robuste

Cette architecture offre une base solide pour un émulateur vintage performant et extensible, avec un timing authentique et une excellente couverture de tests.

## Exemple de l'Amstrad CPC

Pour te donner un peu de contexte et des exemples concrets, voici le contenu du livre "la bible du programmeur de l'amstrad CPC". Notre processeur virtuel n'est pas identique au Z80 de l'Amstrad. Donc, il ne faudra pas, dans tes suggestions, vouloir faire "comme le Z80" pour m'aider par exemple à écrire du code assembleur.
Il faudra toujours bien prendre en compte les spécificités de notre processeur virtuel FS8. Par contre, des astuces d'optimisation, des exemples divers et variés issus du Z80, peuvent nous aider à améliorer notre propre code assembleur, surtout dans la création de notre ROM qui est quasi vierge actuellement.
Voici donc le contenu du livre en intégralité :

INTRODUCTION

Lorsque nous avons reçu le premier CPC à l'automne 1984, nous avons été
d'abord assez sceptiques. ‘Un parmi tant d’autres’ avons-nous pensé avant
de découvrir la puissance de cet ordinateur.

La taille du présent ouvrage ainsi que son contenu montrent que nous
avons Vite changé radicalement de point de vue.

Le CPC est une machine fantastique qui offre actuellement un rapport
entre le prix et les possibilités de l'ordinateur qui n'a pas de
concurrent. Dans la classe de prix qui est la sienne, le CPC représente
une nouvelle dimension. Plusieurs points sont décisifs à cet égard:
d'abord, le fait qu'il s'agisse d’un système complet. Grâce au moniteur
livré avec l'appareil, pas de dispute pour savoir si on regarde Dallas ou
si on utilise l'ordinateur. De même, le lecteur de cassette intégré rend
inutiles les câbles de connexion, le réglage du volume et les interfaces
qui faisaient de l'utilisation du lecteur de cassette un problème
permanent. Votre ordinateur possède tout ce dont vous avez besoin pour
pouvoir l'utiliser immédiatement.

Les possibilités de l'ordinateur sont un second point fort de ce
matériel, Le Basic LOCOMOTIVE est certainement le meilleur disponible sur
les ordinateurs de cette catégorie. La programmation des interruptions
très souple et très facile d'emploi dont dispose ce Basic est
certainement un des aspects les plus remarquables de cet ordinateur.

L'ex cellence du graphisme et la possibilité d'avoir un écran en 80
colonnes sans module ni coût supplémentaire est pour l'heure sans
équivalent, alors que d'autres ordinateurs de la même catégorie ont déjà
du mal à présenter sur l'écran 40 caractères par ligne parfaitement
lisibles.

La résolution graphique de 640 points sur 200 est également unique pour
cette catégorie de prix. On ne trouve de possibilités comparables que sur
l'IBM PC qui est tout de même au moins cinq fois plus cher que le CPC,
Les possibilités sonores du CPC sont également impressionnantes,

En ce qui concerne la vitesse, le CPC n'a pas à rougir. Le
microprocesseur intégré Z80 fonctionne avec une fréquence de 4 mégaherz
et Il dispose d’un Jeu d'instructions très puissant, Ce Jeu
d'instructions a été exploité au maximum par les développeurs de la

machine qui ont ainsi réussi à réaliser un interpréteur Basic
particulièrement rapide.

Mais les possesseurs d’un nouvel ordinateur cherchent en général très
Vite à obtenir plus d'informations sur leur machine. Le manuel
d'utilisation du CPC, qui est par ailleurs tout à fait remarquable, ne
suffit pas à répondre à l'attente de ceux qui veulent connaître leur
ordinateur dans les moindres détails et notamment pour ceux, pour qui le
Basic a perdu un peu de son attrait, qui en ont découvert les limites et
qui souhaiteraient donc s'attaquer à la programmation en langage-machine.
Il faut alors disposer d'informations qui dépassent largement le cadre du
manuel d'utilisation.

Ce sont ces informations que le présent ouvrage met à votre disposition.
Ce livre est le résultat de nuits et de Journées de travail consacrées au
CPC.

Vous trouverez ici une description détaillée du matériel (hardware) avec
un schéma, un listing du système d'exploitation et du Basic très
complètement commenté, les adresses importantes de la RAM mais aussi des
instructions Basic qui ne sont pas décrites dans le manuel. Vous
trouverez également de petits trucs concernant l'utilisation du lecteur
de cassette et de l'imprimante ainsi que la programmation du graphisme en
langage-machine,

Nous espérons que les informations que nous vous fournissons vous seront
utiles et vous permettront de connaître encore mieux votre CPC,

Les auteurs

1 LE MATERIEL (HARDWARE)

1.1 Ce que vous devez absolument savoir sur votre machine

Vous n'avez pas encore pris votre tournevis pour observer la vie interne
de cette “bofte magique"? Cela ne fait rien, nous vous avons évité ce
travail de dévissage et nous avons photographié le résultat.
L'illustration 1.1.0,1 montre à quoi ressemble l'intérieur de votre
machine.

Ce ne sont pas plus de 25 circuits intégrés qui sont disposés sur une
plaque de taille importante. Ce n'est donc pas à une électronique
particulièrement coûteuse que le CPC doit sa puissance et c'est plutôt la
partie logiciel (software) qui rend cet ordinateur extraordinaire et qui
explique également le prix particulièrement bas auquel le système complet
est proposé. Les quelques composants électroniques qui constituent le CPC
ne reviennent en effet pas très cher.

Seuls 9 circuits intégrés représentent la mémoire dont dispose votre CPC.
Huit composants du type 4164 constituent les Rams, la mémoire de travail
de l'ordinateur, Le neuvième circuit intégré de mémoire, est une ROM de
32 kilo-octets. Le processeur Z80 du CPC ne peut cependant, comme tout
processeur 8 bits adresser qu’une zone de 64 kilo-octets et cette zone
est entièrement remplie par les composants Ram,

L'adressage apparemment impossible de 96 kilo-octets a cependant été
obtenu grâce à un truc de programmation très subtile connu sous le nom de
bank-switching (commutation de banques de mémoire). Mais ce n'est pas
tout! Théoriquement, 11 est possible de connecter au CPC Jusqu'à un
maximum de 252 ROMS externes de 16 K chacune, qui pourraient alors être
adressées par bank-switching. La’zone ainsi adressable est donc d'environ
4 méga-octets!

Le CPC contient en outre comme composants hautement intégrés un video-
controller HD 6845, un port parallèle 8255, un chip sonore AY-3-8912 et
un élément appelé gate array, qui a été développé spécialement pour le
CPC.

Le contrôleur vidéo a pour fonction de fournir tous les signaux
nécessaires pour le fonctionnement du moniteur, Il adresse également la

mémoire-écran, cette zone de la mémoire dans laquelle sont placés les
caractères à représenter et le graphisme. Il produit également le refresh
qui est nécessaire pour les Rams, sans lequel vous perdriez vite les
informations stockées,

La tâche du chip sonore est définie par le nom de ce composant. Le choix
des constructeurs est très bon. Le AY-3-8912 a été utilisé dans de
nombreux ordinateurs parce qu'il est très polyvalent et qu’il permet des
possibilités étendues d’influencer le son.

Le 8255 est le “travailleur de force” du CPC. Ses tâches sont très
diverses,

Cela va du contrôle du clavier à la commande du chip sonore en passant
par la commande du magnétophone, à la détermination de certaines
possibilités du CPC etc...

Le gate array est particulièrement intéressant. Ce composant commande
tant de choses dans le CPC qu'on pourrait presque le qualifier de
processeur auxiliaire. C'est ainsi qu'il prend en charge bon nombre des
tâches concernant l'écran, telles que la représentation des différentes
couleurs et les différents formats de l'écran. Tous les signaux
nécessaires de synchronisation sont produits par le gate array. Les
interruptions, qui interrompent le déroulement normal des programmes 300
fois par seconde, sont produites par le gate array ainsi que les signaux
nécessaires à la gestion de la mémoire 96 K du CPC.

Le schéma 1.1.0.2 montre comment les différents composants travaillent
ensemble.

1.1.1 La disposition de la mémoire

Il y a encore 5 ans, les ordinateurs disposant de 16 K de RAM étaient
considérés comme bien armés. Mais depuis l'apparition du Commodore 64,
les limites de la mémoire ont été nettement repoussées. Un constructeur
de micro-ordinateurs n'a de chances suffisantes de prendre une part du
marché que si les magiques 64 apparaissent sur sa machine.

cn Eh

last rneatas st amma age,

1

canaaegee ass Lasane gg,

Jr 7e
DES

Le CPC dispose lui aussi d’une RAM de 64 K = 65536 cases mémoire. Il
possède en plus une ROM intégrée de 32 K.

D'ailleurs, il n’est pas très difficile de placer une mémoire de 64 K
dans un ordinateur puisque les processeurs 8-bits, qui sont les plus
répandus, peuvent tous adresser une zone de 64 kilo-octets. Le Z80 du CPC
peut lui aussi adresser 64 K de mémoire sans truc particulier. Mais cela
suffit normalement tout Juste pour la mémoire RAM et c'est tout.

11 faut donc recourir à un procédé spécial, le bank-switching, si l'on
veut pouvoir adresser plusieurs mémoires avec ce type de processeurs, Ce
procédé permet en effet de choisir entre des zones de mémoire (qu’on
appelle banques) ROM et RAM qui se chevauchent. Il s’agit d’un procédé
qui n'utilise pas de solution matériel mais a uniquement recours à un
logiciel qui organise la cohabitation de la ROM et de la RAM aux mêmes
adresses. Cette solution logiciel a été remarquablement mise en oeuvre
par les développeurs de l'ordinateur.

Le CPC présente donc l'image suivante: 64 K de RAM sont adressés
directement. Parallèlement à la RAM se trouvent une moitié de la ROM dans
les 16 K inférieurs (80000 à &3FFF) et l’autre moitié de la ROM dans les
16 K supérieurs (8C000 à &FFFF).

Les 16 K inférieurs de ROM contiennent le système d'exploitation et un
bloc de routines arithmétiques, Dans le système d'exploitation se
trouvent toutes les routines dont le CPC a besoin pour lire par exemple
un caractère tapé au clavier, pour placer un caractère ou un point sur
l'écran mais c'est également le système d'exploitation qui commande le
lecteur de cassette et l'interface imprimante ainsi que le son,

Dans les 16 K supérieurs se trouve l'interpréteur Basic. Ces 16 K n'ont
pas de fonction spéciale. Il est possible de connecter dans cette zone
jusqu’à 252 ROMs supplémentaires. C’est ainsi que les routines
nécessaires pour la gestion du lecteur de disquette sont placées dans une
ROM qui ‘partage’ cette zone avec le Basic.

La disposition de la mémoire est représentée par la figure 1.1.1.1

1.1.2 Extension d'instructions à travers RST

Etant donné ce mode de gestion de la mémoire, on peut cependant se
demander comment peut se faire l'accès aux ROMs ou aux RAMS situées dans
les même zones. Pour éviter aux utilisateurs le travail de programmation
assez considérable que nécessiteraient normalement ces tâches, les
programmeurs du système d'exploitation ont eu une riche idée. Grâce à des
programmes spéciaux et à une utilisation habile des instructions RESTART
du Z80, ils ont pratiquement abouti à faire des restarts RSTI à RSTS une
extension du jeu d'instructions du Z80, Ces RSTs peuvent être employés
comme des JPs ou des CALLS ordinaires. Certains RSTS réclament par
ailleurs une adresse sur 3 octets. Le troisième octet, supplémentaire,
détermine dans quelle ROM le JP ou le CALL doit aller.

LOW_JUMP__RST 1

Cette instruction Restart permet d'appeler une routine du système
d'exploitation ou de la RAM située dans la même zone d'adresses,
L'instruction RST doit être suivie immédiatement par l'adresse de la
routine à appeler, Comme 14 bits suffisent pour définir une adresse
comprise entre O0 et &3FFF, les deux bits supérieurs restants sont
utilisés pour sélectionner la ROM ou la Ram:

Bit 14=0 Sélection du système d'exploitation
Bit 14=1 Sélection de la Ram

Bit 15=-0 Sélection de la ROM Basic

Bit 15=1 Sélection de la Ram

Un appel de la routine système pourrait donc se présenter ainsi:

RST 1
DW &1410+&8000

Le bit 15 mis sélectionne la RAM dans la zone de &C000 à &FFFF, alors que
le bit 14 annulé appelle le système d'exploitation.

Le code à l’adresse 8 est constitué uniquement par un saut à l'adresse
&B982,

SIDE CALL RST 2

Cette instruction Restart permet d'appeler une routine d'une ROM
d'extension. Cette instruction est utilisée lorsqu'un programme sous
forme d’un module de ROM nécessite plus de 16 kilo-octets et ne peut pas
tenir dans un seul module d'extension. Le SIDE CALL permet alors
d'appeler une routine se trouvant dans la seconde, la troisième ou la
quatrième ROM appartenant au programme, sans qu’il soit pour cela
nécessaire de connaître le numéro absolu de la ROM qu'il s’agit d'appeler
ainsi, L’instruction RST 2 doit être suivie de l’adresse de la routine -
eC000, c’est-à-dire de l'adresse relative par rapport au début de la ROM.
Les deux bits supérieurs servent à sélectionner l'une des quatre ROMs
différentes utilisées.

Le code à l'adresse 80010 est constitué uniquement par un saut à
l'adresse 8BA16.

FAR CALL_ RST 3

Cette instruction Restart permet d'appeler une routine n'importe où en
ROM ou en RAM. L’instruction RST 3 doit être suivie de l'adresse sur deux
octets d’un bloc de paramètres composé de trois octets. Les deux premiers
de ces octets-paramètres contiennent l'adresse de la routine qui doit
etre appelée et le troisième octet doit contenir l'état ROM/RAM souhaité,
Les valeurs de 0 à 251 permettent d'appeler une ROM supplémentaire et les
quatre valeurs restantes ont la fonction suivante:

Valeur 80000-83FFF &C000-8FFFF
252 Système d'exploitation Basic

253 Système d'exploitation RAM

254 RAM Basic

255 RAM RAM

Le code à l'adresse 80018 est constitué uniquement par un saut à
l'adresse 8B9BF.

RAM LAM_RST #4

Cette instruction Restart permet de lire à partir d’un programme en
langage-machine le contenu de la RAM, quel que soit l’état de la ROM
choisi. L'instruction RST 4 remplace alors l'instruction

LD A, (HL)

HL doit donc contenir l'adresse de la case mémoire dont le contenu doit
etre lu. Le code à l'adresse 80020 est constitué uniquement par un saut à
l'adresse &BACB.

FIRM JUMP_RST 5

Cette instruction Restart permet de sauter à une routine du système
d'exploitation. L'adresse doit être placée immédiatement à la suite de
l'instruction RST 5. La ROM du système d'exploitation est sélectionnée
avant le saut à la routine puis elle est déconnectée après le retour. Le
code à l'adresse 80028 est constitué uniquement par un saut à l'adresse
&BA2E,

1.1.0,2. Scnéma de blocs du CPC

-9-

1.2 Le processeur 780

Le début des années 70 a connu le triomphe des microprocesseurs, La
société INTEL a pu se tailler avec le processeur 8080 une part
significative du marché parce qu’au moment où elle le lança sur le
marché, il n'avait pratiquement pas de concurrent dans cette catégorie,
C'est bien ce qui frappe quand on examine de plus près les données
techniques de ce processeur. Le 8080 avait en effet besoin de trois
tensions de courant différentes et de deux circuits intégrés
supplémentaires pour la production des signaux de commande et de
synchronisation.

La société ZILOG a développé le Z80 dans les années 74/75, Mais au lieu
de développer un processeur radicalement nouveau, on s'en tint à la
conception du 8080 qui avait rencontré un tel succès, C’est pourquoi le
180 est compatible avec le 8080 (mais non pas l'inverse). C'est-à-dire
que tous les programmes écrits pour un 8080 tournent aussi sur un Z80.

Cependant toutes les particularités considérées comme néfastes du 8080
furent éliminées et le Jeu d'instructions fut largement étendu. Le Z80 ne
nécessite d’autre part qu'une tension de +5Volt et il n'a pas besoin de
circuits intégrés externes pour produire les signaux de commande,

Mais examinons en style télégraphique les données techniques de ce
processeur, avant que nous n'entrions plus dans le détail de ses
caractèristiques:

Processeur 8-bits de technologie NMOS

Bus d'adresses 16-bits

Alimentation unique 5 Volt

Horloge simple

Compatible TTL

Fréquence d'horloge de 2.5, 4, 6 ou même 8 MHz
Compatibilité logiciel avec le 8080

Double jeu de registres plus deux registres d’index

Entrée d’interruptions non-masquable

Entrée d’interruptions masquable avec trois modes de travail
Refresh automatique de RAMsS dynamiques

Circuits intégrés périphériques du 8080 directement
connectables

-10-

Ces données techniques ainsi qu’un grande masse de logiciels disponibles
ont fait du Z80 l'un des processeurs 8-bits les plus répandus, Dans le
domaine des ordinateurs familiaux et personnels, seul le 6502 a obtenu
une diffusion comparable.

1.2.1 Les connexions du Z80

Après ce bref aperçu sur les possibilités du Z80, intéressons-nous
maintenant à l’affectation des 40 pins de connexion du Z80,

Les points de connexion du Z80 peuvent être répartis entre les 4 groupes
bus de données, bus d'adresses, bus de commande et canaux de
transmission.

Bus d'adresses

AO - A15 : Lignes d'adresses; ces connexions permettent d'appeler une
case mémoire dans la zone adressable qui comprend 65536 cases
mémoire. Dans le traitement des instructions d’entrée-sortie,
les 8 bits inférieurs de l'adresse sont utilisés pour sortir
l'adresse d’entrée-sortie correspondante. 256 ports différents
sont ainsi possibles. Avec certaines limites tenant au Jeu
d'instructions, ce sont même 65536 ports qui peuvent être
adressés, Les 16 canaux d'adresse sont alors utilisés pour
constituer l'adresse du port, Nous reviendrons plus tard sur
ce cas particulier.

Bus de données
DO - D7 : Lignes de données: ces canaux bidirectionnels transmettent les
données venant du processeur ou allant vers le processeur.

Elles font le lien entre le processeur et la case mémoire ou
l'adresse de port choisies à travers le bus d'adresses.

-11-

Bus de commande

Mi* |

MREQ*

I0RQ*

RD*

WR*

RESET* _:

NML* !

Machine Cycle One; ce signal de commande indique que le
processeur lit le code d'instruction sur le bus de données.
L'étoile signifie par ailleurs pour ce signal et pour les
signaux suivants, qu’il s’agit d’un signal actif avec low.

Memory REQuest*, ce signal de sortie indique par un 1ow que le
processeur entreprend un accès en lecture ou écriture à une
adresse de la mémoire et que l'adresse sur le bus d'adresses
est valable.

Input/Output ReQuest*, ce signal de sortie indique par un 1ow
que le processeur entreprend un accès en lecture ou écriture à
une adresse de port et que l'adresse de port sur le bus
d'adresses est valable.

ReaD*, ce signal de sortie indique par un low que le processeur
veut lire des données dans une case mémoire ou dans une adresse
de port. L'utilisation conjointe avec MREQ* ou IORQ permet de
distinguer entre la lecture de la mémoire ou d’un port.

WRite*, ce signal indique, lors d'accès en écriture du
processeur à la mémoire ou aux adresses de port, que les
données figurant sur le bus de données sont valables. Ici
aussi, l’utilisation conjointe de WR* avec MREG* ou IORQ*
permet de distinguer si les données doivent être écrites dans
la mémoire ou dans une adresse de port.

Lorsque ce signal d'entrée passe à low, le compteur de
programme reçoit la valeur 88000, les interruptions sont
interdites et le mode d'interruption 0 est activé. Dès que ce
signal d'entrée redevient high, le processeur commence
l'exécution du programme à partir de l'adresse 20000.

Non Maskable Interrupt*, ce signal d'entrée provoque toujours
par un double signal high-lon une interruption du programme
exécuté par le processeur. Les valeurs placées en 80066 et
80067 sont alors chargées dans le compteur de programme et le
programme se poursuit à partir de cet endroit.

-12-

IRQ* : Interrupt ReQuest*, ce signal d'entrée peut provoquer par un

WAIT*

BUSRQ*__:

BUSAK*_:

HALT*

RFSH*__:

low une interruption du programme exécuté par le processeur, à
condition que ce type d'’interruptions soit autorisé par
instruction. Les effets dépendent du type d'interruption et
seront évoqués plus tard, IRQ* est, au contraire de NMI*, un
signal statique qui doit persister jusqu’à ce que la demande
d'interruption ait été prise en compte.

Ce signal permet d'adapter l'accès en lecture ou en écriture du
180 à des mémoires plus lentes ou à des conditions spéciales du
système,

BUSReQuest*; losque ce signal d'entrée passe à low, les canaux
de données et d'adresses ainsi que tous les canaux de commande
de sortie deviendront high après le traitement de l'instruction
actuelle et le signal BUSAK* deviendra low, Maintenant, un
second processeur pourrait prendre en charge l'accès à la
mémoire et aux éléments périphériques; ce signal est cependant
essentiellement utilisé pour le DMA (DMA= Direct Memory Access,
transfert de données très rapide en contournant le processeur).

BUSAKnowledge*, est le signal de sortie correspondant à BUSRO*,
Un low indique au DMA controller ou au second processeur que
tous les signaux de commande et de bus sont high et qu'un accès
est maintenant possible.

Ce signal de sortie devient low après que le processeur ait
exécuté l'instruction en langage-machine HALT, Après cette
instruction, le processeur ne fait plus rien d'autres que
d'exécuter des NOPs pour assurer le Refresh. Seule une
interruption peut à nouveau le “réveiller”.

ReFreSH*, ce signal de sortie indique que les sept canaux
d'adresses inférieurs contiennent une adresse de Refresh
Valable, Comme le processeur n’a besoin du bus d'adresses et de
données qu'à certains moments, le bus d'adresses peut être
utilisé le reste du temps pour rafrafchir les RAMs dynamiques,
sans qu'une électronique coûteuse ou des routines spéciales de
rafrafchissement ne soient pour cela nécessaires.

—13-

Horloge et_alimentation électrique

0 Ë

GND

Vec

Le signal d'entrée phi sert d'horloge pour le processeur. Comme
le Z80 est un circuit intégré statique, la fréquence d'horloge
peut varier entre 0 Hertz et la fréquence maximale indiquée. La
forme du signal d'horloge doit cependant répondre à certaines
exigences. Le durée low de ce signal ne doit pas dépasser 2
microsecondes,. Cette valeur n'a d’ailleurs qu'un intérêt
théorique, puisqu'on essaiera évidemment toujours de fournir au
processeur une fréquence d'horloge la plus élevée possible, de
façon à obtenir une exécution rapide du programme.

Branchement à la masse du processeur.
C'est par cette connexion que le Z80 reçoit son alimentation en

courant électrique continu de 5 Volts et environ 150 à 200
milliampères.

-14-

1.2.2 LA STRUCTURE DES REGISTRES DU 780

Comme nous l'avons indiqué au début, le Z80 a été construit de telle
façon que les programmes du 8080 puissent être repris sans problème. Mais
le Z80 dispose d’un nombre de registres nettement supérieur.

Mais qu'est-ce donc qu'un registre?

Un registre n’est rien d'autre qu'une mémoire de lecture/écriture sur le
chip du processeur. Chaque processeur doit disposer d’un minimum de
registres. Dans ces cases de mémoire, les données peuvent être placées,
ainsi que les résultats d'instructions arithmétiques et logiques.
D'autres registres ont des fonctions spéciales, telles que la gestion de
la pile, ou sont utilisés comme compteur de programme.

Comme les opérations telles qu’un transfert de données entre deux
registres ou l'addition des contenus de deux registres ne peuvent se
faire à travers le bus de données, de telles opérations peuvent être
exécutées beaucoup plus rapidement que lorsque les valeurs nécessaires
doivent être recherchées dans des cases de mémoire externes.

On peut donc dire en règle générale que les processeurs disposant d’une
mémoire interne plus importante sont supérieurs aux processeurs disposant
de peu de registres pour le traitement des mêmes programmes car le
transfert de données est toujours plus rapide à l’intérieur du processeur
qu'entre le processeur et les cases de mémoire externes.

Le Z80 dispose de 22 registres au total, 18 registres de 8 bits et 4

registres de 16 bits. La figure 1.2,2,1 montre la disposition de ces
registres. :

-15-

A 11
A 12
A 13
A 14
A 15

D 4
D 3
D 5
D 6
+5V
D 2
D 7
D 0
D 1
INT*

HALT"*
MREO*
IORO*

DOO'O LU DUO 00 OGTOe 0e

1.2.1,1, Pinout du Z80
-16-

OO TO D OO EEE ECrEE

A 10

A 9

A 8

A 7

A 6

A 5

A 4

A 3

A 2

A 1

A 0
GND
RFSH*
M1*
RESET*
BUSRO*
WAIT*
BUSAK*
WR*
RD*

Dans cette figure, certains registres sont marqués par un cadre plus
épais. Ces registres existent également sur le 8080.

Vous voyez également que la plupart des registres 8 bits apparaissent en
double exemplaire. 11 s'agit des registres À, F, B, C, D, E, H et L. Le
programmeur peut choisir entre ces deux Jeux de registres.

Nous ne parlerons à l'avenir que d’un seul jeu de registres, d'autant que
le programmeur du CPC ne dispose en fait, à moins de recourir à
certaines astuces particulières, que d’un seul jeu de registres. Le Jeu
de registres alternatif est utilisé par le système d'exploitation pour la
gestion des interruptions. Mais notez bien que toutes les tâches d'un Jeu
de registres peuvent également être prises en charge par le Jeu de
registres alternatif, si celui-ci n'est pas employé pour des opérations
spécifiques.

Les registres B à L sont les registres 8 bits normalement disponibles,
alors que les registres À et F répondent à des tâches particulières,

Le registre À est généralement qualifié d’accumulateur, C’est dans
l’accumulateur qu’on obtient le résultat de toutes les opérations
arithmétiques et logiques sur 8 bits. Pour ces opérations, un opérande
doit d'autre part être placé dans l’accumulateur. Pour additionner par
exemple deux octets, il faut placer un opérande dans l'accumulateur alors
que le second opérande peut être placé dans un autre registre du
processeur ou dans une case de la mémoire externe. Après l'addition, le
résultat se trouve dans l'accumulateur.

Comme, lors de telles opérations, le résultat peut être supérieur à la
valeur maximale qui peut être exprimée avec 8 bits (255+255=510), un bit
Supplémentaire est nécessaire pour représenter le résultat correctement.
C'est le registre F qui remplit cette fonction. Le registre F,
généralement qualifié de registre flag est divisé en ses différents bits.
Un de ces bits a entre autre pour fonction de conserver une éventuelle
retenue (carry en anglais) résultant de telles additions. Les autres bits
indiquent si le résultat d'opérations de calcul ou de comparaisons est
nul, etc...

Les registres B à L ne peuvent toutefois pas uniquement être appelés
Séparément. B et C, D et E ainsi que H et L peuvent être regroupés en
registres 16 bits. Ces registres 16 bits reçoivent alors naturellement
les noms BC, DE et HL. Les registres doubles conviennent parfaitement à

-17-

l'adressage de tableaux ainsi qu’au transfert et à la recherche de blocs
de données.

Le registre double HL a une signification particulière. Comme le Z80
dispose d'instructions d'addition et de soustraction sur 16 bits, le
registre HL fait office, pour de telles instructions, d'accumulateur 16
bits.

Les registres PC, SP, IX et IY ne travaillent qu'avec des valeurs 16 bits
(remarque: les spécialistes savent qu'il est également possible de
manipuler les registres d'index octet par octet mais nous ne
considèrerons IX et IŸ que comme de purs registres 16 bits).

Le registre PC est le compteur de programme (Programm Counter). Le
contenu du PC est placé sur le bus d'adresse comme adresse pour les
mémoires externes, Avec chaque instruction, le PC est incrémenté
(augmenté de 1) automatiquement. Pour les instructions sur plusieurs
octets, le PC est automatiquement augmenté de la valeur correspondant à
ce nombre d'octets, Si des sauts doivent se produire à l'intérieur d'un
programme, la nouvelle adresse du programme est automatiquement chargée
dans le PC et le processeur continue l'exécution à partir de cette
adresse.

Le registre SP est le pointeur de pile (Stack Pointer). La pile est
utilisée lorsque des sous-programmes sont appelés. Dans ce cas en effet,
l'adresse de retour est automatiquement placée sur la pile puis rechargée
dans le PC après exécution du sous-programme,

Les deux registres 16 bits IX et IY permettent grâce à des instructions
spéciales un travail particulièrement efficace avec les tableaux.

Il ne reste plus que les registres I et R. Le registre I ou registre
d'interruption (Interrupt Register) est utilisé en liaison avec le mode
d'interruption spécial IM3, Dans ce mode d'interruption, l'élément
produisant l'interruption doit fournir, à la demande du processeur, une
valeur 8 bits. Cette valeur comme low byte et le contenu du registre I
comme high byte constituent l'adresse de la routine d'interruption.

Le registre R ou Refresh Register est utilisé en liaison avec le Refresh
que le Z80 exécute automatiquement. Chaque fois qu'une instruction a été
retirée, les sept bits inférieurs de ce registre sont automatiquement
incrémentés. Le huitième bit reste toujours à 1 ou à O0, suivant sa

-18-

programmation.

Les registres I et R ne sont pas utilisés sur le CPC. Cependant, comme la
valeur du registre R se modifie sans cesse, celui-ci peut être utilisé
comme générateur de hasard.

1.2.3 Particularités du Z80 du CPC

Les nombreuses possibilités du Z80 laissent une grande marge de manoeuvre
aux concepteurs de matériel ou de logiciel dans la construction d'un
ordinateur. Cette CPU (unité centrale) peut être utilisée avec la même
efficacité dans des systèmes très réduits ainsi que dans des machines
aussi puissantes que le CPC,

Les développeurs du CPC se sont ingéniés à obtenir un maximum de
puissance avec un minimum de composants. D’où certaines particularités
qu'il est nécessaire de connaître pour pouvoir programmer et utiliser
efficacement cette machine, particulièrement en langage-machine. Ce sont
ces particularités que nous allons maintenant étudier.

Tout d'abord la gestion des interruptions du CPC.

La seule source d'interruptions du CPC est le gate array, ce composant
fantastique qui contribue de façon décisive à la puissance de cet
ordinateur, Toutes les 3,3 millisecondes, soit 300 fois par seconde, le
gate array produit une brève impulsion qu'il place sur l'entrée IRG* du
180. L'entrée NMI* du processeur n'est pas utilisée et est disponible sur
le connecteur d'extensions pour des extensions éventuelles.

La fréquence du signal d'interruptions est obtenue, à partir du signal H-
Sync du CRTC 6845, au moyen d'un diviseur de fréquence. L'’impulsion H-
sync qui apparaît environ toutes les 65 microsecondes est ici divisée par
52,

Comme le Z80 fonctionne sur le CPC en mode d'interruption IM1, chaque
interruption IRQ identifiée provoque un RST7 ou encore un CALL 80038. Le
processeur interrompt immédiatement le programme en cours, place l'état
actuel du PC sur la pile et saute à l'adresse 80038. Ici figure, sur le
CPC, un saut à l'adresse 8B939 où se trouve la routine d'interruption
proprement dite. Comme l'endroit où s'est produit l'interruption est
enregistré sur la pile, le programme interrompu peut être repris une fois
terminée la routine d'interruption.

Comme l'entrée IRG* du processeur se trouve également sur le connecteur
d'extension, on peut bien sûr se demander comment une interruption par le
gate array peut être distinguée d'une interruption externe. Les

-19-

développeurs du CPC ont eu ici recours à une astuce. A l’intérieur de la
routine d'interruption en 8B939, l'interruption est à nouveau autorisée
un court instant. Comme l'impulsion produite par le gate array ne dure
pas plus de 5 microsecondes, cette autorisation de l'interruption n’a
aucun effet, puisque l'impulsion est terminée depuis longtemps. Par
contre, les sources externes d'interruption ne mettent fin à l'émission
de leur signal que sur instruction expresse du processeur. Lorsqu'il y a
une interruption externe, la routine d'interruption est donc elle-même
interrompue. Ce cas peut être identifié et traité d’une manière spéciale.
C'est ainsi que sont rendues également possibles les sources
d'interruptions externes. La seule condition qu'elle doivent remplir,
c'est une impulsion suffisamment longue.

Le second cas particulier qui doit être pris en compte, c'est la
possibilité limitée d'utiliser les instructions de port.

En liaison avec le signal IORQG* (Input/Output ReQuest), le Z80 peut
adresser un maximum de 256 ports différents, de façon analogue à
l'adressage de cases mémoire. Pour cela, l'adresse du port souhaité est
placée dans les 8 bits inférieurs d'adresse AO à A7. Ces ports sont
essentiellement utilisés pour connecter des éléments périphériques.

Sur d'autres processeurs qui ne connaissent pas l’adressage de port, le
concepteur est toujours tenté d'adresser les éléments périphériques comme
des cases mémoire. Ce procédé est appelé Memory Mapped et il présente
l'inconvénient de réduire la zone d'adresses disponible pour la RAM.

Pour l'utilisation de l'adressage de port, le Z80 fournit le groupe très
puissant des instructions IN et OUT. Si l’on étudie plus attentivement
les instructions de ce groupe, on trouve dans les instructions IN(C),r et
OUT(C),r une possibilité élégante d'adresser plus que les 256 ports
normalement prévus. Dans ces instructions, l'état des 8 bits d'adresse
inférieurs est déterminé par le contenu du registre C mais le contenu de
B est en outre placé dans les bits d'adresse A8 à A15, C’est ainsi 65536
adresses de ports qui sont disponibles, C'est Justement cette
caractéristique du Z80 que les concepteurs du CPC ont utilisée, Tous les
circuits intégrés périphériques sont sélectionnés au moyen des bits
d'adresse A8 à A15.

De telles astuces ont malheureusement souvent un inconvénient, En
l'occurence l'inconvénient réside dans une nette limitation du Jeu
d'instructions du Z80. Aucune des autres instructions 1/0 du Z80 ne peut
plus être utilisée. Ceci vaut notamment pour les instruction 1/0 avec
automatisme de boucle. Ces instructions utilisent le registre B comme

-20-

compteur et ne peuvent donc pas ‘fournir’ le highbyte de l'adresse de
port. C'est en particulier le cas des instructions INI, INIR, IND et INDR
ainsi que OUTI, OTIR, OUTD et OTDR.

L'utilisation des cycles wait constitue une troisième particularité du
CPC,

La nécessité de cette connexion du Z80 remonte à l'époque où les circuits
intégrés de mémoire disponibles se la coulaient encore douce. Les
premières EPROMs notamment n'étaient pas en mesure de préparer les
données, après réception de l'adresse, avant un délai de quelques
microsecondes.

Pour faire fonctionner le Z80 avec de tels ‘paresseux’, il fallait
attendre un certain temps. Ce délai peut être produit par le signal
WAIT*, Lors de chaque signal négatif sur l'entrée de l'horloge, le
processeur examine l'état de la connexion WAIT*, Si cette connexion est à
O Volt, le Z80 exécute ce que l'on appelle un cycle d'attente de la durée
d'un mouvement d'horloge. Une fois écoulé le signal d'horloge, donc avec
le signal négatif, l'état du canal WAIT* est à nouveau examiné, etc..
L'utilisation de ce signal sur le CPC n'a cependant aucun rapport avec
les circuits intégrés de mémoire utilisés, Ils sont tous suffisamment
rapides pour un Z80 d'une fréquence de 4 MHz, La raison de l’utilisation
de cette connexion est la nécessaire synchronisation entre processeur et
contrôleur vidéo. Comme les deux circuits intégrés peuvent accéder à la
mémoire, il faut contrôler de qui c’est le tour à un moment donné. Le
contrôleur vidéo est d’ailleurs toujours prioritaire car sinon
l'affichage sur le moniteur pourrait être sérieusement endommagé. Pour
obtenir cette synchronisation, un signal WAIT* est produit pour le
processeur tous les 4 mouvements d'horloge. Bien que le processeur
fonctionne à 4 MHz (Méga Hertz= millions de vibrations par seconde), du
fait des cycles d'attente, la fréquence de travail effective est
d'environ 3,3 MHz.

Ce ralentissement de la vitesse de l'ordinateur n’est pas très grave en
soi. Ce qui est plus gênant, c'est que les durée d'exécution des
instructions correspondant aux données techniques fournies pour le
processeur sont inexactes en ce qui concerne le CPC. C’est ainsi qu’il
devient très difficile de réaliser des boucles de temporisation très
précises telles qu’elles sont nécessaires par exemple pour utiliser des
formats d'écriture sur cassette spéciaux et particulièrement rapides.

Les signaux BUSRG* et BUSAK*, les signaux de commande du DMA ne sont pas

-21 _

utilisés sur le CPC, Ils sont cependant placés sur le connecteur
d'extension et sont donc disponibles pour des extensions externes.

Le signal HALT*, qui n'est pas non plus utilisé sur le CPC est également
disponibles sur le connecteur d'extension.

FFFF

16 K 16K
(VIDEO) BASIC
RAM ROM

16 K
RAM
16 K
RAM
16 K
RAM

1,1,1.1, Organisation de la mémoire du CPC
-22-

Cooo

8000

4000

18K
ROM
Système
d’exploit.

1.3 Le gate array, le coordinateur du système

Presque tous les composants du CPC se trouvent couramment dans le
commerce, dans n'importe quel magasin d'électronique bien approvisionné.
Les seules exceptions sont la ROM et le gate array qui est désigné dans
le schéma technique sous le nom de IC116, C'est ce dernier circuit
intégré qui nous occupera dans cette section.

Ce circuit intégré à 40 pôles a été développé spécialement pour le CPC et
11 remplit plusieurs fonctions importantes, Si l’on voulait reconstituer
toutes les fonctions intégrées avec des portes logiques TTL, le nombre de
circuits intégrés ferait vite plus que doubler.

Les fonctions du gate array sont entre autres les suivantes:

Production de toutes les fréquences d'horloge nécessaires
Production des signaux pour l'exploitation de la RAM dynamique
Commande des accès à la RAM

Connexion et déconnexion de la ROM sur la zone de mémoire
Production des signaux vidéo

Production des informations RVB pour le moniteur couleur
Commande du mode d'écran

Stockage des couleurs d'encre

Production de l'impulsion d'interruption

Il n'y a malheureusement que très peu d'informations disponibles sur ce
circuit intégré très intéressant. Il est impossible d'obtenir une
description technique de ce circuit intégré dont la vie interne est
visiblement considérée par le constructeur comme un secret de
fabrication.

Mais nos efforts et tentatives de découvrir le fonctionnement de ce
circuit intégré de la façon la plus détaillée possible ont débouché sur
un réel succès et nous ne voulons pas vous cacher les résultats auxquels
nous avons abouti.

-23-

ARE

1.2.2.1. Jeu de registres 780

1,3,1 L'affectation des pôles de connexion du gate array

Le signal qui détermine tout sur le CPC est le signal quarz d'une
fréquence de 16 MHz qui se trouve sur le pin 8 (XTAL). Le IC125, un
circuit intégré TTL du type 7400, constitue avec deux de ses quatre
portes logiques une commutation d'oscillateur typique, Ce signal
constitue pratiquement le battement cardiaque du CPC.

La fréquence d'entrée divisée par quatre est disponible pour le
processeur, sous la forme d'un signal d'horloge de 4 MHz sur le pin 39
comme mouvement d'horloge

Une nouvelle division par quatre donne une fréquence de 1 MHz, Ce signal
est fourni sur le pin 1 du gate array.

Le signal de 1 MHz a deux emplois. C'est tout d’abord le signal d’horloge
pour le chip sonore et il contribue ensuite à déterminer si le processeur
ou le CRTC peut adresser la RAM. S'il y a un low, les canaux d'adresse du
processeur sont commutés sur la RAM à travers les circuits intégrés
multiplexeurs IC 104, 105, 109 et 113,

Comme par ailleurs la commande de la RAM sur le CPC n'est pas tout à fait
évidente, vous trouverez une description détaillée des signaux de
commande de la RAM dans un prochain chapitre.

Comme les composants de mémoire ne disposent que de 8 canaux d'adresse,
l'adresse totale de 16 bits doit être multiplexée, c'est-à-dire placée
sur les entrées avec un décalage dans le temps. Cette commande dans le
temps est obtenue avec les signaux CAS ADDR*(pin 6), CAS*(pin 3) et
RAS* (pin 7). Ces signaux RAS* et CAS* sont placés directement vers les
RAMS, le signal CAS ADDR* est conduit vers les multiplexeurs que nous
avons déjà évoqués.

Le signal MAO/CCLK sur le pin 40 du gate array a également une fréquence
de 1 MHz, Ce signal est par ailleurs déphasé par rapport au signal CPU
ADDR*, c'est-à-dire que les deux fréquences sont high à des moments
différents. MAO/CCLK a également une double fonction. Il constitue d'une
part le signal d'horloge pour le CRTC qui tire tous les autres signaux de
ce signal; d'autre part il est placé comme bit d'adresse auxiliaire sur
le multiplexeur IC 106. La fonction de ce bit d'adresse auxiliaire sera

-25-

également évoquée plus tard plus précisément, à propos de la commande de
la RAM.

Le gate array produit encore sur le pin 13 le signal RAMRD*. Cette
connexion devient low, lorsque le processeur, après avoir fourni une
adresse, veut lire des données dans la RAM et qu’il l'indique au gate
array par son signal RD* sur le pin 19. Comme la ROM et la RAM se
chevauchent sur de grandes zones, le signal RD* du processeur ne peut
etre utilisé directement. Si des données doivent être lues dans la ROM,
le signal RAMRD* reste high et les sorties du IC114, qui est ce qu'on
appelle un buffer (mémoire provisoire) deviennent high. Dans ces moments,
aucune information ne peut passer de la RAM sur le bus de données, bien
que l'adresse de la mémoire soit également parvenue à la RAM et que
celle-ci tienne un octet prêt dans ses sorties.

En plus du RAMRD*, le signal READY du pin 2 du gate array est placé sur
l’IC114,. Ce signal produit sur le processeur le signal pour l'intégration
des cycles d'attente. La liaison supplémentaire entre le READY et 1’1C114
permet d'obtenir que l'information sur le bus de données du processeur ne
se modifie pas pendant les cycles d'attente, Le 74LS373 stocke, après
envoi d’un high, l'information en sortie actuelle sur le pin 11, jusqu’à
ce que ce pôle devienne low. Le circuit intégré se comporte ensuite comme
un simple buffer, c'est-à-dire que les sorties suivent immédiatement les
modifications des entrées.

Le signal ROMEN* sur le pin 12 du gate array devient low lorsque le
processeur veut lire des données dans la ROM. La ROM intégrée de 32 K du
CPC occupe les zones d'adresses 80000 à 83FFF et e8C000 à &FFFF. Cette ROM
peut donc être appelée en deux moitiés distinctes. Dans les zones de
mémoire où RAM et ROM se chevauchent, il faut indiquer au gate array le
choix fait avec une instruction OUT. Il est ainsi tout à fait possible de
n'activer qu'une moitié de 1a ROM.

Conformément à la configuration de la mémoire choisie, le gate array
décode l'état des canaux d'adresse A14 et A15. Suivant la mémoire
demandée c'est le signal RAMRD* ou ROMEN* qui sera activé lors de la
lecture,

Une instruction d'écriture du processeur va toujours vers la RAM,

indépendamment de la configuration de la mémoire choisie. Le gate array
produit à cet effet le signal MWE*.

-26-

Outre la fonction décrite, les canaux d'adresse A14 et A15 sur les pins
20 et 21 sont encore utilisés dans un autre but. Le gate array a une
adresse de port qui est utilisée pour programmer les différentes
possibilités du gate array. L'adresse de port est 87F00 et elle est
décodée sur le pin 18, à travers les canaux d'adresse (A14 Hign, A15 Low)
et le signal IORG*,

Comme le bus de données du Z80 n'est pas directement relié aux canaux de
données DO à D7 du gate array, le GA (gate array) met le pôle 244EN* sur
low lorsque l'adresse de port 87F00 est identifiée de la façon que nous
avons indiquée. Les sorties de 110115 (74LS244, un buffer de bus de
données) sont ainsi libérées et l'octet fourni par le Z80 peut être écrit
dans le GA.

Mais le signal I0RQ* a lui aussi une double signification pour le GA, Le
180 a en effet la particularité, lorsqu'il identifie une interruption, de
mettre simultanément à low les signaux IORQ* et M1*. Cette situation est
identifiée par le GA et l'impulsion d'interruption est immédiatement
annulée, Si par contre, le traitement de l’IRQ a été interdit par
l'instruction DI, Disable Interrupt, le pôle 10 du GA reste low, Jusqu'à
ce que l'’IRQ soit à nouveau autorisé. Dès que l'’IRQ est à nouveau
autorisé par l'instruction El, Enable Interrupt, l'interruption présente
est identifiée et la sortie d'interruption redevient high,

Le signal d'interruption sur le pin 10 est produit par une chaîne de
division programmable du GA. Cette chaîne de division est alimentée par
le signal HSYNC du CRTC et elle divise la fréquence existante par 52.
Comme l'impulsion HSYNC se produit environ toutes les 65 microsecondes,
l'intervalle entre deux impulsions d'interruption est de 3,3
millisecondes. Les impulsions sont couplées avec le signal VSYNC du CRTC.
La durée du VSYNC est programmée dans le CRTC à environ 500
microsecondes. Après environ 125 microsecondes apparaît l'interruption,
de sorte que la routine d'interruption a encore environ 375 microsecondes
pour examiner sur le bit O du port B du 8255 s’il y a un VSYNC. Ce signal
est utilisé comme horloge dans différentes opérations.

Ce cas ne se produit cependant que toutes les 15 interruptions, pour les
14 interruptions restantes, 11 y a un high du VSYNC et le compteur
interne n'est pas affecté.

Mais les signaux HSYNC et VSYNC sont bien sûr nécessaires, de même que
DISPEN pour produire le signal vidéo, Une liaison de ces signaux donne le

-27-

signal SYNC* sur le pin 11 du GA.

CPU ADDR*
READY
CAS"*
244EN*
MWE*

CAS ADDR*
RAS"

XTAL

Vec2
INTERRUPT*
SYNC*
ROMEN*
RAMRD*
HSYNC
VSYNC
IORO*

M1*

MREO*

RD*

A 15

CROLOREP EF EE EL OLERC EME ER EENS
COTE ae, d'Or SEC

1.3,1.1. Pinout du Gate Array

-28-

MA0/CCLK
(n)

Vccl
RESET"*

R

GND

« A
a
a
N

CRC CC |
N° OO D OU

Ù
M ©
È

Vec1
A 14

1.3,2 La structure des registres du gate array

L'exécution de toutes les tâches que nous avons décrites nécessite que
les données soient stockées dans le GA. Le nombre exact des registres
internes n’est pas connu mais nous pensons pouvoir décrire les registres
les plus importants,

Comme tous les autres éléments du CPC, le GA est appelé à travers
l'adressage de port.

Il occupe l'adresse &7Fxx, Il en résulte donc que le bit d’adresse A15
doit être lon et le bit d'adresse A16 high, Les autres bits d'adresse
(A12 à A18) doivent être mis (sur le niveau high) puisque les autres
éléments périphériques sont décodés d’une manière  semblablement
incomplète. Sur ces périphériques, les entrées de sélection ne sont
également reliées qu'aux différents bits d'adresse.

L'état de l’octet d'adresse inférieur est sans importance pour le
décodage et n’importe quelle valeur peut y figurer.

On peut distinguer en tout trois différents registres.

Les deux premiers registres sont liés à la production des couleurs, plus
précisément aux affectations de couleur fixées avec PEN et INK.

Le premier registre reçoit l'adresse dans laquelle la valeur de couleur
doit être écrite. Nous le désignerons désormais sous le nom de registre
du numéro de couleur (reg NC).

La valeur de la couleur elle-même peut être ensuite écrite dans le second
registre (sous la même adresse de port!). Nous appellerons ce registre
registre de valeur de couleur (reg VC).

Le troisième registre est un registre multi-fonctions (reg MF) qui
détermine le mode d'écran et la configuration de la mémoire. La sélection
des différentes possibilités y est déterminée par les différents bits à
l'intérieur du registre.

Dans tous les registres du GA, Îl est seulement possible d'écrire. Il est
IMPOSSIBLE de lire les valeurs de ces registres.

Comme le GA ne peut être appelé qu'à travers une seule adresse de port,
il faut qu'il y ait un moyen de distinguer les différents groupes. Cette
distinction est opérée grâce aux deux bits supérieurs de l’octet de
donnée, Les combinaisons possibles sont:

-29-

Bit 7 Bit6

Ecrire une valeur dans le reg NC

Ecrire une valeur de couleur dans le reg VC choisi
Ecrire une valeur dans le reg MF

Inutilisé?

oo
-o0o-o

Mais que représentent les registres de numéro de couleur et de valeur de
couleur?

Fondamentalement, ces registres correspondent aux instructions PEN et
INK, L'’instruction PEN modifie la couleur d'écriture actuelle sur le
moniteur. L'’affectation d’un numéro PEN à une couleur peut être fixée
avec l'instruction INK, 11 faut pour cela indiquer le numéro à modifier
et la valeur souhaitée. Ce sont exactement ces fonctions qu’exécutent ces
deux registres. Le numéro de la couleur à modifier est placé dans le
registre NC, après quoi la valeur de couleur souhaitée est écrite dans le
registre VC,

Pour modifier par exemple la couleur affectée à PEN 1, il faut employer
les instructions suivantes:

OUT _87F00, &X00000001 :OUT_87F00, &XO1OXXXXX

Dans la première instruction OUT, les bits 6 et 7 valent 0 et les bits 0
à 3 contiennent le numéro de la couleur à modifier. Dans notre exemple,
il s’agit du numéro 1. Le bit 5 n'a pas de fonction, le bit 4 a une
fonction spéciale sur laquelle nous reviendrons bientôt.

Dans la seconde instruction OUT, les bits 6 et 7 ont été choisis:de façon
à ce que le registre VC soit sélectionné. Les bits ’X’ correspondent
simplement à la valeur de couleur. 5 bits permettent en principe de
sélectionner 32 couleurs différentes mais 11 n’y a que 27 couleurs
différentes. Les 5 valeurs de couleur restantes sont identiques à
d'autres couleurs.

Si vous essayez cet exemple en Basic, vous constaterez que le succès
escompté se fait attendre, Tout ce que vous obtenez, c'est un rapide
flash de la nouvelle couleur.

La cause en est une particularité du logiciel du CPC, Toutes les couleurs
sont représentées en “clignotement”. Vous ne le remarquez pas parce que
le clignotement ne se fait pas entre couleurs différentes, mais entre
couleurs identiques, Lors de chaque commutation entre deux couleurs, tous
les paramètres pour le GA sont chargés à nouveau. Mais si, avant les

instructions OUT, vous utilisez l'instruction SPEED INK 255,255, vous
pourrez observer nettement plus longtemps, au moins lors de quelques
tentatives, l'effet de ces instructions.

Venons-en maintenant à l'explication du bit 4 du reg NC. Si ce bit est
lors de l'accès fixé sur le registre, l'information des bits O0 à 3 sera
ignorée et la valeur de couleur transmise par la prochaine instruction
OUT sera interprétée comme nouvelle couleur du bord.

Le registre MF est adressé lorsque, dans l'instruction OUT, le bit 7 est
mis et le bit 6 est lon. Les autres bits de ce registre ont la
signification suivante:

Bit 5: Aucune fonction?

Bit 4: 1 = annuler le compteur V Sync
Bit 3: 1 = déconnecter ROM 8&C000 à 8FFFF
Bit 2: 1 = déconnecter ROM 80000 à 83FFF
Bit 1: Mode écran

Bit 0: Mode écran

Nous n'avons rien pu découvrir Jusqu'ici sur la fonction du bit 5.

Si le bit 4 est mis, la chaîne de division pour l’impulsion
d'interruption est annulée et le processus de comptage des impulsions V
Sync recommence du début. Il serait ainsi possible d’allonger
l'intervalle entre deux impulsions d'interruption. Vous pouvez constater
cette fonction en Basic grâce à la boucle de programme suivante:

10 OUT 87F00,8X10010110:G0T0 10

Après avoir lancé cette ligne de programme, vous constatez que
l'ordinateur est complètement bloqué et qu'un RESET avec SHIFT/CTRL/ESC
n'est même plus possible. Cette ligne provoque en effet une annulation si
rapide du registre de comptage, que plus aucune impulsion d'interruption
ne peut plus se produire, Et comme le clavier est interrogé par la
routine d'interruption, vous ne pouvez plus réutiliser votre CPC qu'après
l'avoir éteint puis rallumé.

Les bits 2 et 3 déterminent la configuration de la mémoire actuelle, Si
l'un des bits est mis, c’est la Ram que le processeur rencontrera dans la

-31-

zone d'adresse correspondante, lors de ses accès en lecture, si ces bits
sont nuls, le processeur lira des données dans la Rom.

Une manipulation désordonnée de ces bits débouche au minimum sur des
messages d'erreur mais le ”’plantage” complet du système ou un Reset sont
également possibles.

Les bits restants O0 et 1 déterminent le mode actuel de l'écran. Les
combinaisons possibles sont:

Bit 1 Bit 0
0 O0 Mode 0, 20 colonnes, 16 couleurs
0 1 Mode 1, 40 colonnes, 4 couleurs
1 0 Mode 2, 80 colonnes, 2 couleurs
1 1 Comme Mode 0, mais sans clignotement

Si vous avez essayé notre programme d’une ligne pour supprimer les
interruptions en mode 1, vous aurez certainement constaté une très
curieuse modification des caractères sur l'écran. Dans cet exemple, nous
avons choisi comme mode écran le mode 80 colonnes et changé de mode sans
vider l'écran. Les caractères représentés se présentent comme s’il
manquait des points au milieu de chaque caractère. Vous trouverez
l'explication de ce phénomène à la fin du chapitre suivant, lorsque nous
décrirons la structure de l'écran et la représentation des caractères,

-32-

1.4 Le contrôleur vidéo HD 6845

Le travail principal dans la production de l’image sur le moniteur est
accompli par le contrôleur vidéo HD 6845, également désigné comme Cathode
Ray Tube Controller, CRTC en abrégé, Ce circuit intégré a été
spécialement conçu comme une interface entre des microprocesseurs et des
écrans à grille tels que les moniteurs courants.

Il produit, à partir d'un signal d’horloge unique, tous les signaux de
synchronisation nécessaires pour le moniteur et tous les paramètres
nécessaires à cet effet peuvent être programmés à l'intérieur de limites
assez larges.

Avant de décrire l'affectation des ples de connexion et la structure
interne de registres, nous voulons vous donner un bref aperçu des
possibilités de cet élément intéressant:

Nombre de caractères par ligne programmable

Nombre de lignes par écran programmable

Matrice de points verticale des caractères programmable
Accès à une zone de mémoire de 16 K

Refresh automatique pour l’utilisation de Rams dynamiques
Fonctions de contrôle du curseur

Curseur programmable (hauteur et clignotement)

Entrée light-pen

Alimentation en 5 volt continu

Entrées/Sorties compatibles TTL

Le 6845 fut développé à l'origine par Motorola pour être employé dans des
systèmes informatiques dotés de processeurs de la famille 68xx. Mais du
fait de son extraordinaire flexibilité et de sa manipulation aisée ce
contrôleur se rencontre sur de très nombreux systèmes, y compris sur des
systèmes aussi puissants que par exemple Sirius.

-33-

MA 10
MA 11
MA 12
MA 13
DISPTMG
CUDISP

misisretsieieisieicieieieteieseisieinie

siens/arnisiensiRis she; 2e} 0rsieieioie
(=
œ

1.4,1,1. Pinout du CRTC HD 6845

1.4,1 Les pôles de connexion du CRTC

La signification des 40 pattes de connexion est la suivante:

MAO-13 :

RAO-4  :

DO-7

R/W* û

CS*

RS

EN

RES*

CLK ;

HSYNC

VSYNC  :

DISPTMG :

CUREN

Memory Adress Lines; les cases mémoire de la mémoire écran sont
adressées à travers ces 14 connexions

Raster Adress Lines; ces 5 connexions choisissent à partir du
générateur de caractères la ligne actuelle de la grille du
caractère à représenter

Bidirectional Data Bus; des informations peuvent être écrites
dans le contrôleur et lues à partir de celui-ci à travers ces
pins

Read/Write*; ce signal détermine le sens des données sur les
canaux de données, Avec un low, les données peuvent être
écrites du processeur dans le CRTC, avec un high, elles sont
lues à partir du CRTC.

Chip select*, Pour permettre des transferts de données avec le
6845, celui-ci doit être adressé, ce qui est obtenu par un low
sur l'entrée CS*.

Register Select. Ce signal est utilisé pour choisir entre le
registre d'adresse et 18 registres de contrôle, Avec un niveau
lon sur RS, on peut accéder au registre d'adresse, avec un
high, on accède au registre de contrôle.

Enable. Avec une bascule ascendante de ce signal, les signaux
du processeur se trouvant sur le circuit intégré sont pris en
compte.

Reset*.Adress Lines; les cases mémoire de la mémoire écran sont
adressées à travers ces 14 connexions

Character _ Clock est le signal d'horloge dont sont tirés par
division tous les sigriaux dont a besoin le moniteur.

: Horizontal _ Sync fournit le signal de synchronisation

horizontale du moniteur. La mauvaise définition ou l'absence de
HSYNC se traduit par un défilement de l’image.

Vertical Sync fournit le signal de synchronisation verticale du
moniteur,

Display Timing, Ce signal est high lorsque le signal envoyé au
moniteur doit être représenté à l'écran. Ce signal permet
d'inhiber les retours en arrière du faisceau

: Cursor_ Enable (souvent appelé également Cursor Display ou

CURDISP) est utilisé lorsque le curseur n’est pas commandé par

-35-

le logiciel mais par le CRTC lui-même, Cette connexion permet
également de commander le clignotement du curseur.

LPSTB : Light Pen Strobe. Si une bascule low-high est envoyée sur cette
entrée, l'état actuel des canaux MA est transféré et stocké
dans les registres Light-pen. Ces registres peuvent être lus
pour être utilisés dans un programme.

1.4.2 Les registres internes du contrôleur vidéo

Comme nous l'avons déjà indiqué, le 6845 contient un registre d'adresse
et 18 registres de contrôle. Comme le signal RS, Register Select, ne
permet toutefois de choisir qu'entre deux adresses, on peut donc se
demander comment il est possible d'appeler les 18 registres de contrôle à
travers une seule adresse. La solution de ce problème réside dans le
registre d'adresse. Le numéro du prochain registre de contrôle auquel on
veut accéder est écrit dans le registre d'adresse. Ce procédé semble
certes relativement compliqué mais il présente un avantage indéniable. De
cette facon en effet, le CRTC n'occupe Justement que deux adresses et non
pas 18 ou même 32. Comme d'autre part le CRTC n'est normalement programmé
qu'une seule fois, lors de la mise sous tension de la machine, ce travail
de programmation supplémentaire est tout à fait acceptable.

Mais examinons maintenant les 18 registres un peu plus en détail. La
description suivante semblera peut-être un peu sèche et difficilement
compréhensible à cause de la structure complexe des différents registres.
De même, certaines connaissances de base en technique vidéo sont
nécessaires pour la compréhension de certains registres. Si vous ne
comprenez pas tout à la lecture de cette description, consolez-vous en
vous disant que le contrôleur vidéo de votre ordinateur n'a absolument
pas à être programmé “manuellement”.

Dans la présentation suivante, un R placé à la suite du nom du registre
indique que ce registre doit être lu et un W signifie qu'on peut écrire
dans ce registre. Remarquez que certains registres peuvent uniquement
etre lus ou écrits, ce qui est indiqué par -.

AR -/W : Adress Register, Ce registre 5 bits reçoit le numéro du

registre de contrôle souhaité. Les valeurs de registre 18 à 31
sont ignorées, les seules valeurs valables vont de O0 à 17, Ce

-36-

RO

R1

R2

R3

R&

R5

R6

R7

-/W :

-/W

registre est: appelé :-lorsau'aussi bien CS que RS sont lon.

: Horizontal Total. Ce registre 8 bits reçoit le nombre de

caractères par ligne complète, Notez d’ailleurs qu’une ligne
complète est nettement plus grande que les caractères visibles
à l'écran car les durées pour le bord et le retour en arrière
du faisceau doivent être également prise en compte dans le
calcul. Cette valeur est donc environ 1 fois et demi plus
importante que le nombre choisi de caractères par ligne.

Horizontal Displayed. Ce registre contient le nombre de
caractères à représenter à l'écran. La valeur placée ici doit
être inférieure à celle de RO.

: Adress Register, Ce registre 5 bits reçoit le numéro du

registre de contrôle souhaité, Les valeurs de registre 18 à 31
sont ignorées, les seules valeurs valables vont de O0 à 17. Ce
registre est appelé lorsqu'aussi bien CS que RS sont low.

: Sync Width. Les 4 bits inférieurs de ce registre déterminent

la largeur des impulsions HSync et VSync. Les 4 bits supérieurs
de ce registre ne sont pas utilisés,

: Vertical Total. Les 7 bits inférieurs de ce registre

déterminent le nombre total de lignes de grille par image,
Cette Valeur détermine donc ainsi également si la fréquence de
renouvellement de l’image est de 50 ou 60 Hertz,

: Vertical Total Adjust. Les 6 bits inférieurs de ce registre

permettent de réaliser un ajustement précis de la fréquence de
renouvellement de l’image.

: Vertical displayed. Les 7 bits inférieurs de ce registre

déterminent le nombre de lignes de grille réellement
représentées sur le moniteur. Théoriquement, on peut programmer
ici n'importe quelle valeur inférieure au contenu de R4.

: Vertical Sync Position, La valeur 7 bits de ce registre

détermine le moment de l'impulsion VSync. Si la valeur de R7
est diminuée, l'image du moniteur est alors décalée vers le

-37-

R8 -/M :

R9 -/W

R10 -/W

R11 -/W

R12 R/W

R13 R/H :

R14 R/W

R15 R/H

bas, si cette valeur est augmentée, il y a décalage vers le
haut.

Interlace. Les deux bits inférieurs de ce registre permettent
de déterminer si la représentation doit avoir lieu avec ou sans
procédure de saut de ligne (interlace).

Maximum Raster Adress. Ce registre 5 bits détermine le nombre
de lignes de grille des caractères à représenter.

Cursor Start Raster. Les bits 0 à 4 de ce registre déterminent
sur quelle ligne de la grille doit commencer le curseur. Les
bits 5 et 6 fixent le mode de curseur de la façon suivante:

Bits 6 5
0 O0  Curseur non clignotant
O0 1  Curseur non représenté
1 O0  Curseur clignotant (env. 3 par
seconde)
1 1  Curseur clignotant (env. 1.5 par
seconde)

Cursor End Raster. En fonction du contenu de R10, les 5 bits
inférieurs de ce registre déterminent sur quelle ligne de
l'écran se termine le curseur.

Start Adress High. Les bits O0 et 5 déterminent à partir de
quelle adresse de tout le domaine d'adressage de 16 K du CRTC
commence la mémoire écran. Si ce registre est lu, les bits 6 et
7 sont toujours 10w.

Start Adress Low. Ce registre fixe, de façon analogue à R12
l'octet d'adresse faible de la mémoire écran à adresser.

Cursor High. Les bits 0 à 5 de ce registre représentent l'octet
fort de la position actuelle du curseur.

: Cursor Low. De façon analogue à R14, ce registre reçoit l'octet

faible de l'adresse du curseur,
Comme R14 ainsi que R15 peuvent être écrits ou lus, ces

-38-

R16 R/-

R17 R/-

registres permettent de déterminer librement la position du
curseur,

: Ce registre contient après une impulsion strobe positive

l'octet fort de l'adresse de la mémoire écran qui était activée
au moment de l'impulsion, Les bits 6 et 7 de ce registre sont
toujours 104.

: De façon analogue à R16, ce registre contient l'octet faible au

moment du strobe light-pen,
R16 ainsi que R17 ne peuvent qu'être lus.

1,5 La Ram du CPC

La RAM (mémoire écriture/lecture) de 64 K intégrée dans le CPC n’est pas
uniquement utilisée comme mémoire de donnée et de programme, Les
informations concernant l'écran sont également placées dans cette
mémoire,

Après que nous ayons étudié en détail dans les chapitres précédents les
trois éléments les plus importants, le processeur, le gate array et le
contrôleur vidéo, nous allons dans le présent chapitre jeter un regard
sur l'interaction de ces trois éléments lors de l'accès aux circuits
intégrés de mémoire. Nous expliquerons également à cette occasion comment
le contrôleur vidéo appelle la Ram pour représenter des caractères à
l'écran,

Mais nous voulons” faire auparavant une petite digression pour étudier
comment fonctionnent les éléments de mémoire.

Nous allons tout d’abord expliquer comment est possible l’adressage de
65536 cases mémoire avec les 8 connexions d'adresse existantes.

Le principe consiste à diviser l'adresse 16 bits en deux moitiés et à
envoyer ces deux octets d'adresse l'un après l’autre sur les pins
d'adresse de la Ram. Ce procédé est appelé multiplexage. Le multiplexage
nécessite cependant des signaux qui indiquent à la Ram quelle information
se trouve dans l'instant sur les connexions d'adresse,

C'est ici qu’entrent en Jeu les signaux RAS* et CAS* fournis par le gate
array,

Après qu’un octet d'adresse ait été envoyé aux Rams, une bascule high-1ow
du signal RAS* leur indique qu'une moitié d'adresse est prête. Avec la
bascule négative (high-low) du RAS*, l'information d'adresse disponible
est stockée dans les Rams.

La deuxième moitié de l'adresse peut alors être envoyée à la Ram, Dès que
cet octet d'adresse est prêt, le signal CAS* devient low. La Ram a ainsi
reçue la totalité de l'adresse 16 bits et sélectionne alors la case
mémoire souhaitée. Il est maintenant possible d'écrire ou de lire cette
case.

La commutation des moitiés d’adresse doit bien sûr être également prise
en charge par un signal convenable, sur le CPC, c'est le signal CAS-
ADDR*,

-L0-

Le multiplexage est effectué par les circuits intégrés IC 104, 105, 109
et 113. On peut se représenter le fonctionnement de ces circuits intégrés
du type 74LS153 comme deux commutateurs commandés électroniquement. À
travers deux entrées de commande, on peut décider laquelle des quatre
entrées doit être reliée à la sortie.

Les deux entrées de commande sont commandées par les signaux CPU-ADDR* et
CAS-ADDR*. Le signal CPU-ADDR* permet de décider si c'est le processeur
ou le CRTC qui peut envoyer une adresse à la Ram, et CAS-ADDR* effectue
la commutation entre les moitiés d'adresse.

Nous allons examiner un exemple de commutation avec le multiplexeur IC
105,

Les pins de sortie 7 et 9 sont reliés chacun à travers une résistance de
120 Ohms avec les entrées d'adresse A0 et A1 des Rams,

Les entrées de commande A (pin 14) et B (pin 2) sont reliées aux signaux
CPU-ADDR* et CAS-ADDR* que nous connaissons.

L'information d'adresse se trouve sur les pins 3 à 6 et 10 à 13. C'est
ici aussi que vous retrouvez le signal CCLK que nous avions qualifié au
chapitre précédent de bit d'adresse auxiliaire, Le tableau suivant
indique quel bit d'adresse apparaît sur les sorties, suivant la
combinaison de commande:

-1 _

CPU- CAS- SORTIE AO DU SORTIE A1 DU
ADDR ADDR MULTIPLEXEUR  MULTIPLEXEUR

0 0 Z80, A9 Z80, AO
0 1 Z80, A2 Z80, A1

1 0 6845, MA8 CCLK

1 1 6845,MA1 6845, MAÜ

A première vue, ce tableau ne contribue pas particulièrement à la
compréhension de la commande des Rams. Il est particulièrement troublant
que le canal d'adresse AO du processeur ne se trouve pas sur A0 de la
Ram. Considérez cependant qu'il est indifférent au processeur de savoir
dans quelle adresse physique de la Ram il écrit son information. Il est
par exemple sans aucune importance pour le processeur que lorsqu'il écrit
ou lit ‘sa’ case mémoire 0, ce soit vraiment l'adresse physique 0 de la
Ram qui soit adressée ou que ce soit une tout autre adresse. De toute
façon, chaque fois qu'il accédera à ‘sa’ case mémoire 0, c’est toujours
la même case mémoire qui sera adressée, La désignation des pins d'adresse
de la Ram est donc arbitraire et sans importance pour le processeur.

Baucoup plus importante est l'affectation d'adresses du processeur aux
adresses du CRTC. Cette affectation est montrée par la figure 1.5.0.1.

Comme on voit, tous les bits d'adresse du processeur sont envoyés à
travers les multiplexeurs sur les connexions d'adresse des Rams mais le
contrôleur vidéo adresse également avec l’aide du CCLK l'ensemble de la
zone adressable de 64 K, Ce qui contredit cependant le chapitre précédent
où nous disions que le CRTC ne peut adresser qu'une zone de 16 K,

Cette affirmation était exacte dans la mesure où seules les 14 connexions
désignées par MA (Memory Adress Line) peuvent être comptées comme canaux
d'adresse, Ces 14 connexions permettent d'adresser une zone d'adresse de
16 K,

-42-

A0 CCLK AS MA7
A1 MAO A9 MAB
A2 MAI A10 MA9
LS] MA2 Ait RAO
M4 MAS A12 RAI
A5 MA4 A13 RA2
A6 MAS At4 MA12
A7 MAG A15 MA13

1.5.0.1 Accès du 780 et du 6845 à la mémoire commune

Le mode de travail du 6845 utilisé dans le CPC pour l'adressage de la
mémoire vidéo est rarement employé. Les connexions RAO à RA4 servent
normalement à appeler une Rom de caractères déjà programmée qui contient
le modèle bits des caractères qui doivent être représentés à l'écran,

Les ordinateurs ont normalement une zone de mémoire appelée mémoire vidéo
dans laquelle sont stockés tous les caractères à représenter à l'écran.
Dans cette mémoire, l'emplacement de chaque caractère occupe un octet.
Cela donne donc, pour représenter 80 x 25 caractères, une mémoire de 2000
octets.

Mais 11 n’est pas possible de faire entrer dans un octet toutes les
informations nécessaires pour la représentation des caractères. Chaque
caractère se compose en effet d’un certain nombre de lignes de points
placées les unes sous les autres,

Sur le CPC, on peut également reconnaître ces lignes sur le moniteur.
C'est ainsi par exemple que le curseur se compose de 8 lignes placées les
unes sur les autres, dont tous les points image sont “allumés”. Pour
représenter des lettres ou des chiffres, seuls les points d’une ligne
correspondant à la forme de la lettre ou du chiffre sont allumés. Les
modèles de ces lignes de points sont stockées sous forme de cartes bits,
dans lesquelles un bit mis correspond normalement à un point allumé sur
l'écran.

Les connexions RA sont maintenant nécessaires pour recevoir de la Rom de
caractères les différentes lignes, c’est-à-dire les cartes bits, À cet
effet, les connexions RA sont utilisées comme canaux d'adresse pour la

-143-

Rom de caractères.

Comme vous pouvez l'imaginer, 11 n'est pas possible de réaliser à l'écran
du graphisme haute résolution lorsqu'on utilise une Rom de caractères.
Les ordinateurs fonctionnant suivant ce principe ne peuvent sortir du Jeu
de caractères intégré,

Sur le CPC, cette Rom de caractères n'existe pas et on a choisi une voie
totalement différente,

Comme les connexions RA adressent directement la mémoire, les
informations sur les points doivent donc nécessairement figurer également
en Ram. Ce n'est qu’à travers cette astuce de commutation qu’il est
possible de produire n'importe quelle carte bits sur le moniteur et donc
de représenter le graphisme dans les limites connues.

Mais avant que nous ne nous tournions vers la structure concrète de la
mémoire vidéo, il nous faut enfin expliquer le signal CCLK. Mais il nous
faudra pour cela un peu de mathématiques.

Le CRTC est commandé par une fréquence d’horloge de 1 MHz. Avec chaque
impulsion d'horloge est adressée une case mémoire. Dans cette case se
trouvent les informations sur les points qui doivent être représentés
‘allumés’ sur l'écran, c'est-à-dire dans la couleur d'écriture. Comme une
fréquence de 1 MHz correspond à une période de 1 micro-seconde,
exactement un huitième de la fréquence d'horloge est disponible pour la
réprésentation de chaque point, soit 0.125 micro-secondes. Pour
représenter les 640 points d'une ligne, il faut donc un temps de 80
micro-secondes,

Mais comme le signal V Sync qui détermine la durée d’une ligne a une
période de 52 micro-secondes, le compte n'est pas bon. Ces valeurs ne
permettent de réprésenter que 40 caractères au maximum.

La solution à ce problème réside dans un mode spécial de travail des
Rams, le Page Adress-Mode (mode d’adressage par page), Lorsqu'une Ram,
après avoir envoyé les signaux RAS et CAS, envoie le contenu de la case
mémoire souhaitée sur les sorties de donnée, il suffit alors de n’envoyer
avec une autre impulsion CAS qu'une nouvelle moitié d'adresse aux Rams
pour recevoir l’octet suivant. Cela suppose bien sûr que seule une moitié
des informations d’adresse ne change.

C'est exactement cette possibilité qu'ont utilisée les développeurs du
CPC, Bien sûr, il faut que les informations d'adresse correspondant aux
deux différentes impulsions CAS soient différentes, sinon on lit deux
fois la même case mémoire, Mais c'est justement ce que réalise le signal
CCLK qui commute exactement entre les deux impulsions CAS. Ce signal est

44

envoyé par le multiplexeur IC 105 sur le bit d'adresse O0 (du point de vue
du processeur), lorsque le signal CAS-ADDR est sur low et le signal CPU-
ADDR par contre sur high. Ce signal représente bien ainsi le bit
d'adresse inférieur de la Ram vidéo.

Les deux octets fournis rapidement l'un après l'autre par la Ram vidéo
sont entrestockés dans le gate array, convertis dans la forme sérielle
indispensable pour le moniteur et envoyés avec les informations de
couleur sur la sortie RVB,

Restent encore les deux signaux MA12 et MA13, Ces deux signaux permettent
de déterminer par blocs de 16 K le début de la Ram vidéo. Ces bits sont
normalement mis et la Ram vidéo commence donc en &C000. Mais il est
également possible d'obtenir par programmation que la Ram vidéo soit
placée de 284000 à g7FFF,

-45-

1.6 La Ram vidéo entre 780 et 6845
Essayez maintenant ce petit programme sur le CPC:

10 MODE 2

20 FOR i=8c000 TO gffff
30 POKE i,255

40 NEXT i

Vous obtenez sur l'écran une ligne étroite qui est rapidement dessinée
vers la droite à partir de l'angle supérieur gauche de l'écran. À la fin
de la première ligne, le dessin se poursuit exactement 8 lignes plus bas,
Une fois dessinées ces lignes étroites sur toute la surface de l'écran,
le dessin reprend d'en haut mais en dessous des lignes déjà dessinées.

Essayez le programme également en mode 1 et en mode O0.
Puis modifiez aussi la ligne 30 ainsi:
30 POKE i,1

Vous obtenez maintenant une ligne de points qui remplit l'écran
verticalement.

Lorsque le programme tourne en mode 2, on voit que les lignes verticales
se trouvent sur le côté droit des caractères.

En mode 1, nous obtenons 2 lignes verticales par caractère, en mode O0, 4
lignes.

Nous allons maintenant apporter une dernière modification au programme.
Supprimez la ligne 10 du programme et entrez "MODE 2’ en mode direct.
L'écran se vide et Ready apparaît dans l'angle supérieur gauche. Appuyez
sur la touche de curseur BAS, jusqu'à ce que le message Ready disparaisse
de l'écran. Le curseur se trouve maintenant dans la dernière ligne de
l'écran, Faites à nouveau tourner le programme.

Le résultat est quelque peu agaçant.

Ce petit programme nous a révélé plusieurs choses importantes d’un seul
coup. D'abord nous avons démontré que la mémoire écran commence en &C000

-46-

et finit en &FFFF. Curieusement, la taille de la mémoire écran est la
même pour les trois modes écran. La seule différence entre les modes
réside dans les couleurs.

Cependant on peut se demander à quoi servent 16 K de mémoire écran en
mode O0, lorsqu'on ne représente que 20 caractères par ligne, 20
caractères par 25 lignes font 500 caractères sur l'écran, Pourquoi le CPC
a-t-il besoin de 16384 cases mémoire pour représenter à l'écran ces 500
caractères?

La réponse est simple, Comme nous l'avons déjà indiqué, le CPC ne possède
pas de Ram vidéo dans laquelle chaque caractère serait stocké dans un
octet.

En mode 80 colonnes, un caractère sur l'écran occupe 8 octets, en 40
colonnes, un caractère occupe 16 octets et 32 octets en mode 20 colonnes.
C'est d’ailleurs ce que montrait le programme qui produisait les lignes
verticales.

Le mode 80 colonnes est à cet égard le plus simple à comprendre, puisque
chaque bit mis produit un point dans la couleur actuelle d'écriture
(pen), Si un bit n'est pas mis, c’est au contraire la couleur du fond de
l'écran qui apparaît à cet endroit, Comme en mode 2, il n'y a qu’une
couleur d'écriture possible, il n'y a pas d’autres possibilités.

Mais à quoi servent donc en mode O0 les 32 octets nécessaires pour un
caractère?

Le fonctionnement des modes O0 et 1 n'est plus aussi simple à expliquer.
Nous vous conseillons de taper le petit programme suivant et d’avoir sous
les yeux les effets de ce programme, pendant que vous lirez nos
explications. Les explications seront alors plus compréhensibles,

10 MODE 2

20 REM

30 PRINT A”

40 FOR adresse=8c000 TO &f800 STEP 8800

50 p$=BIN$(PEEK(adresse),8)

60 FOR I=1 TO 8

70 IF MID$(p$,1,1)="1" THEN PRINT "X"; ELSE PRINT ”,":
80 NEXT I

-L7-

90 PRINT
100 NEXT adresse

Faites tourner ce programme et vous obtiendrez une image correspondant à
la matrice de ’A'.

Modifiez maintenant l'instruction MODE de la ligne 10 en MODE 1 et faites
tourner le programme. Le résultat est assez surprenant.
Vous pouviez vous imaginer que seule une moitié de la matrice figurerait
dans les octets lus, Mais il semble curieux a priori que la matrice
n'utilise qu’une moitié d'octet, soient les bits 4 à 7.

Mais nous nous rapprochons de la solution de cet enigme, lorsque vous
modifiez ainsi la ligne 20:

20 PEN 2

Non seulement la couleur d'écriture (PEN) s'est modifiée, mais la carte
bits montrée par notre programme s’est aussi modifiée. Et voilà la
solution de notre problème!

Si vous connaissez déjà le CPC, vous savez qu'en mode 40 colonnes, 4
couleurs sont possibles, Ces 4 couleurs sont tout simplement stockées
avec le caractère lui-même, En effet 4 bits seulement déterminent les
pixels (points de l'écran) allumés et les quartets Low et high décident
des couleurs (un quartet=un demi-octet, 4 bits). Avec le principe
utilisé, le gate array n'a qu’à doubler horizontalement les pixels
correspondant à l'affichage, représentant ainsi 8 points, alors que seuls
4 bits sont stockés en mémoire.

En mode 0, pour représenter 20 caractères par ligne, cette méthode est
encore étendue, Il n’y a plus ici que deux bits qui contiennent les
informations sur les pixels, La position de ces deux pixels à l’intérieur
de l'’octet détermine la couleur dans laquelle ces pixels doivent être
représentés. Il y a ainsi 16 combinaisons possibles, ce qui correspond
exactement au nombre de couleurs disponibles. Come seulement deux pixels
sont stockés dans un octet, 4 x 8 = 32 octets sont nécessaires pour
représenter un caractère avec 16 couleurs différentes possibles.

Essayez à nouveau le programme en mode O0 en utilisant différentes valeurs
pour l'instruction PEN. Vous comprendrez vite le fonctionnement.

-L8-

Les deux premiers points soulevés au début du chapitre sont ainsi
éclaircis. Reste cependant le point du ‘décalage’ de la Ram écran. Ce
problème a sa source dans l'électronique du CPC.

Même un Z80 avec une fréquence d'horloge de 4 MHz à besoin d'un certain
temps pour décaler un bloc de données de 16 K. Par exemple, pour éviter
d'avoir à décaler de 640 cases mémoire, lors du listage d'un programme
assez long, la totalité de la zone de Ram vidéo, on a utilisé une
propriété du CRTC. Par programmation adéquate des registres 12 et 13 du
6845, l'écran peut commencer pratiquement en n'importe quelle case
mémoire paire de la Ram vidéo. Le scrolling (défilement de l'écran) peut
ainsi se produire nettement plus vite, puisqu'il suffit de fournir les
valeurs adéquates aux registres qui conviennent. La nouvelle ligne dans
le bord inférieur de l'écran est vite effacée et remplacée par les
nouveaux caractères.

Il n'est pas possible de faire commencer la Ram vidéo à une adresse
impaire, par exemple en 8C001, du fait de l'utilisation décrite plus haut
du signal CCLK comme bit d'adresse,

Le programme suivant montre qu'il est possible de manipuler les registres
décrits, même en Basic:

10 adrreg = &bc00 : REM registre d'adresse du 6845
20 datreg = 8bd00 : REM port du registre de donnee
30 OUT adrreg,13 : REM selectionner le registre

40 FOR offset = 1 TO 40

50 OUT datreg,offset : REM modifier 40 fois

60 for attendre = 1 TO 40 : REM et attendre un peu
70 NEXT attendre,offset

(1

Ce programme réalise un scrolling horizontal de l'écran. Sans la boucle
de temporisation, le scrolling se déroulerait tellement vite qu’il ne
serait pas possible de suivre avec un oeil humain.

Le scrolling vertical peut également être programmé en Basic. Il faut
alors modifier les deux registres low-byte et high-byte. Mais comme 11
s'écoule beaucoup de temps entre les deux instructions OUT, on obtient
des phénomènes désagréables à l'écran.

Mais, en ce qui concerne la Ram vidéo, il faut encore relever une

-49-

particularité.

Multiplions les valeurs que nous connaissons entre elles.

En mode 2, un caractère se compose de 8 octets. 11 y a 80 caractères par
ligne et 25 lignes sur l'écran. La place occupée en mémoire est donc de
80 x 25 x 8 = 16000 octets, Mais une zone de 16 K comporte 2 puissance 14
= 16384 emplacements. Où sont les 384 octets manquants?

Très simple. Ils ne servent à rien, du moins tant qu’il n'y a pas de
scrolling de l'écran.

Il est donc possible de placer ici des valeurs à stocker provisoirement.
Ces valeurs seront cependant effacées par la première instruction CLS,

Vous vous demandez certainement comment il est donc possible de
programmer du graphisme avec une organisation aussi compliquée de la
mémoire écran,

Il semble également impossible de lire un caractère à partir de l'écran.
Sur d’autres ordinateurs, cela ne pose pas de problème, puisqu'on peut
placer un caractère sur l'écran avec POKE et qu’on peut donc lire le
contenu de la Ram vidéo avec PEEK.

D'autre part il est normalement assuré que la Ram vidéo commence à une
adresse déterminée,

Les choses ne se présentent cependant pas aussi mal que cela peut sembler
au premier abord. Le système d'exploitation est en effet en mesure de
discerner les adresses de début modifiables ou de déterminer un caractère
à partir de la matrice de l'écran, comme cela se produit chaque fois que
vous utilisez la touche COPY. Les routines utilisés à cet effet peuvent
également être employées dans des programmes en langage-machine que vous
aurez réalisés vous-même.

Vous retrouverez bon nombre de ces routines du système d'exploitation
dans un prochain chapitre, Nous vous montrons concrètement comment
utiliser le graphisme dans un exemple de dessin de rectangles et dans un
programme de hardcopy graphique.

-50-

1,7 L'interface parallèle 8255

Développé à l'origine par INTEL pour le 8080, le 8255 convient également
pour d’autres processeurs comme élément polyvalent d'entrée/sortie, Le
8255 dispose en tout de 24 canaux à travers lesquels les signaux peuvent
etre sortis ou entrés. Chaque groupe de 8 canaux constitue un port 8 bits
et le troisième port peut être scindé en deux moitiés programmables.

Les principales caractèristiques du 8255 sont:

24 connexions d’entrée/sortie programmables
Alimentation en courant continu de 5 volts

Entièrement compatible TTL

Trois puissants modes de travail programmables

Chaque port programmable séparément

Courant de sortie de 1 mA pour une tension de 1,5 Volts
Possibilité de fonction mettre bit/annuler bit

1.7.1 L'affectation des connexions du 8255

L'affectation des pins du 8255 est indiquée par la figure suivante. En
voici la signification:

DO - D7 : Data lines. Ces connexions sont reliées au bus de données du
processeur, Elles servent au transfert des données vers et à
partir du processeur.

CS : Chip select. Un low sur ce pôle sélectionne le composant. Les
signaux figurant actuellement sur les canaux RD, WR et Data
sont acceptés par le 8255.

RD : Read. Un lon sur ce pôle entraîne que le 8255 envoie des
données ou des informations d'état au processeur, à travers
le bus de données.

WR : Write devient low lorsque le processeur veut envoyer des
données ou des instructions de commande au 8255,

-51 _

PA 3
PA 2
PA 1
PA 0
RD°
cs*
GND
A 1
A 0
PC 7
PC 6
PC 5
PC 4
PC 0
PC 1
PC 2
PC 3
PB 0
PB 1
PB 2

CO OC OCO OL E: 0e
MEME CO CE CES EEE

1.7,1,1. Pinout du port parallèle 8255

PA 4
PA 5
PA 6
PA 7

D 0
D 1
D Z
D 3
D 4
D 5
D 6
D 7

PB 7
PB 6
PB 5
PB 4
PB 3

AO, A1 : Adress Lines 0 et 1. À travers ces connexions s'opère la
sélection entre les trois canaux de données et le registre de
commande. Ces connexions sont souvent liées aux deux canaux
d'adresse inférieurs du processeur.

RESET : Un high sur cette entrée rétablit les valeurs initiales de
tous les registres, y compris le registre de commande. Les
canaux de port sont placés en mode de travail entrée.

PAO - PA7 : Port À. Ces huit canaux représentent le port d’entrée/sortie
A et peuvent être utilisés au choix en entrée ou en sortie.

PBO - PB7 : Port B. Fonctionnement identique au port À

PCO - PC7 : Port C, Fonctionnement identique au port À

1.7.2 Les modes de travail du 8255

Avant que nous n'en venions aux quatre registre internes, 11 nous faut
tout d’abord examiner d’un peu plus près les possibilités de ce circuit
intégré. Comme nous l'avons indiqué au début, le 8255 dispose de 3 modes
de travail possibles:

Mode de travail O0 : Simple entrée/sortie
Mode de travail 1 : Entrée/sortie manipulable
Mode de travail 2 : Bus à deux sens

Le mode de travail 0 est le plus simple et le plus courant. Dans ce mode,
il est possible de déterminer si les ports doivent travailler comme
canaux d'entrée ou de sortie. Si les canaux sont programmés comme sortie
et si le processeur envoie une information sur ces sorties, cette valeur
est stockée, et les sorties sont conservées jusqu’à une nouvelle
programmation ou jusqu’à un reset.

Les ports programmés comme entrée fournissent lors d’une lecture l'état
momentané de ces canaux.

Le sens des données sur le port A aussi bien que sur le port B ne peut
‘etre programmé que de façon identique pour tout le port. Il n’est pas

possible d'utiliser par exemple les bits de port PAO, PA3 et PA7 en
sortie et les autres bits du même port en entrée.

Le port C peut cependant être programmé en deux moitiés distinctes. Le
sens des données de chaque moitié peut être programmé séparément.

Le mode de travail 1 se différencie fondamentalement du mode 0, Dans ce
mode de travail, un transfert de données dans un sens est possible avec
des signaux hand shake, On ne parle plus alors de trois ports car les
deux moitiés du port C sont mises à la disposition des deux autres ports
comme signaux de commande et de réception. On parle alors des deux
groupes A et B.

Le groupe À comprend le port À et les bits 4 à 7 du port C, le groupe B
le port B et les bits O0 à 3 du port C.

Pour programmer facilement le mode 1, il est possible d'utiliser un bit
spécial de chaque moitié du port B comme signal d'interruption.

Un tel transfert de données 8 bits est utilisé par exemple sur les
interfaces d'imprimante. Un signal indique ici que les données sur les
canaux de données sont valables. Un signal rapporté indique si le
récepteur, en l'occurence l'imprimante, est prêt à recevoir des données,
ou si les données ont été reçues correctement.

Cette fonction peut être exécutée par le 8255, au choix pour une sortie
ou une entrée de données.

Le troisième mode de travail (mode 2) est un mode de travail
bidirectionnel. Cette fonction n'est possible qu'avec le port A. Les bits
PC3-7 sont utilisés comme signaux de commande et de réception.

Une application possible de ce mode de travail serait la commande d’un
lecteur de disquette car les données doivent dans ce cas être transmises
aussi bien du lecteur de disquette au processeur que du processeur au
lecteur, à travers les mêmes connexions.

Il est d’autre part possible dans les trois modes de travail de mettre ou
d'annuler individuellement par instruction les bits programmés en sortie,
Les trois modes de travail ainsi décrits peuvent être également combinés.
I1 est ainsi possible d'utiliser le Port A en mode O0 comme sortie, le
port B en mode 1 comme entrée et de programmer les bits restants du port
C en entrée.

1.7.3 Commande du 8255, description des registres

-53-

Lorsqu'on considère tout d’abord ce nombre troublant de possibilités, on
se demande malgré soi comment toutes les possibilités et combinaisons
peuvent être programmées avec un seul registre de commande.

L'astuce qui rend cela possible est simple. Le bit supérieur du mot de
commande est utilisé comme bit témoin. Si ce bit est mis dans le mot de
commande, les bits 0 à 6 ont la signification suivante:

Bit O : commande la fonction Port C bits 0-3
1=Entrée
O=Sortie

Bit 1 : commande la fonction Port B
1=Entrée
O=Sortie

Bit 2 : sélectionne le mode groupe B
1=Mode de travail O0
O=Mode de travail 1

Bit 3 : commande la fonction Port C bits 4-7
1=Entrée
0=Sortie

Bit 4 : commande la fonction Port A
1=Entrée
O=Sortie

Bit 6,5 : sélectionne le mode groupe A
00=Mode 0
01=Mode 1
1x=Mode 2, bit 5 sans signification

Si par contre le bit supérieur du mot de commande est nul, la fonction
‘mettre un bit/annuler un bit’ du port C est définie. La signification de
ces bits est:

Bit O0 : commande Bit Set/Bit Reset

1=mettre un bit
O=annuler un bit

-54-

Bits 3-1: Sélection du bit

000 = PCO
001 = PCI
010 = PC2
011 = PC3
100 = PC4
101 = PCS
110 = PC6
111 = PC7

Les bits 4 à 6 du mot de commande sont sans signification lorsque le bit
7 est nul,

Ce registre de commande ne peut être lu. Il n'est possible que d'!
écrire, Les registres correspondant aux ports peuvent par contre êtrt
lus, même lorsque les ports sont utilisés en sortie. Dans ce cas, li
valeur lue correspond à l'état des canaux de port.

L'accès aux quatre registres se fait à travers les pins de connexion A
et A1. Ces connexions sont décodées dans le 8255 et utilisées comme
signaux de sélection de registre. Normalement AO et A1 du 8255 sont
envoyés sur les canaux de même nom du processeur, Il en résulte ur
adressage transparent sur 4 adresses.

L'affectation aux registres des connexions AO et A1 est indiquée par lt
tableau suivant:

A1 AO

0 O Registre Port A

0 1 Registre Port B

1 O0 Registre Port €

1 1 Registre de commande

1,7,4 L'utilisation du 8255 sur le CPC

Après avoir donné un aperçu des possibilités variées du 8255, nous et
venons au fonctionnement pratique de ce composant universel sur le CPC
Comme en fait presque tous les circuits intégrés sur le CPC, le 8255 es
également utilisé de façon optimale. Aucun bit n'est inutilisé.

Mais devenons plus concret,

-55-

Le 8255 sert le clavier, le chip sonore, le moteur du lecteur de
cassette, produit les signaux d'écriture du lecteur de cassette, lit le
flux de bits venant du lecteur de cassette, contrôle le signal V Sync du
CRTC, contrôle si l'imprimante est prête à recevoir, interroge avec un
bit l'état du signal EXP du connecteur d'extension, décide à travers un
pont si la production de l’image doit se faire suivant la norme PAL ou
SECAM en 50 ou 60 Hertz et il reste enfin encore trois bits qui
interrogent des ponts lors de la mise sous tension de façon à savoir quel
ordinateur vous avez acheté. L'état de ces ponts décide en effet si vous
recevrez dans le message d'’initialisation, le nom de la firme Amstrad,
Awa, Triumpf, Schneider ou un autre des 8 noms possibles.

Avoir réalisé toutes ces fonctions avec uniquement les 24 canaux
d'entrée/sortie disponibles, témoigne de l'esprit d'économie et de
l'inventivité des développeurs de ce matériel.

Le schéma de fonction montre comment le 8255 est connecté. Le bus de
données est relié directement au bus de données du processeur. Le signal
CS (Chip Select) est produit par le bit d'adresse A11 du processeur, Les
pins AO et A1 du 8255 pour la sélection de registre sont reliés aux pins
d'adresse A8 et A9 du processeur.

Comme nous l'avons déjà indiqué, les éléments périphériques du CPC sont
appelés à travers des adresses de port. C’est pourquoi le canal RD* du
8255 est relié au signal IORD*.

Ce signal est produit par la combinaison des signaux RD* et IORQ* du Z80
avec une porte logique de l1'I1C112. Uniquement lorsque IORQ* et RD* sont
low, apparaît un low sur le pin 6 de sortie de l’IC 74LS32.

La connexion WR* du 8255 est commandée de même. Ici apparaît un low,
venant du pin 3 du 74LS32, Ilorsqu'aussi bien WR* que IORQ* du 780
deviennent lon sur les pins 1 et 2 de l'IC 112.

Ces données permettent maintenant de déterminer les adresses de port du
8255, Pour, par exemple, écrire une valeur dans le registre 0, le
registre de données du port À, les connexions A11, A9 et A8 doivent être
low, En écriture binaire, nous obtenons, pour l'octet fort du bus
d'adresse, la valeur suivante:

A15 A14 A13 A12 A11 A10 AO9 A08
1 1 1 1 0 1 0 0

Ce qui correspond à la valeur hexadécimale 8F4.

-56-

Les 8 bits d'adresse inférieurs n’interviennent pas dans la sélection du
8255, une valeur entre 800 et &FF est ici possible.

Les bits mis dans l'octet fort ne sont pas non plus nécessaires en
réalité à un adressage correct et on pourrait donc avoir l’idée
d'utiliser comme octet fort la valeur 00H. Cela marcherait d’ailleurs.
Mais comme le “décodage des différents circuits intégrés périphériques se
produit d’une semblable façon incomplète, les bits doivent être mis,
sinon d'autres circuits intégrés tels que le CRTC ou le gate array
pourraient se croire également appelés.

Mais revenons à notre exemple. Donc, pour charger une valeur dans le
registre À, la valeur 8F400 doit être placée sur le bus d'adresse. Ceci
peut être obtenu avec les instructions:

LD A, valeur
LD BC, &F400
OUT (C),A

Le registre de port C peut de même être lu avec les instructions:

LD BC, &F600
IN A, (C)

Les trois ports sont utilisés essentiellement en mode 0. Les 24 canaux
d'entrée/sortie sont ainsi disponibles.

Le port À (8F400) est relié aux 8 canaux de données du générateur de son
AY-3-8912, Suivant l'action demandée, le port À est programmé comme
entrée ou sortie.

S'il est programmé en sortie, les instructions de commande sont envoyées
au chip sonore à travers les 8 canaux du port. Vous trouverez le détail
de ces instructions de commande dans le chapitre sur la programmation du
AY-3-8912, Indiquons simplement pour le moment que le chip sonore dispose
également d’un port 8 bits bidirectionnel., Une page de la matrice du
clavier est connectée sur ce port. A travers le port À du 8255, il est
possible par un détour du port du AY-3-8912 de savoir si une touche est
enfoncée, A cet effet, le port A doit bien sûr être programmé en entrée,

Le port B (gF500) est programmé comme port d'entrée. Toutes les
interrogations évoquées, hormis celle du clavier, se produisent à travers

-67-

ce port. Les différents bits de ce port reçoivent l'affectation suivante:

Bit 0 : Ce bit interroge l'état du V Sync du CRTC. Comme cette
interrogation doit aller très vite, le bit 0 peut être décalé
dans le flag carry par simple rotation de la valeur lue avec
INP, Il est ainsi possible de connaître rapidement l'état de
V Sync,

Bits 1-3 : Ce bit est relié au pont LK4. Si ce pont est ouvert, le
contrôleur vidéo est programmé pour le travail en PAL en 50
Hertz, Un pont fermé entraîne une programmation du CRTC pour
la norme SECAM de 60 Hertz pour la fréquence de
renouvellement de l'image. Cette possibilité de programmation
différente est importante lorsque le CPC doit être utilisé à
travers le module MP1 sur un téléviseur.

Bit 5 : Ce bit interroge l'état du signal EXP du connecteur
d'extension.
Bit 6 : Ce bit restitue l'état d'une imprimante connectée. Comme

l'imprimante ne peut pas recevoir de caractères en
permanence, il est possible d'interdire un transfert de
caractère en fixant cette connexion sur high.

Bit 7 : Les données fournies par le lecteur de cassette avec un
niveau TTL sont lues à travers ce bit, Ici aussi vaut ce que
nous disions pour le bit O0. Comme ce canal doit être examiné
très rapidement, l'état de ce canal peut être déterminé très
vite par une rotation unique du bit 7 vers le flag carry.

Le port C (8F600) est sur le CPC programmé comme port de sortie. Quatre
de ses huit canaux lui permettent de commander une partie de
l'interrogation du clavier et deux autres bits sont utilisés pour le
lecteur de cassette. Les deux bits restants sont employés pour la
commande du chip sonore. Comme les canaux du port C peuvent être mis et
annulés directement, celui-ci convient particulièrement à ce type de
tâches,

Les différents bits sont ainsi utilisés:

Bits 0-3 : Ces bits commandent la matrice du clavier. Les quatre canaux

-58-

Bit 4

Bit 5

Bits 6-7

programmés en sortie sont reliés à 1’1C101, un décodeur BCD-
décimal.

Ce décodeur met sur la masse une de ses 10 entrées, en
fonction de l'information binaire en entrée. Les combinaisons
en entrée autorisées sont les valeurs de 0 à 9.

Ce bit commande le moteur du lecteur de cassette. Le moteur
n'est cependant pas commandé directement, mais à travers un
transistor (et un relais commuté à la suite). Si ce bit est
sur la masse, le moteur s'arrête, Un high en sortie sur PB4
est conduit par le transistor 0101 et le moteur tourne si la
touche PLAY est enfoncée,

: Les fréquences, qui doivent être reçues par le lecteur de

cassette et qui produisent cette si douce mélodie, sont
fournies par l'ordinateur à travers ce pin du 8255,

: Ces bits de port sont reliés aux connexions BC1 et BDIR du

chip sonore et travaillent comme signaux de chip select et de
strobe pour l'’AY-3-8912, Vous trouverez une description plus
détaillée de ces connexions dans le prochain chapitre sur le
générateur de son.

-59-

Le générateur de son programmable AY-3-8912

L'AY-3-8912 de General Instruments est un générateur de son programmable
(PSG) de grande classe. I1 a été développé pour les jeux électroniques,
afin de doter ceux-ci d’un son particulièrement réaliste alors que les
premiers Jeux électroniques ne pouvaient produire que des bruits vraiment
monotones, Pour pouvoir être employé le plus universellement possible, le
PSG a été doté d’un grand nombre de possibilités d’influencer le son. On
pensa en outre lors du développement de ce circuit intégré que, dans
pratiquement tous les domaines d'application, il faudrait pouvoir
interroger des touches, joysticks ou commutateurs quelconques. C’est
pourquoi on a donc également doté ce PSG d’un port parallèle 8 bits.

Les caractéristiques de ce circuit intégré sont les suivantes:

Trois oscillateurs de son programmables indépendamment
Un générateur de bruit programmable

Des sorties analogues entièrement commandées par logiciel
15 niveaux de volume étagées par logarithme

Courbes d’enveloppe programmables

Compatible TTL

Alimentation en courant continu de 5 Volts

L'AY-3-8912 dispose en tout de 16 registres, dont 15 registres peuvent
etre utilisés. À travers ces registres peuvent être programmées toutes
les possibilités sonores du chip.

Le branchement du PSG peut être divisé en différents blocs de fonction.

Il y a d'abord le bloc des générateurs de son. Les générateurs de son
reçoivent un signal d'horloge qui est produit à partir de la division par
16 du signal de l'horloge. Les générateurs de son sont responsables de la
production fondamentale des trois fréquences de son carrées.

Le générateur de bruit produit un signal carré en modulation de fréquence
dont l'écart de pulsation est influencé par un pseudo générateur de
bruit.

Les mixeurs couplent les signaux de sortie des trois générateurs avec le
Signal de bruit. Le couplage peut être programmé séparément pour chaque

CHANNELC (Ÿ QO DA0
TEST1 () QO DA1
Vcc () O DA2
CHANNELB () QO DA3
CHANNELA () Q DA4
Vss () (O) DA5
I0A7 () O DA6
I0A 6 () Q DA7
I0A5 () Q 8c1
1044 ( Q 8c2
I0A 3 () CO BDIR
I0A2 () O A8
I0A1 ( ©  RESET*
I0AO () CO) cLock

1.8.1.1 Soundchip AY—-3—-8912

canal.

Le bloc de fonction du contrôle d'amplitude offre deux possibilités à
l'utilisateur. D'une part l'amplitude de sortie (le volume) des trois
canaux peut être influencée à travers la programmation du registre de
volume correspondant.

D'autre part il est possible de les faire influencer de façon variable
par le PSG. La sortie du registre de courbe d'enveloppe est alors
utilisée pour influencer le volume. Comme la courbe d’enveloppe peut être
programmée avec quatre paramètres distincts, les possibilités
d'influencer le son sont variées,

Le bloc de fonction du convertisseur D/A est responsable de la production
du volume des signaux de sortie. Comme les informations de volume et
d'enveloppe sont sous forme de valeurs digitales, elles sont converties
dans le convertisseur D/A.

Le dernier bloc de fonction n’a rien à voir avec la production du son.
Dans ce bloc sont placés deux ports 1/0, Si vous êtes maintenant un peu
surpris, c'est que vous nous avez lu attentivement. En effet le chip AY-
3-8912 contient deux ports I/0 complets dont un seul cependant est
branché sur les pins de connexion. Le même chip est utilisé dans l'AY-3-
8910, sur lequel les deux ports peuvent être utilisés.

1.8.1 Les connexions du chip sonore

Comme les noms des connexions du PSG ne sont pas suffisamment
explicatifs, voici une description détaillée de la fonction des pins:

DAO - 7 : Ces connexions du chip sonore sont reliées au bus de données
du processeur, Le nom DA indique que aussi bien des Données
que des Adresses (de registre) passent à travers ces
connexions.

-61-

A8

BDIR &
BC1,2

: Cette connexion peut être comprise comme un signal CHIP-
SELECT. Pour appeler des registres du PSG, ce signal doit
être high.

: La connexion signal BDIR (Bus DIRection) et les connexions
BC1 et BC2 (Bus Control) commandent l'accès aux registres sur
le PSG. Au premier abord, l'affectation indiquée par le
tableau peut paraître curieuse, Mais comme ce circuit intégré
fut à l’origine développé comme composant du processeur 1610,
un processeur 16 bits spécial de General Instruments, on a
pris en compte lors de la conception les propriétés spéciales
et des connexions de commande de ce processeur.

BDIR BC2 BC1 Fonction du PSG
INACTIVE

LATCH ADRESS
INACTIVE

READ FROM PSG
LATCH ADRESS
INACTIVE

WRITE TO PSG
LATCH ADRESS

0000
O=O0—-0-0

Dans ce tableau, seules quatre des huit combinaisons ont vraiment un
sens. C’est pourquoi la connexion BC2 est souvent mise sur +5 Volts. Le
tableau restant n’est donc plus influencé que par les signaux BDIR et BCI
et il se présente ainsi:

BDIR BC1 Fonction du PSG
0 O INACTIF, le bus de données du PSG a une valeur en ohm
haute
0 1 READ, des données peuvent être lues dans les
registres du PSG
1 O WRITE, des données peuvent être écrites dans le
registre du PSG sélectionné
1 1 LATCH, le numéro ou l'adresse du registre du PSG

-62-

ANALOG A

ANALOG B

ANALOG C

IOA7 - 0

CLOCK

RESET

TEST1

Vcc

Vss

souhaité est écrit dans le PSG

: C'est la sortie du canal À. Ici peuvent être retirés les sons
produits par le canal À. La tension maximale en sortie est d’
1 Vss,

: Fonction identique au pin 1, pour le canal B
: Fonction identique au pin 1, pour le canal C

: Les connexions I0A représentent le port 8 bits du PSG.
Suivant la façon dont elles sont programmées, les connexions
travaillent comme sortie ou entrée. Mais on ne peut fixer
qu'un même mode de travail pour tout le port. On ne peut
avoir simultanément des bits travaillant en entrée et
d'autres en sortie.

: De la fréquence de ce signal sont dérivées par division
toutes les fréquences de son. La fréquence de ce signal
devrait être entre 1 et 2 MHz,

: Un niveau low sur cette connexion annule les valeurs de tous
les registres. Sans reset, les registres contiennent après la
mise sous tension des valeurs aléatoires dont la conséquence
serait un bruit probablement très peu musical.

: Test n'est utilisé que par le constructeur et ne doit pas

être connecté en travail normal,
: Une tension de +5 Volts est placée sur cette connexion,

: Ceci est la connexion de masse du PSG.

1.8.2 La fonction des différents registres du 8912

Comme nous avons maintenant vu comment les registres peuvent être appelés
fondamentalement à travers les connexions BDIR et BC1, nous allons
étudier quelles sont les fonctions remplies par ces registres. Le numéro
de registre utilisé dans la liste suivante est identique au numéro qui
doit être placé dans le registre d'adresse pour appeler le registre

-63-

souhaité.

I est un fait intéressant qui est que le registre d'adresse conserve son
contenu jusqu'à ce qu'il soit à nouveau programmé. On peut donc accéder
sans problème plusieurs fois successives à un registre de données, sans
devoir chaque fois recharger le registre d'adresse.

Mais voici maintenant la description des registres:

Reg 0,1 ;

Reg 2,3 :
Reg 4,5 ,

Reg 6 !

Reg 7 :

Bit O :
Bit 1 :
Bit 2 :
Bit 3 :
Bit 4 :

Ces registres déterminent la période et donc la fréquence du
signal de son sur ANALOG A. Mais les 16 bits ne sont pas tous
utilisés. Tous les 8 bits du registre 0 et les quatre bits
inférieurs du registre 1 sont utilisés. La fréquence peut
être influencée de façon fine avec le registre O ou
grossièrement avec le registre 1, Plus la valeur 12 bits de
ces registres est petite, plus le son est haut.

Fonction comme Reg 0,1 mais canal B.
Fonction comme Reg 0,1 mais canal C.

Ce registre influence le générateur de bruit avec ces 5 bits
inférieurs,

Dans ce registre multi-fonctions, les différents bits
contrôlent des tâches différentes, comme le montre le tableau
suivant :

mettre/couper le son du canal A O=mis/1=n0on
mettre/couper le son du canal B O=mis/1=n0n
mettre/couper le son du canal C O=mis/1=n0n
mettre/couper le bruit du canal À O=mis/1=n0on
mettre/couper le bruit du canal B O=mis/1=n0n

-64-

Bit ©
Bit 6
Bit 7

Reg 8

Reg 9

Reg 10

: mettre/couper le bruit du canal C O=mis/1=n0on
: Port À comme entrée/sortie 0=entrée/1=sortie
: Port À comme entrée/sortie O=entrée/1=sortie

: Ce registre détermine le volume du signal sur le canal A. Les

quatre bits inférieurs sont utilisés pour fixer le volume,

Le bit 4 a une signification particulière. S'il est mis, le
volume est déterminé par le registre de courbe d'enveloppe et
le contenu des bits 0 à 3 est alors ignoré.

: Comme Reg 8 pour le canal B

: Comme Reg 8 pour le canal C

Reg 11,12 :

Reg 13

Les 16 bits de ces deux registres influencent la période de
la courbe d'’enveloppe. Le contenu du Reg 11 est considéré
comme low byte, c'est-à-dire qu’il influence la période par
étapes fines, alors que le Reg 12 est le high byte du
générateur de courbe d’enveloppe.

Les bits O0 à 3 de ce registre déterminent la forme de la
courbe du générateur de courbe d'enveloppe. Il est presque
impossible de rendre compréhensible par des mots
l'affectation de ces bits. C’est pourquoi les courbes
d’enveloppe sont montrées dans le graphique 1.8.2.1.

-65-

1.8.3 Le fonctionnement de l'AY-3-8912 sur le CPC

Nous allons nous intéresser dans cette section à la connection concrète
et certaines choses plus concrètes pour l’utilisation du chip sonore sur
le CPC. Comme la description des registres qui précède était
nécessairement abstraite et peut-être pas très aisément compréhensible,
vous comprendrez mieux, après avoir Iu ce chapitre, certaines
particularités du PSG.

Jetons d’abord un coup d'oeil sur le schéma de fonction,

Le PSG y figure comme IC 102.

Les pins 3, 17 et 19 sont sur +5 Volts. L'AY-3-8912 reçoit son
alimentation électrique à travers le pin 3. Comme BC2 (pin 19) et A8 (pin
17) sont sur +5 Volts, ils n’interviennent pas dans la sélection des
registres,

Les connexions de commande des registres restantes BC1 (pin 20) et BDIR
(pin 18) sont reliées aux bits de port PC6 et PC7 du 8255. Suivant l'état
de ces connexions, des adresses de registre peuvent être communiquées au
PSG ou des données peuvent être écrites ou lues dans le PSG.

Le transfert d'adresse et de données proprement dit se produit à travers
les connexions DO à D7 du PSG qui sont reliées au port À du 8255, Suivant
l'action demandée, le port À doit être programmé comme entrée ou sortie.

Le signal de l'horloge sur le pin 15 est un signal carré d’une fréquence
de 1 MHz, Ce signal est fourni par le gate array par division de la
fréquence quartz. De ce signal sont dérivées par division de fréquence
toutes les fréquences de son et de courbe d’enveloppe.

Le port 1/0 du PSG est relié au clavier et à la connexion pour le
joystick, Vous trouverez dans un prochain chapitre une description
détaillée du clavier et du Joystick, nous ne nous intéressons ici qu'aux
possibilités sonores du chip sonore.

Les connexions les plus importantes de ce circuit intégré sont
certainement les trois sorties analogues A, B et sur les pins 1, 4 et 5.
Ces sorties fonctionnent comme ce qu’on appelle des sorties Open-Emitter.
Pour pouvoir sortir une tension alternative du son, des résistances sont
nécessaires qui commutent entre sortie et masse, C'est la fonction des

résistances R121, R122 et R123.

Le signal sonore est mixé par ces trois résistances à travers les trois
résistances R114, R115 et R116 et il se présente alors sous forme d’un
signal mono sur la connexion 1 du port d'extension. Ce signal mono est
cependant également conduit sur la prise CP001. De là, ce signal arrive à
l'amplificateur et au haut-parleur internes.

Les trois sorties sont cependant en outre conduites également vers la
prise stéréo à l'arrière de l'ordinateur, A cet effet, le signal du canal
B est envoyé de façon identique sur les deux canaux stéréo, à travers les
résistances R118/R119. Les sorties A et C sont chacune envoyées
directement sur un des canaux stéréo, à travers un condensateur de
découplement (R177 et R120).

Ce type de branchement rend même possible, avec une habile programmation,
d'obtenir de véritables effets stéréo. Il serait par exemple imaginable
de ne sortir d’abord un son que sur le canal À. Au bout de quelque temps,
le même son pourrait être sorti en plus sur le canal B. On pourrait, ce
faisant, faire monter lentement le volume du signal sur le canal B, alors
que le volume du signal serait par contre réduit de façon symétrique. Le
résultat serait qu’il semblerait que le son se promène d’un coin de la
pièce vers le milieu entre les deux baffles, De 1à, il peut si nécessaire
continuer vers l’autre coin.

Ces effets sont mêmes possibles en Basic avec la puissante instruction
sound, Le manuel d'utilisation comporte cependant des contradictions dans
l'indication de la répartition des trois canaux de son sur les deux
canaux stéréo. Observez-le après avoir relié votre CPC à une chaîne
stéréo. Seuls les sons du canal B apparaissent sur les deux canaux de la
chaîne stéréo,

Mais comment le PSG produit-il au fond les sons? Examinons un peu comment
les choses se produisent en détail sur un canal,

Comme nous l'avons déjà indiqué, tous les sons sont dérivés du signal de
l'horloge sur le pin 15. Le signal d'horloge est d'abord divisé par 16.
Il en résulte sur le CPC une fréquence de commande et 62,5 KHz, Cette
fréquence est alors conduite vers un diviseur de fréquence programmable.
Suivant le contenu des registres du générateur de son, la fréquence de
commande est ou non à nouveau divisée, pour obtenir la fréquence voulue,
Les développeurs de ce circuit intégré ont à cet égard fait montre de
beaucoup d'’astuce. La chaîne de division n’est pas seulement constituée
de flip-flops qui peuvent diviser la fréquence par deux, Par une
technique de branchement spéciale, des facteurs impairs de division sont

-67-

également possibles. La fréquence de commande peut tout-à-fait être
divisée par 3 ou par 17. C’est uniquement ainsi que toutes les valeurs
nécessaires peuvent être produites dans la zone de fréquences élevées.

Si vous consultez l'annexe du manuel du CPC, vous trouvez pour la note Ré
de la quatrième octave une valeur de période de 27, Comment cette valeur
est-elle obtenue?

La première fois que nous nous sommes posé cette question, nous nous
sommes arraché les cheveux, Quels qu'aient été les calculs que nous
faisions, nous n'obtenions pas de valeur raisonnable. Ce n’est que
plusieurs heures et plusieurs litres de café plus tard que l'idée nous
vint que le magnifique tableau fournit dans le manuel du CPC devait être
faux, L'entrée de la période dans l'instruction SOUND produit une
fréquence qui se situe exactement une octave en dessous de celle
indiquée, L'entrée de ‘’SOUND 1,284,100' ne produit pas la fréquence
attendue de 440 Hertz mais exactement 220 Hertz!

La formule correcte pour le calcul de la période est donc:

PERIODE=ROUND(62500/FREQUENCE )

Le tableau a été vraisemblablement réalisé en partant d’une fréquence de
commande de 2 MHz.

Mais considérons encore la production des sons sur le PSG. Le contenu des
registres du générateur de son détermine donc le facteur de division pour
le signal sonore. Si le registre 0 du PSG reçoit la valeur 100, le
registre 1 la valeur 0, la fréquence de commande sera divisée par 100.
Sur la sortie de la chaîne de division du canal À se trouve un signal
d'une fréquence de 625 Hertz.

Ce signal ne peut cependant pas encore être retiré sur la sortie A, Il
faut d’abord que le canal correspondant soit activé, Ceci est obtenu en
annulant le bit correspondant du registre 7, Comme nous avons choisi dans
notre exemple le canal À, nous devons annuler le bit 0. Mais il faut, ce
faisant, considérer l'état des autres bits, Sur le CPC, cela signifie
concrètement qu’il ne faut pas modifier le bit 6 involontairement car
Sinon le clavier est bloqué.

Mais pour le moment on ne peut entendre encore aucun son, parce que le
volume de chaque canal doit être fixé. Pour le canal À, c'est le registre
8 qui est responsable. Une valeur de 1 ne produit qu’un son très doux,
alors qu’une valeur de 15 donne le volume maximal,

Si nous mettons le bit 4 de registre de volume, les informations
contenues dans les bits O à 3 seront ignorées. Ce sont maintenant les
registres 11, 12 et 13 qui déterminent le volume. Le volume n'est plus
alors fixé sur une valeur mais variable.

Considérons d’abord le registre 13. Ce registre porte le nom officiel de
"ENVELOPE SHAPE/CYCLE CONTROL REGISTER’. Sa fonction sera illustrée plus
aisément grâce à un exemple.

Après que nous ayons fourni les valeurs adéquates aux registres 0, 1, 7
et 8, écrivons donc dans le registre 13 la valeur 12. Les bits 2 et 3
sont maintenant mis, alors que les 2 bits inférieurs sont annulés.

Le tableau fourni dans la description des registres montre pour cette
combinaison une suite de dents montant lentement et retombant rapidement.
En pratique, cela signifie que le volume du son monte tout d'abord
lentement jusqu’au maximum, Puis le son est coupé et le volume recommence
à monter. Cet état demeure jusqu’à ce qu’une nouvelle instruction soit
envoyée au registre 13,

La durée de la montée du volume peut être fixée à travers les registres
11 et 12. Ces registres influencent de façon analogue aux registres des
générateurs de son une autre chaîne de division programmable sur le PSG.
La chaîne de division reçoit un signal qui correspond au signal de
l'horloge divisé par 256, Cela donne une fréquence de 3906,25 Hertz
correspondant à une période d'environ 250 microsecondes,

Si une valeur 1 est écrite dans le registre 11 et une valeur 0 dans le
registre 12 qui travaille comme high-byte, le volume du son est
réellement conduit en 250 microsecondes de O Jusqu'au volume maximum.
Cela figure cependant déjà dans la zone des sons audibles et produit un
sifflement net qui est superposé au son véritablement souhaité.

C'est pour cette raison que les valeurs de registre choisies sont
toujours nettement plus élevées. Avec la valeur maximale (255 dans Reg 11
et Reg 12), la montée Jusqu'au volume maximum dure 16,8 secondes.

L'altération du volume à travers le registre d’enveloppe n’est pas
utilisée par le logiciel du CPC. L'’instruction ENV influence le volume du
son uniquement à travers des manipulations des autre bits inférieurs du
registre de volume. L'instruction ENT du CPC n’a pas d’équivalent sur le
PSG. Cette fonction est produite par une modification habile des
registres du générateur de son.

-69-

REG. 15
83,82,81/80

XO0»b1-<>
MA>ZDMAr»

1,8,2.1 Courbes d'enveloppe du PSG

1.9 Les interfaces du CPC

Le concept d'interface peut être défini comme un point de liaison entre
l'ordinateur et le monde extérieur. Le monde extérieur peut être aussi
bien un autre ordinateur qu’une imprimante ou un autre périphérique,
qu’un appareil de mesure ou un homme, D’après cette définition du monde
extérieur, nous ne décrirons pas seulement dans ce chapitre les
connexions figurant à l'arrière de l'ordinateur mais également le
clavier, la connexion du moniteur et le lecteur de cassette,

Les interfaces les plus importantes pour l'utilisateur sont le clavier et
le moniteur car celles-ci représentent le contact immédiat avec
l'ordinateur. Commençons donc par ces deux interfaces.

1.9.1 Le clavier

Le clavier du CPC comprend en tout 74 touches. Comme les deux touches
SHIFT sont branchées parallèlement, il y donc 73 touches différentes qui
peuvent être interrogées.

La matrice dans laquelle les touches sont rangées comprend 8 fois 10
canaux. Comme les Joysticks peuvent également être interrogés à travers
cette matrice, 79 positions de touche sont donc occupées en tout. Le
second joystick connecté directement sur le premier n’est pas connecté à
des positions autonomes de la matrice, les branchements correspondants
sont parallèles à des touches du clavier,

Du point de vue électronique, le clavier est interrogé à travers le 8255
et le chip sonore. Cela fonctionne à peu près de la façon suivante.

Le 8255 fournit aux sorties de port PCO à PC3 une moitié d'octet qui est
transformée par le décodeur 1C101 en une information décimale, Suivant
l'information figurant en entrée, une des dix sorties devient lon. Ce
décodeur, un 74LS145 est pour cette raison également appelé décodeur BCD-
décimal. Si l'information en entrée n’est pas comprise entre O0 et 9,
toutes les sorties du décodeur sont sur high.

Le port parallèle du chip sonore est programmé pour l'interrogation du
clavier comme port d'entrée. Si aucun signal ne se trouve sur ces
entrées, on obtient lors de la lecture du port un 1 sur toutes les
entrées, en tout donc &8FF.

Soit maintenant une information en entrée sur le décodeur de &04, La

-70-

sortie pin 5 deviendra donc low. Mais le chip sonore ne le prendra pas en
compte tant qu'aucune touche correspondante ne sera enfoncée. Le fait
d'appuyer sur la touche ESC n'aura par exemple aucun effet à ce moment
puisque la sortie pin 8 du décodeur est high. Mais si par contre la
touche ESPACE est enfoncée, la valeur fournie par le chip sonore se
modifiera. À caüse de la touche enfoncée, le bit 7 du port est maintenant
sur la masse et nous obtenons du chip sonore la valeur g7F.

Toutes les touches sont examinées 50 fois par seconde. À cet effet, les
valeurs 0 à 9 sont sorties l'une après l'autre sur les quatre sorties
utilisées du port C et la valeur du chip sonore est examinée après chaque
sortie. Si des touches enfoncées sont alors enregistrées, les touches
enfoncées sont placées dans un tableau et sont si nécessaire converties
en numéros de touche et en caractères correspondants.

Un fait très pratique sur le clavier est que jusqu’à 20 caractères sont
stockés provisoirement. Dans des programmes Basic, on peut déjà commencer
à faire des entrées alors que l'ordinateur n'a pas terminé certains
calculs ou qu'il est occupé à la sortie sur écran. L'interrogation du
clavier n'est bloquée que lors de l'utilisation du’ lecteur de cassette
car il ne reste pas assez de temps pour cela, étant donné le timing très
précis de ces opérations. La seule exception est la touche ESC qui est en
effet nécessaire pour permettre une inventuelle interruption d'une
opération avec le lecteur de Cassette.

Le clavier a par ailleurs une petite particularité. Essayez par exemple
d'appuyer simultanément sur les touches J, K et L. De façon très
surprenante, vous voyez apparaître en outre un H sur l'écran, Cela se
produit toujours lorsque vous appuyez sur trois touches qui constituent
les angles d'un carré dans la matrice du clavier, de même par exemple que
123 ou DFG, Dans ce cas apparaît simultanément le quatrième caractère de
la matrice.

Ce ‘défaut’ est sans grande conséquence et vous pouvez par ailleurs
également interrompre des programmes en appuyant simultanément sur les
touches 2, 3etE,

1.9.2 La connexion vidéo

La connexion vidéo du CPC fournit tous les signaux nécessaires au
fonctionnement d'un moniteur. Il est à cet égard indifférent qu'il

-71 _

s'agisse du moniteur fourni avec ou de (presque) n'importe quel autre.

Le gate array fournit quatre signaux pour le moniteur. Trois signaux
contiennent les informations sur la couleur, le quatrième signal est un
mélange des signaux du CRTC V Sync et H Sync.

Ces signaux sont mixés avec les résistances R131, R132 et R133 ainsi que
R195 et ils sont amplifiés par le transistor @102, Le signal de sortie
ainsi produit est appelé LUM et sert aux moniteurs verts de signal vidéo.
Mais également des moniteurs couleur courants peuvent être utilisés à
travers ce signal pour représenter des couleurs.

Keyboard
Connector

m V#
NÉE

D ,
7

<

=
[N
Lo
Le
=.

CARE)

? | 7 ré ré c ÆaPs

/ LOOK
[7

LA [7
SHIFT

_- Bts
un
ui
N [NN [N
ee ou
se ele
N N [N
ce ml
N [N [N
[NN N [N [N
u- el. |
œ
œ ui
= =
[N NW N
de sl
[N [N
és lelel

[7 [7 [7
10
24 7
NTER A \

[NN N ir]

[ON

p

1.9.1.1, La matrice du clavier

-72-

1.9.3 Le lecteur de cassette

La cassette est un moyen de stockage de données standard remarquable
pour un prix très intéressant.

Même si vous possédez déjà ou acquerrez plus tard un lecteur de
disquette, le lecteur de disquette continuera certainement à vous rendre
de bons services. Comme les disquette utilisées par le CPC sont tout de
même relativement chères, la cassette peut être utilisée comme un moyen
bon marché d'effectuer des copies.

Le lecteur de cassette lui-même est un type de vente courante, ce qui
explique la présence de la touche PAUSE qui est en fait parfaitement
inutile,

L'électronique du lecteur a toutefois été adaptée aux besoins du CPC, Le
signal de sortie est un signal carré avec une amplitude d'environ 5
Volts. Il peut ainsi être traité directement par le bit 7 du port B du
8255,

L'’amplificateur audio qui permet d'entendre le son du CPC a été également
placé sur la platine du lecteur.

Mais venons-en au format d'écriture.

Le lecteur de cassette ne peut fondamentalement stocker les données que
bit par bit. Chaque octet à stocker doit donc être décomposé en ses
différents bits et être transmis sous cette forme. Cette décomposition
est réalisée par le processeur par logiciel, le bit supérieur étant à cet
effet envoyé en premier au lecteur de cassette.

Le signal fourni par le 8255 pour le lecteur de cassette est un signal
carré, Chaque bit est marqué par une vibration carrée, dans laquelle la
phase lon est exactement aussi longue que la phase high. On dit également
que le signal carré a un rapport de 1:1. Un bit O nécessite moitié moins
de temps qu'un bit 1.

C'est pourquoi les indications sur la vitesse d'écriture ne peuvent être
que des indications imprécises. Il est évident qu'un bloc composé
uniquement d'octets O0 sera sauvegardé en deux fois plus de temps qu’un
bloc d'à peu près la même taille ne comportant que des 8FF. Mais comme la
répartition des bits 0 et 1 dans un bloc de données est à peu près égale,
on peut s'en tenir aux indications de 1000 baud (1 baud=1 bit par

-73-

seconde) pour SUPER-SAVE et de 2000 baud pour SPEED-LOAD.

Chaque fichier, qu’il s'agisse d'un fichier programme ou d’un fichier de
données, peut comporter au maximum 65536 octets. Les fichiers sont écrits
par blocs comportant chacun au maximum 2048 octets. Chaque bloc comprend
au maximum huit segments de données de 256 octets. Devant chaque bloc est
écrit un header, c'est-à-dire une tête de bloc

Bien qu’il n'y ait pas de liaison électrique avec l'amplificateur et le
haut-parleur, 11 est possible, même si le volume est baissé, de suivre le
chargement et la sauvegarde de données et de programmes.

Le header de bloc est facile à identifier à l'oreille. On entend en effet
un long ton égal suivi de quelques octets qu’il n'est toutefois pas
possible de distinguer à l'oreille.

Le ton long et égal est une série de 2048 bits 1. Après ces bits vient un
seul bit O puis un octet de synchronisation. La longue suite de bits 1
est nécessaire à l'ordinateur pour déterminer la vitesse (baud-rate). Le
bit O indique à l'ordinateur que cette tête est terminée et l'octet sync
est nécessaire pour distinguer entre l'information du header et les
données,

L'information du header figure dans une zone de données longue de 64
octets qui est transmise devant chaque bloc de 2K de données. Dans ce
fichier header figurent les informations sur le fichier lui-même, par
exemple le nom, si le fichier est ou non protégé, s'il s’agit d’un
programme Basic ou d’un fichier Ascii et quelle est la longueur du
programme.

Octets 0- 15 : Nom du fichier, si moins de 16 octets, rempli avec 00

Octet 16 : Numéro de bloc, dans cet octet figure le numéro qui sera
affiché lors du chargement ou également avec Catalog.

Octet 17 : Si dans cet octet figure une autre valeur que 00, il
s'agit du dernier bloc du fichier.

Octet 18 : Cet octet contient le type de fichier. L'information est

codée dans les différents bits. La signification des bits
vient à la suite de ce tableau.

-74-

Octets 19,20 :

Octets 21,22 :

Octet 23

Octets 24,25 :

Octets 26,27 :

Ces octets contiennent la longueur des informations du
fichier. Si le bloc, donc les 2 K, est entièrement écrit,
ces octets contiennent la valeur 80800. Dans le dernier ou
unique bloc, figure ici le nombre d’octets du bloc.

Ces octets indiquent l'adresse de chargement, à partir de
laquelle les données ont été écrites à l'origine, Pour les
programmes Basic, c'est l'adresse décimale 368, pour les
fichiers binaires, donc pour le langage-machine, c’est
normalement l'adresse où tourne le programme en mémoire.

: Si le contenu de cet octet est différent de 0, il s’agit

du premier bloc du fichier.
Ces octets contiennent la longueur du fichier.

Les possibilités de ces octets ne sont malheureusement pas
soutenues directement par le Basic du CPC. Elles
contiennent l'adresse de début d’un fichier en langage-
machine, qui n’est pas en effet nécessairement identique à
l'adresse de chargement. Ces octets permettent par
programmation de réaliser un ‘’auto-start'.

Les octets restants 28 à 63 du header ne sont pas utilisés par le système
d'exploitation et sont à la disposition des programmeurs chevronnés.

Mais voici maintenant le codage des bits de l’octet 18 du header:

Bit 0

Bits 1-3

Bits 4-7

: Si ce bit est mis, le fichier correspondant est déclaré

protégé, Les programmes protégés peuvent être également
produits en Basic avec ‘SAVE "NOM”,p’.

: Ces bits déterminent le type de fichier. Bien que trois

bits permettent 8 différents types de fichier, seuls les
types de fichier programme Basic (0), fichier binaire (1)
et fichier de données ascii (3) sont utilisés.

: Ces bits comportent normalement un 0, seuls les fichiers

Ascii ont un 1 dans le bit 4.

-75-

Comme nous l'avons déjà indiqué, les informations stockées dans les
différents blocs sont encore subdivisées en différents segments. Chaque
segment se compose de 256 octets de données et d’octets de checksum
(contrôle du total). La checksum de chaque segment est calculée d’après
une formule spéciale et permet de vérifier lors de la lecture du fichier
si les bits ont été correctement transmis, Dès lors que la checksum
calculée ne correspond pas aux valeurs lues, le READ ERROR B est affiché,
Le READ ERROR A indique qu’un bit a été 1u dont la durée était trop
longue par rapport aux valeurs calculées pour les bits nuls ou 1. Cette
erreur se produit souvent lors de la lecture de programmes, lorsque la
cassette qui coinçait lors de la sauvegarde est maintenant fluide.

La troisième erreur possible est le READ ERROR D. Cette erreur ne devrait
se produire que rarement car elle signale que le bloc lu est plus long
que les 2048 octets autorisés, Cela ne peut toutefois se produire que si
l'utilisateur écrit dans les informations du header, lors de la
sauvegarde, des valeurs plus grandes que celles autorisées,

Vous connaissez certainement l'instruction Basic ‘SPEED WRITE par’.
Suivant les paramètres utilisés, les données sont stockées sur la
cassette à une vitesse moyenne de 1000 ou 2000 baud. Ceci n'atteint
cependant pas encore la vitesse la plus grande possible. Par
l'utilisation d’une routine du système d'exploitation, la vitesse
(baudrate) peut être fixée à toute valeur comprise entre 700 et environ
3600 baud. La routine nécessaire est à l'adresse eBC68. Elle attend des
paramètres dans deux registres et fixe la vitesse d'écriture en fonction
de ces paramètres. Une valeur est transmise à la paire de registres HL
qui détermine la vitesse (baudrate). La formule pour déterminer cette
valeur est:

Baudrate=333333/moitié de la longuéur d’un bit nul

Cela donne pour 1000 baud une vitesse de 666 microsecondes pour un bit
nul: un bit 1 dure exactement le double.

L'électronique utilisée dans le lecteur de cassette a cependant une
particularité. Si des bits nuls et des bits 1 sont lus tour à tour,
l'électronique essaye de combler les différences de durée. Les bits 1
deviennent de ce fait plus court, alors que les bits nuls apparaissent
comme des : impulsions plus longues qu’on ne l'aurait attendu après
l'écriture, Pour cette raison, une compensation anticipée doit être

exécutée et les bits nuls sont écrits plus brièvement, alors que les bits
1 sont écrits avec des durées légèrement plus longues. Ces durées
nécessaires pour la compensation anticipée sont transmises à la routine
dans l’accumulateur.

Pour des tentatives de fixer la vitesse d'écriture la plus rapide, qui
est à moitié fiable, il suffit de transmettre dans l'accumulateur une
valeur de 10. Pour écrire avec une vitesse de 3600 baud, il faut activer
la routine suivante:

LD HL,93
LD A,10
CALL eBC68
RET

Ces quelques octets peuvent facilement être placés dans la mémoire avec
les lignes suivantes:

10 MEMORY HIMEM - 10

20 FOR 1 = 1 109

30 READ X : POKE HIMEM + I,X

4O NEXT I

50 CALL HIMEM + 1

60 DATA 821,85D, 800, 83E, &0A, &CD, 868, &BC, C9

Ne craignez pas de faire varier quelque peu les valeurs dans HL et dans
l’accumulateur (les deuxième et cinquième valeurs de la ligne de Data),
pour déterminer la plus haute fréquence d'écriture possible, Elle dépend
des cassettes utilisées. Mais les propriétés de rotation régulière de
votre lecteur de cassette Jouent également un rôle non négligeable.

Si les valeurs sélectionnées sont trop petites, le CPC ne peut plus alors
tenir les durées réclamées et vous obtenez comme résultat le message
d'erreur WRITE ERROR A.

Encore un conseil pour finir:

Vous avez certainement remarqué que lorsque vous sauvegardez de très
longs programmes avec de nombreuses variables, cela peut durer jusqu’à 15
minutes jusqu’à ce que les données ou le programme soient sauvegardées.
Cela vient du fait que le CPC nécessite pour la sauvegarde une zone de 2K

-77-

pour les blocs à transférer. Ce buffer est placé dans la limite
supérieure de la mémoire, Si cette zone est toutefois occupée par des
variables, ces variables sont recopiées dans une autre zone de la
mémoire. Ce procédé est comparable à la redoutable garbage collection qui
se produit toujours lorsqu'il n'y a plus de place suffisante en mémoire
pour les chaînes de caractères et les tableaux.

Le délai d'attente provoqué par le transfert des variables peut cependant
etre notablement réduit si ce buffer de 2K est déjà installé et protégé
au début de chaque programme. Un début de programme possible pourrait se
présenter ainsi:

10 OPENOUT “’DUMMY"

20 MEMORY HIMEM - 1

30 CLOSEOUT

40

50 ‘RESTE DU PROGRAMME

Ce procédé n'a bien sûr de sens que si vous travaillez dans le programme
en question avec des fichiers. Si ce n'est pas le cas, vous pouvez
renoncer à ces lignes de programme et entrer simplement l'instruction
CLEAR avant la sauvegarde. Toutes les variables définies auparavant
seront ainsi supprimées et l'installation du buffer de cassette se fera
sans délai notable.

1.9.4 L'interface d'imprimante centronics

On trouve sur tout ordinateur quelque chose qu’on considère comme pouvant
etre amélioré. Sur le CPC, c'est sans conteste l’interface imprimante. On
a malheureusement trop économisé ici.

Nous ne pensons pas à la réalisation mécanique de la connexion. Le choix
fait par le constructeur est certainement le moins cher pour lui mais il
n'est pas non plus sans avantage pour le possesseur de l'ordinateur car
les câbles de connexion nécessités sont bon marché et vraiment fiables.
La cause de notre ‘mauvaise humeur’ est le fait que l'interface ne
dispose que de 7 bits. La plus part des imprimantes, y compris celle
proposée par AMSTRAD pour le CPC, ont une entrée 8 bits et donc de
nombreuses commandes et possibilités de ces imprimantes ne peuvent être
obtenues que par des détours, ou même ne peuvent pas être obtenues du
tout,

-78-

Mais considérons d’abord la structure électronique de cette interface.
L'interface se compose principalement de 1'’1C106, latch 8-pôles 74LS273,
Les huit différents latchs travaillent comme des flip-flops,
l'information envoyée sur les entrées est stockée avec une bascule high-
lon sur l'entrée d'horloge pin 11 et elle est disponible sur les sorties,
jusqu'à un RESET ou à une nouvelle programmation, quelles que soient les
modifications sur les signaux d'entrée,

Le signal d'horloge dont la bascule high-low déclenche le stockage des
Valeurs d'entrée est produit avec la porte logique OR 74LS32, IC112, pins
11, 12 et 13, La sortie pin 11 devient low, lorsque les deux entrées sont
10w.

La connexion de l'imprimante est également appelée à travers l'adressage
de port. C'est pourquoi le signal IOWR* se trouve sur une entrée de la
porte logique OR et que le canal d'adresse A12 se trouve sur l'autre
entrée.

Comme sur les autres éléments périphériques, le décodage est ici donc
également très incomplet. Les canaux d'adresse qui ne sont pas utilisés
Four le décodage doivent donc être high pour éviter des collisions avec
d'autres adresses de port utilisées. Ceci donne une adresse de port
effective de 8EFxx,

Les entrées du latch de l'imprimante sont reliées au bus de données du
processeur. Les sorties se trouvent sur la connexion de l'imprimante,
Seul le bit 7 est envoyé au port Centronics à travers une porte logique
NAND de 1’I1C110 utilisée comme inverseur, Ce bit représente le signal
strobe nécessité par l'imprimante, Ce signal est normalement high. Mais
si l'ordinateur veut envoyer un caractère à l'imprimante, il envoie
l'octet à transmettre sur les canaux de données et place peu après le
Signal strobe sur low. L'octet à transmettre est ainsi accepté par
l'imprimante,

À condition toutefois que le signal busy de l'imprimante soit low. L'état
du signal busy est interrogé par le bit 6 du port B du 8255,

Mais comment le signal strobe peut-il être produit? Rien de plus simple.
Chaque octet à transmettre est d’abord ANDé avec 87F. Le bit supérieur de
l'octet est ainsi supprimé de façon certaine. Cet octet est sorti sur le
port de l'imprimante par une instruction OUT.

Les bits à transmettre se trouvent maintenant déjà sur l'imprimante, mais
le signal strobe est toujours high, à travers l’inverseur. C'est pourquoi
on met ensuite avec OR 880 le bit 7 de la valeur à sortir qui est

-79-

également sortie sur le port imprimante. La valeur à transmettre n'a pas
été modifiée, seul le signal strobe est devenu low à travers l’inverseur.
Ce signal doit cependant redevenir également high, c'est pourquoi le bit
supér jeur est à nouveau supprimé avec AND et l'octet est à nouveau sorti.
Un octet a été ainsi envoyé de l'ordinateur à l'imprimante,

La sortie sur l'imprimante ne pose pas de problème en Basic. Mais même en
langage-machine, il n'est pas nécessaire d'écrire soi-même toute cette
procédure. Il y a plusieurs routines système qui vous évitent une bonne
part de ce travail de programmation.

Il y a d’abord la routine dont l'entrée est en 8&BD2B. A travers cette
routine, vous pouvez sortir un caractère sur l'imprimante. Le caractère
doit chaque fois se trouver dans l'accumulateur. Cette routine teste en
outre si l'imprimante est ‘’busy’. Si l'imprimante ne répond pas dans un
délai de 0.4 secondes, la routine revient avec un flag carry nul, Il faut
alors faire une nouvelle tentative avec le même caractère. Cette routine
est également utilisée par l'interpréteur Basic. Si la transmission est
réussie, le carry est mis. Le prochain caractère peut alors être envoyé.

Une autre routine a son entrée trois octets plus loin (8&BD2E). Cette
routine peut être utilisée pour examiner l'état de l'imprimante. Si
aucune imprimante n'est connectée ou si l'imprimante répond ‘busy’, si
elle ne peut donc pas recevoir de caractères pour le moment, cette
routine revient avec un carry mis, sinon le carry est supprimé.

La troisième routine exploitable (&BD31) accomplit toutes les procédures
nécessaires à la sortie d'un caractère sur l'imprimante. Le programmeur
doit cependant tester alors auparavant si l'imprimante est prête à
recevoir puis transmettre le caractère voulu dans l’accumulateur. Si le
test de l’état de l'imprimante est négligé, le caractère peut
éventuellement se perdre dans le ‘vide’.

Comment ces routines peuvent être mises en oeuvre, nous vous
l'indiquerons plus tard dans cet ouvrage. Nous vous montrerons en effet
pour l'exemple d’un hardcopy de texte et de graphisme, comment utiliser
ces routines et d’autres.

Mais il convient de tenir compte d’une autre particularité de cette

connexion Centronics.
La disposition des contacts du port d'imprimante incite à se procurer les

-80-

fiches nécessaires ainsi qu’un bout de câble plat pour réaliser soi-même
un tel câble. Si les connecteurs sont en outre des pinces crocodile, même
des possesseurs de CPC peu doués manuellement peuvent réaliser un tel
câble en 5 à 10 minutes. Toutes les imprimantes Centronics peuvent être
alors utilisées.

Mais lors du premier essai de fonctionnement, vous aurez une grosse
surprise, L'imprimante dépense curieusement le papier très généreusement,
Une ligne vide est ajoutée après chaque ligne imprimée.

La raison en est la suivante:

Le CPC ajoute à la fin de chaque ligne imprimée la suite de caractères
CR/LF (Carriage Return, Line Feed) c'est-à-dire la suite d'instructions
pour retour de chariot et passage à la ligne. Le papier avance donc d'une
ligne. De plus, et sans raison très claire, le pin 14 de la connexion
centronics du CPC est cependant encore relié à la masse, Cela produit sur
la plus part des imprimantes un passage à la ligne supplémentaire, de
sorte qu’une ligne vide est ainsi toujours produite.

La solution est dans ce cas l'interruption du canal menant au pin 14.
Après avoir écarté ce canal et éventuellement installé des commutateurs
dans l'imprimante si nécessaire comme par exemple sur Epson, tout devrait
fonctionner correctement,

1.9,5 La connexion du joystick

La connexion du Joystick est certainement utilisée principalement dans un
but qui ustifie son nom: comme entrée pour l'interrogation d'un

joystick. A travers 7 des 9 connexions disponibles, il est cependant
également possible d'interroger d'autres touches ou commutateurs, Par
programmation et en renonçant aux interruptions et à l'interrogation du
clavier, ces sept connexions pourraient même être employées comme sortie.
Les connexions de Joystick sont en effet reliées au port bi-directionnel
du chip sonore et pourraient travailler comme sortie, sous les
contraintes indiquées. Le port Centronics est cependant plus facile à
manipuler pour effectuer une sortie.

Comme nous l'avons déjà décrit au chapitre 1.9.1, les joysticks sont
considérés comme des touches du clavier. C'est pour cette raison que les
7 entrées nécessaires du port du chip sonore sont placées sur la prise du
joystick. Deux sorties du décodeur BCD-décimal 1C101 sont encore en outre

-81 _

placées sur la prise.

Tous les cinquantièmes de seconde, le clavier est interrogé entièrement.
L'état des Joysticks est également interrogé à cette occasion, Pour les
programmeurs Basic, l'état des Joysticks est fourni par la fonction
JOY(numéro). L'état des Joysticks pourrait être également déterminé
Simplement avec INKEY, Mais également pour les fanas de l'assembleur, il
est possible de déterminer facilement l'état des joysticks, La routine
système &BB24 fournit dans le registre double HL l’état actuel des
Joysticks. En appelant cette routine, on obtient l’état du Joystick 0
dans le registre H et le registre L vaut pour le joystick 1. Le codage
des touches Joystick suit le même schéma qu'avec la fonction JOY(x).

1.9.6 Le connecteur d'extension

Cette interface est la plus universelle du CPC. Sur cette carte de
conducteurs à 50 pôles se trouvent, outre tous les signaux du processeur,
différents signaux de commande. C’est ici que sont connectées toutes les
extensions du système,

La signification des signaux 3 à 39 nous est connue puisqu'elle découle
de la description du processeur. C'est pourquoi nous allons nous limiter
ici aux connexions restantes.

Sur le pin 1 figure encore une fois le signal sonore. Ce signal n'est
toutefois que mono, les trois canaux sont conduits ici.

Les pin 2 et 49 sont reliés à la masse de l'alimentation électrique.

Une particularité est constituée par le signal BUS-RESET* sur le pin 40.
En plaçant ce signal à low, on provoque un reset du système.
Malheureusement, le CPC vide toute la mémoire lors d'un reset, Ce signal
n'est donc comme signal d'alarme pas plus efficace que le fait de couper
puis de rallumer l'ordinateur.

Sur le pin 41 figure le signal reset proprement dit pour les extensions
extérieures. Notez cependant que tous les composants ne peuvent pas être
alimentés avec ce signal. Le 8255 a par exemple besoin de ce signal sous
sa forme inversée.

-82-

Les deux signaux ROMEN* et ROMDIS sont très intéressants. Le signal
ROMEN* qui se trouve sur le pin 42 signale par son niveau low un accès à
la Rom intégrée de 32 K. Cet accès peut cependant être interdit par un
niveau high sur le pin 43, ROMDIS. La totalité de la Rom intégrée peut
donc être ainsi remplacée par des Roms ou Eproms extérieures,

Par un décodage approprié des canaux d'adresse, il est cependant
également possible de ne masquer et remplacer que des zones déterminées
de la Rom intégrée,

Les deux signaux RAMRD* und RAMDIS ont une fonction semblable pour les
accès en lecture sur la Ram interne. Ces signaux sur, les pins 43 et 44
peuvent être utilisés pour échanger par exemple des zones de mémoire
déterminées avec des Roms ou même des Rams,

La commande de Rams extérieures n’est cependant pas très simple sur le
CPC. La principale difficulté vient du fait que le signal WR* pour les
Rams internes n'est pas produit par le processeur mais par le Gate Array.
Cette impulsion d'écriture ne peut malheureusement (à notre connaissance)
etre empêchée par aucune astuce de programmation, de sorte qu’un accès en
écriture à une Ram externe adresse toujours également et écrit sur la Ram
interne,

Le signal CURSOR envoyé sur le pin 46 est fourni avec: une programmation
appropriée par le contrôleur vidéo. Le CRTC dispose en effet de la
possibilité offerte par le curseur électronique. Suivant la
programmation, un signal carré d'une fréquence d'environ 1.5 ou 3 Hertz
apparaît sur cette sortie. Mais il est également possible de programmer
sur cette connexion des niveaux low ou high permanents.

Après l’allumage du CPC, c'est un niveau low permanent qui figure ici.

L'entrée LPEN (Light Pen) sur le pin 47 est reliée directement avec
l'entrée light-pen du CRTC. Ce circuit intégré dispose de tous les
registres nécessaires pour la gestion du lightpen,

L'utilisation du light pen, surtout en graphisme aute résolution est

cependant difficilement réalisable sur le CPC car le contrôleur vidéo
fournit certes l'adresse MA de la position actuelle du light-pen mais il
n'indique pas l'adresse RA actuelle, Du fait de la structure spéciale de
la Ram vidéo, cette indication est cependant nécessaire si l’on veut
dessiner sur l’écran avec le light-pen.

L'entrée pin 48 porte la désignation EXP* et est reliée au port B du 8255

-83-

Bit 4. Une extension extérieure peut placer cette connexion sur la masse
et se faire ainsi remarquer par le système d'exploitation.

Le dernier signal à évoquer, sur le Pin 50, est le signal d'horloge du
processeur. Ce signal avec une fréquence de 4 MHz est par exemple utilisé

par le contrôleur du lecteur de disquette.

-84-

2 LE SYSTEME D'EXPLOITATION

Derrière ce nom qui ne dit rien au non initié, se cache le coeur de
l'ordinateur, C'est ici qu'est réalisée la liaison entre programme de
l'utilisateur et le matériel.

L'intérpréteur Basic doit à cet égard être considéré lui-même comme un
programme qui accède à travers le système d'exploitation à l'électronique
de l'ordinateur.

La structure du système d'exploitation est organisée logiquement et
clairement en sections où packs dont chacune a une fonction particulière.
Cela commence au niveau inférieur par le MACHINE PACK qui est la partie
la plus proche de l'électronique et qui sert par exemple le port
d'imprimante, les registres de son, etc..., cela continue avec le SCREEN
PACK qui contrôle l'écran et qui est appelé par le TEXT PACK ou le
GRAPHICS PACK,

Un examen plus approfondi montre que chaque pack est strictement
délimité et fermé et que la communication avec les autres packs ne se
fait qu’à travers certaines interfaces bien définies. En outre, chaque
pack dispose d'une zone de Ram propre qu'il emploie comme mémoire de
travail. L'appel des routines se produit en règle générale à travers des
vecteurs de la Ram ou, plus rarement, directement à travers l'adresse de
la Rom,

Cela incline à supposer que le système d'exploitation, probablement à
cause de peu de temps disponible, a été écrit par plusieurs programmeurs,
chacun étant responsable d’un ou plusieurs packs et après qu'on se soit
entendu uniquement sur les interfaces.

Quoi qu’il en soit, cette structure claire et l'accès par des vecteurs à
tous les coins et recoins ouvrent au programmeur des horizons
insoupçonnés et tout à fait inconnus Jusqu'ici.

Citons simplement comme exemple la possibilité d'écrire une routine pour
une véritable imprimante 8 bits (sans parler du problème de la connexion)
et de rendre cette routine utilisable par le système simplement en
modifiant le vecteur MC WAIT PRINTER.

Cette indication doit également vous servir d'avertissement: ne craignez
pas d'utiliser les routines du système d'exploitation, mais ne les
utilisez qu’à travers les vecteurs! 11 se pourrait en effet que quelqu'un
d'autre (cartouche Rom) ait déplacé quelques vecteurs pour faire exécuter
certaines fonctions par des routines propres.

Vous constaterez à l'usage qu’il est possible d'écrire des programmes

-85-

propres en un minimum de temps, pour peu qu’on utilise scrupuleusement
les vecteurs. Ce qui est entièrement nouveau, c’est que même les routines
arithmétiques du Basic tournent avec ce mécanisme ce qui peut vous
permettre d’une part d'y faire exécuter vos propres calculs et d'autre
part d’y placer vos propres programmes si vous souhaitez par exemple une
plus grande précision.

Puisque nous vous avons montré notre enthousiasme pour les vecteurs,
c'est aussi avec eux que nous commencerons dans le chapitre suivant,

2.1 _ Les vecteurs du système d'exploitation

Nous vous présentons dans les pages suivantes les adresses de la Ram à
travers lesquelles vous pouvez appeler des routines du système
d'exploitation ou que vous pouvez au besoin modifier pour faire exécuter
certaines fonctions par vos propres programmes.

La fonction de la routine est indiquée en quelques mots lorsque le nom
même de la routine n’est pas suffisamment explicite. Vous trouverez des
indications plus précises sur certaines parties dans les introductions
des différents ‘packs’.

Il s’agit pour une part de routines complètes qui ont été copiées ici et
au beau milieu desquelles il vous est possible de sauter en cas de besoin
et pour une autre part de RST 1 ou RST 5 suivie de l'adresse Inline
(voyez à ce sujet le chapitre 1.1.2) qui concerne la Rom.

Vous pouvez lire dans l'annexe 4.1 où ces routines figurent dans la Rom.

B900 KL U ROM ENABLE

B903 KL U ROM DISABLE

B906 KL L ROM ENABLE

B909 KL L ROM DISABLE

B90C KL ROM RESTORE Réactive l’ancienne configuration Rom

B90F KL ROM SELECT Active une Rom d'extension (théoriquement, il peut y
en avoir Jusqu'à 252)

B912 KL CURR SELECTION Quelle Rom d'extension est actuellement en
fonction ?

B915 KL PROBE ROM De quel type d'extension de la Rom s’agit-11?

B918 KL ROM DESELECT Reconstituer extension de la Rom précédente

B91B KL LDIR

B91E KL LDDR

B921 KL POLL SYNCHRONOUS Ÿ a-t-1l un Event avec une priorité supérieure à

-86-

celle de l'Event actuel?
B939 RST 7 INTERRUPT ENTRY CONT'D

B97C KL

LOW PCHL CONT'D

B982 RST 1 LOW JUMP CONT'D
B9A8 Préparer ‘configuration et exécuter saut

B9B1 KL
B9B9 KL

FAR PCHL CONT'D
FAR ICALL CONT'D

B9BF RST 3 LOW FAR CALL CONT'D

BA10 KL

SIDE PCHL CONT'D

BA16 RST 2 LOW SIDE CALL CONT'D
BA2E RST 5 FIRM JUMP CONT'D

BAUA KL
BA5S4 KL
BASE KL
BA72 KL
BA7E KL
BA83 KL
BA8C KL
BAA2 KL
BAAG KL
BAAC KL

L ROM ENABLE CONT'D

L ROM DISABLE CONT'D

U ROM ENABLE CONT'D
ROM RESTORE CONT'D

ROM SELECT CONT'D
PROBE ROM CONT'D

ROM DESELECT CONT'D
CURR SELECTION CONT'D
LDIR CONT'D

LDDR CONT'D

BACB RST 4 RAM LAM CONT'D
BADC RAM LAM (IX) correspond à ld a, (1x)

BB0O KM
BBO3 KM
BB06 KM
BB0O9 KM
BBOC KM
accès

BBOF KM
BB12 KM

BB15 KM
BB18 KM
BB1B KM
frappée
BB1E KM
BB21 KM
BB24 KM
BB27 KM

INITIALISE

RESET

WAIT CHAR Attendre un caractère du clavier

READ CHAR Aller chercher un caractère du clavier s’il y en a un
CHAR RETURN placer caractère dans buffer clavier pour prochain

SET EXPAND Constituer chaîne d'extension
GET EXPAND Retirer caractère de chaîne d'extension

EXP BUFFER Affecter mémoire pour chaîne d'extension
WAIT KEY Attendre frappe d’une touche
READ KEY Aller chercher numéro de touche, si une touche a été

TEST KEY Une touche a été frappée?

GET STATE Aller chercher état SHIFT

GET JOYSTICK

SET TRANSLATE Recevoir entrée dans table clavier (premier niveau)

-87-

BB2A

KM GET TRANSLATE Aller chercher entrée dans table du clavier

(premier niveau)

BB2D
BB30
BB33
BB36
BB39
BB3C
elle
BB3F
BB42
BB45
BB48
BB4B
BBUE
BB51
BB54
BB57
BB5SA
BB5D
BB60
BB63

KM SET SHIFT Comme BB27 pour le deuxième niveau
KM GET SHIFT Comme BB2A pour le deuxième niveau
KM SET CONTROL Comme BB27 pour le troisième niveau
KM GET CONTROL Comme BB2A pour le troisième niveau
KM SET REPEAT Fixer fonction de répétition pour touche déterminée
KM GET REPEAT La fonction de répétition d’une touche déterminée est-
activée
KM SET DELAY Fixer fréquence et vitesse de répétition des touches
KM GET DELAY Aller chercher paramètres ci-dessus
KM ARM BREAK Autoriser touche Break
KM DISARM BREAK Verrouiller touche Break
KM BREAK EVENT Exécuter routines si touche Break frappée
TXT INITIALISE
TXT RESET
TXT VDU ENABLE Des caractères peuvent être écrits sur l'écran
TXT VDU DISABLE Interdire représentation des caractères
TXT OUTPUT Représenter ou exécuter caractère (de commande)
TXT WR CHAR Représenter caractère
TXT RD CHAR Lire caractère de l'écran
TXT SET GRAPHIC Activer ou désactiver la représentation de

caractères de commande

BB66
BB69
BB6C
BB6F
BB72
BB75
BB78
BB7B
BB/E
BB81
BB84
BB87
BB8A
BB8D
BB90
BB93
BB96

TXT WIN ENABLE Fixer dimensions fenêtre texte actuelle
TXT GET WINDOW Quelles dimensions a la fenêtre actuelle
TXT CLEAR WINDOW Vider fenêtre de texte actuelle

TXT SET COLUMN

TXT SET ROH

TXT SET CURSOR

TXT GET CURSOR

TXT CUR ENABLE Autoriser le curseur (programme utilisateur)
TXT CUR DISABLE Verrouiller le curseur (utilisateur)

TXT CUR ON Autoriser le curseur (système d'exploitation)
TXT CUR OFF Verrouiller le curseur

TXT VALIDATE Curseur dans la fenêtre texte?

TXT PLACE CURSOR Activer curseur

REMOVE CURSOR Désactiver curseur

TXT SET PEN Fixer couleur du premier plan

TXT GET PEN Couleur du premier plan?

TXT SET PAPER Fixer couleur fond

-88-

BB99 TXT GET PAPER Couleur fond?

BB9C TXT INVERSE Echanger entre elles les couleurs actuelles du fond et
du premier plan

BB9F TXT SET BACK Activer/désactiver mode transparent

BBA2 TXT GET BACK Mode transparent?

BBA5 TXT GET MATRIX Aller chercher adresse de la carte points d’un
caractère

BBA8 TXT SET MATRIX Fixer l'adresse de la carte points (défihie par
l'utilisateur) d'un caractère déterminé

BBAB TXT SET M TABLE Fixer adresse de départ et premier caractère d’une
matrice de points définie par l'utilisateur

BBAE TXT GET M TABLE Adresse de départ et premier caractère d’une matrice
utilisateur

BBB1 TXT GET CONTROLS Aller chercher adresse de la table de saut des
caractères de commande

BBB4 TXT STR SELECT Choisir fenêtre de texte

BBB7 TXT SHAP STREAMS Les paramètres (couleurs, limites des fenêtres,
etc...) de deux fenêtres de texte sont échangés entre eux

BBBA GRA INITIALISE

BBBD GRA RESET

BBCO GRA MOVE ABSOLUTE

BBC3 GRA MOVE RELATIVE

BBC6 GRA ASK CURSOR Où est le curseur actuel?

BBC9 GRA SET ORIGIN

BBCC GRA GET ORIGIN

BBCF GRA WIN WIDTH Fixer limites gauche et droite de la fenêtre
graphique

BBD2 GRA WIN HEIGHT Fixer limites supérieure et inférieure de la fenêtre
graphique

BBD5 GRA GET W WIDTH Limites gauche et droite de la fenêtre graphique?
BBD8 GRA GET W HEIGHT Limites supérieure et inférieure de la fenêtre
graphique

BBDB GRA CLEAR WINDOW Supprimer fenêtre graphique

BBDE GRA SET PEN Fixer couleur d'écriture

BBE1 GRA GET PEN Couleur d'écriture?

BBE4 GRA SET PAPER Fixer couleur fond

BBE7 GRA GET PAPER Couleur fond?

BBEA GRA PLOT ABSOLUTE Fixer point graphique (absolu)

BBED GRA PLOT RELATIVE Fixer point graphique (relativement au curseur
actuel)

-89-

BBFO GRA TEST ABSOLUTE Point mis? (absolu)

BBF3 GRA TEST RELATIVE Point mis (relativement au curseur actuel)

BBF6 GRA LINE ABSOLUTE Tracer ligne de position actuelle à position
absolue.

BBF9 GRA LINE RELATIVE Tracer ligne de position actuelle à distance
relative

BBFC GRA WR CHAR Ecrire un caractère dans la position graphique actuelle
BBFF SCR INITIALISE

BCO2 SCR RESET

BCO5 SCR SET OFFSET Fixer adresse de départ du premier caractère
relativement à l'adresse de base de la Ram vidéo

BCO8 SCR SET BASE Fixer adresse de base de la Ram vidéo

BCOB SCR GET LOCATION Début actuel de l'écran? (Baseroffset)

BCOE SCR SET MODE

BC11 SCR GET MODE

BC14 SCR CLEAR Vider l’écran

BC17 SCR CHAR LIMITS Aller chercher nombres maxi de lignes et de colonnes
de l'écran (suivant le mode)

BC1A SCR CHAR POSITION

BC1D SCR DOT POSITION

BC20 SCR NEXT BYTE Augmenter une adresse d'écran donnée d’une position de
caractère.

BC23 SCR PREV BYTE Diminuer l'adresse d'écran d'une position,

BC26 SCR NEXT LINE AUGMENTER L'adresse d'écran d’une ligne.

BC29 SCR PREV LINE Diminuer l'adresse d'écran d'une ligne.

BC2C SCR INK ENCODE

BC2F SCR INK DECODE

BC32 SCR SET INK Affecter couleur(s) à une Ink-#.

BC35 SCR GET INK Couleur(s) à une Ink-#?

BC38 SCR SET BORDER Composer couleur(s) du cadre.

BC3B SCR GET BORDER Couleur(s) du cadre?

BC3E SCR SET FLASHING Fixer périodes de clignotement.

BC41 SCR GET FLASHING Périodes de clignotement?

BC44y SCR FILL BOX Remplir fenêtre existante avec une couleur (positions
relatives aux caractmres, suivant le mode).

BC47 SCR FLOOD BOX Remplir fenêtre existante avec une couleur (positions
sont adresses d'écran, indépendantes du mode).

BC4A SCR CHAR INVERT Pour un caractère inverser couleur de premier plan
et couleur du fond.

BC4D SCR HW ROLL Décaler l'écran d'une ligne vers le haut ou d'une ligne

-90-

vers le bas (selon le hardware).

BC50 SCR SW ROLL Décaler l'écran d’une ligne vers le haut ou d'une ligne
vers le bas (selon le software).

BC53 SCR UNPACK Agrandir matrice de caractère (pour mode0/1).

BC56 SCR REPACK Refondre matrice de caractère dans sa forme originale.
BC59 SCR ACCESS Fixer caractère de commande visible/invisible.

BCSC SCR PIXELS Fixer point à l'écran.

BC5F SCR HORIZONTAL Tracer ligne horizontale.

BC62 SCR VERTICAL Tracer ligne verticale.

BC65 CAS INITIALISE

BC68 CAS SET SPEED Fixer vitesse d'écriture.

BC6B CAS NOISY Entrée/sortie de messages de cassette.

BCGE CAS START MOTOR

BC71 CAS STOP MOTOR

BC74 CAS RESTORE MOTOR Rétablir ancienne position de moteur.

BC77 CAS IN OPEN

BC7A CAS IN CLOSE

BC7D CAS IN ABANDON Fermer aussitôt fichier d'entrée.

BC80 CAS IN CHAR Lire caractère (du buffer).

BC83 CAS IN DIRECT Entrer tout le fichier dans la mémoire.

BC86 CAS RETURN Rentrer le caractère lu le dernier dans le buffer.
BC89 CAS TEST EOF Fin de fichier?

BC8C CAS OUT OPEN

BC8F CAS OUT CLOSE

BC92 CAS OUT ABANDON Fermer aussitôt fichier de sortie.

BC95 CAS OUT CHAR Ecrire caractère (dans le buffer).

BC98 CAS OUT DIRECT Ecrire zone de mémoire définie sur cassette (sans
passer par le buffer).

BC9B CAS CATALOG

BCE CAS HRITE Ecrire bloc,

BCA1 CAS READ Lire bloc.

BCA4 CAS CHECK Comparer bloc sur bande avec contenu de la mémoire.
BCA7 SOUND RESET

BCAA SOUND QUEUE Placer le son à la queue.

BCAD SOUND CHECK Encore de la place dans la queue?

BCBO SOUND ARM EVENT Block d'event pour provoquer la libération d’une
place dans la queue.

BCB3 SOUND RELEASE Permettre des sons,

BCB6 SOUND HOLD Tenir aussitôt les sons

BCB9 SOUND CONTINUE Continuer de traiter les sons auparavant tenus.

-91 _

BCBC SOUND AMPL ENVELOPE Dresser la courbe d’enveloppe de volume,
BCBF SOUND TONE ENVELOPE Dresser la courbe d’enveloppe de son.
BCC2 SOUND A ADDRESS Prendre l'adresse d’une courbe d’enveloppe de
volume.
BCC5 SOUND T ADDRESS Prendre l'adresse d’une courbe d’enveloppe de son.
BCC8 KL CHOKE OFF Ramener kernel en arrière,
BCCB KL ROM WALK Quelles extensions- rom?
BCCE KL INIT BACK Ajouter extensions-rom.
BCD1 KL LOG EXT Ajouter extension résidente.
BCD4 KL FIND COMMAND Chercher instruction dans tous les domaines ajoutés
de mé moire.
BCD7 KL NEW FRAME FLY Installer et suspendre bloc d’event.
BCDA KL ADD FRAME FLY Suspendre bloc d’event.
BCDD KL DEL FRAME FLY Sortir bloc d'event.
BCEO KL NEW FAST TICKER Comme BCD7.
BCE3 KL ADD FAST TICKER Comme BCDA.
BCE6 KL DEL FAST TILCKER Comme BCDD.
BCE9 KL ADD TICKER Installer et suspendre bloc ticker.
BCEC KL DEL TICKER Sortir bloc ticker.
BCEF KL INIT EVENT Installer bloc d’event,
BCF2 KL EVENT Expulser le bloc d'event.
BCF5 KL SYNC RESET Effacer Sync Pending Queue.
BCF8 KL DEL SYNCHRONOUS Effacer un certain bloc de la pending queue.
BCFB KL NEXT SYNC Suivant SVP.
BCFE KL DO SYNC Exécuter routine d'event.
BDO1 KL DONE SYNC Routine d’'event prête.
BDO4 KL EVENT DISABLE
BDO7 KL EVENT ENABLE
BDOA KL DISARM EVENT Fermer bloc d’event(compteur négatif).
BDOD KL TIME PLEASE
BD10 KL TIME SET
BD13 MC BOOT PROGRAM Ramène le système d'exploitation en arrière et
transmet la commande à une routine dans (hl).
BD16 MC START PROGRAM
BD19 MC WAIT FLYBACK Attendre le retour du rayon,
BD1C MC SET MODE
BD1F MC SCREEN OFFSET
BD22 MC CLEAR INKS
BD25 MC SET INKS
BD28 MC RESET PRINTER

-92-

BD2B MC PRINT CHAR Imprimer caractère si possible.

BD2E MC BUSY PRINTER Imprimante encore en fonction?

BD31 MC SEND PRINTER Imprimer caractère (attendre que cela marche).
BD34 MC SOUND REGISLTER Fournir des données au Sound Controller.
BD37 JUMP RESTORE Initialiser tous les vecteurs de saut.

Les vecteurs suivants sont utilisés en BASIC.

BD3A EDIT

BD3D FLO Copier variable de (de)=>(h1)
BD4O FLO Int=>Flo

BD43 FLO valeur 4 octets =>F10
BD46 FLO Flo=>Int

BD49 FLO Flo=>Int

BD4C FLO FIX

BD4F FLO INDT

BD52 FLO

BD5S5 FLO Chiffre multiplié par 10°a.
BD58 FLO Addtion

BD5B FLO Soustraction

BDSE FLO Soustraction

BD61 FLO Multiplication

BD64 FLO Division

BD67 FLO Chiffre multiplié par 2°a
BD6A FLO Comparaison

BD6D FLO Modification du caractère initial
BD70 FLO SGN

BD73 FLO DEG/RAD

BD/6 FLO PI

BD79 FLO SGR

BD7C FLO Elévation à la puissance
BD7F FLO LOG

BD82 FLO LOG10

BD85 FLO EXP

BD88 FLO COS

BD&E FLO TAN

BD91 FLO ATN

BD94 FLO valeur 4 octets *256=>F10
BD97 FLO RNDIni

BD9A FLO Set RND Seed

BDSD
BDAO
BDA3
BDA6
BDA9
BDAC
BDAF
BDB2
BDB5
BDB8
BDBB
BDBE
BDC1
BDC4
BDC7
BDCA

FLO
FLO
INT
INT
INT
INT
INT
INT
INT
INT
INT
INT
INT
INT
INT
INT

RND
Prendre dernière valeur-RND.

Recevoir signe initial en b.
Addition

Soustraction

Soustraction
Multiplication avec signe
Division avec signe

MOD

Multiplication sans signe
Division sans signe
Comparaison

Changement de signe

SGN

Ici commencent ce qu’on appelle les indirections. Ce sont des sauts dans
le système d’exploitation qui ne sont pas affectés globalement mais
individuellement par chaque pack, lorsque son RESET ou son INITIALISE est
exécuté.

BDCD
BDDO
BDD3
BDD6
BDD9
BDDC
BDDF
BDE2
BDES
BDE8

TXT
TXT
TXT
TXT
TXT
GRA
GRA
GRA
SCR
SCR

DRAH CURSOR Curseur sur l'écran

UNDRAW CURSOR Curseur éteint

WRITE CHAR Caractère sur l'écran

UNWRITE Lire caractère de l'écran

OUT ACTION Représenter ou exécuter caractère
PLOT fixer un point

TEST point ?

LINE Tracer une ligne

READ Aller chercher point dans l'écran

MODE CLEAR vider écran avec Ink#0

BDEE KM TEST BREAK Touche Break enfoncée ?
BDF1 MC WAIT PRINTER Envoyer caractère à l'imprimante

-94-

2.2 La Ram du système d'exploitation

Vous trouverez ici une liste du système d'exploitation de la Ram, pour
autant que nous ayons réussi à découvrir la signification des différentes
adresses.

Vous ne devez cependant entreprendre de manipulation directe de ces
adresses que si vous savez auparavant quels effets peuvent résulter de
ces manipulations. Vous pouvez constater en effet que toutes les
fonctions importantes du système d'exploitation viennent fureter par ici,
y compris des choses aussi considérables par exemple que la table de saut
du TEXT SCREEN.

Nous comprenons bien sûr, car c'est pour cela que vous avez acheté cet
ouvrage, que vous ayez envie de faire des testes. Donc, allez-y! Mais
n'oubliez pas de sauvegarder auparavant le programme qui se trouve en
mémoire, car il pourrait pâtir de vos essais.

B08B Pointeur de pile Basic

B08D pointeur début des chaînes de caractères
BO8F Pointeur fin des chaînes

BO9A Pointeur de pile du stringdescriptor
BO9C Pile du stringdescr.

BOBA Stringdescriptor

BOC1 Type de variable

BOC2 INTvar / AdrFLOvar / PointSTRdesc
B100 KL Start Int Pending Queue

B104 KL div. flags pour rout. int.
B105 KL sp save

B187 KL Timer low

B189 KL Timer high

B18B KL Timerflag

B18C KL Start Frame Fly Chain

B18E KL Start Fast Ticker Chain

B190 KL Start Ticker Chain

B192 KL Count for Ticker

B193äKL Start Sync Pending Queue

B195 KL Priorité évènement courant
B196 KL Instruction à exécuter

B14A8 KL Rom d'extension actuelle

B1A9 KL Entrée Rom actuelle

B1AB KL Configuration de Rom actuelle

-95-

B1C8 SCR curr. Screen Mode

B1CA SCR Adr. Screen Start

B1CB SCR High Byte Screen Start

B1CC SCR Write Indirection

B1CF SCR Configuration bits suivant le mode
B1D7 SCR Flash Periods

B1D8 SCR Flash Period 1ère couleur

B1D9 SCR Mémoire de couleur 2ème couleur
B1DA SCR Mémoire de couleur 2ème couleur
BIEA SCR Mémoire de couleur première couleur
B1FB SCR Flag jeu de couleur actuel

B1FD SCR curr. Flash Period

B1FE SCR Event Block: Set Inks

B20C TXT fenêtre d'écran actuelle

B20D TXT Start Params Fenêtre 0

B21C TXT Params Fenêtre
B22B TXT Params Fenêtre
B23A TXT Params Fenêtre
B249 TXT Params Fenêtre
B258 TXT Params Fenêtre
B267 TXT Params Fenêtre
B276 TXT Params Fenêtre 7

B285 TXT Position actuelle du curseur (ligne,col)
B287 TXT Flag fenêtre (O=écran entier)

B288 TXT Fenêtre actuelle haut

B289 TXT Fenêtre actuelle gauche

B28A TXT Fenêtre actuelle bas

B28B TXT Fenêtre actuelle droite

B28C TXA Roll Count actuel

B28D TXT act. Cursor Flag

B28E TXT VDU Flag (0-disabled)

B28F TXT Pen actuel

B290 TXT Paper actuel

B291 TXT Background Mode actuel

B293 TXT Graph Char Write Mode (0-disabl)

B294 TXT ler caractère matrice utilisateur

B296 TXT Adr. User Matrix

B2B8 TXT Compteur de caractères Control Buffer
B2B9 TXT Start Control Buffer

B2C3 TXT Table de saut caractères de contrôle

O U1 Æ NN —

-96-

B328
B32A
B32C
B32E
B330
B332
B334
B336
B338
B339
B342
B344
B4DE
BUEO
BUE1
B4E3
B4ES
B4E7
B4E8
BUE9
B4EB
B4ED
B&F1

GRA X Origin

GRA Y Origin

GRA actuelle coord. X

GRA actuelle coord. Ÿ

GRA coord X Fenêtre GRA gauche
GRA coord X Fenêtre GRA droite
GRA coord Ÿ Fenêtre GRA haut
GRA coord Ÿ Fenêtre GRA bas
GRA Pen

GRA Paper

GRA Buffer de calcul coord X
GRA Buffer de calcul coord Y
KM Exp. String Pointer

KM Put Back Buffer

KM Adr. Start Exp Buffer

KM Adr. Fin Exp Buffer

KM Adr. Start Exp Buffer libre
KM Shift Lock State

KM Caps Lock State

KM Delay
KM Key State Map
KM Key 16..,23

KM Joystick 1

BUF4ëKM Joystick 0

B4FS
BUFF
B50D
B541
B543
B545
B547
B551
B552
B555
B55C
B59B
B5DA
B60A
B6FA
B800

KM pendant scanning touches enfoncées
KM Multihit contr. à B4F5

KM Break Event Block

KM Adr, Key Translation Table

KM Adr. Key SHIFT Table

KM Adr, Key CTRL Table

KM Adr, de la table de répétition
SOUND ancienne act. sound (après HOLD)
SOUND actuelle Activité sound

SOUND Sound Event Block

SOUND Params canal A

SOUND Params canal B

SOUND Params canal C

SOUND courbes d'enveloppe de volume
SOUND courbes d'enveloppe de ton

CAS Cass. Message Flag

-97-

B802
B803
B805
B807
B847
B848
B84A
B84C
B8D1
B8DD

CAS Input Buffer Status

CAS Adr. Start Input Buffer
CAS Pointer Input Buffer

CAS File Header Input

CAS Output Buffer Status

CAS Adr. Start Output Buffer
CAS Pointer Output Buffer
CAS File Header Output

CAS Cass. Speed

EDIT Insert Flag

—-98-

2.3 Utilisation des routines du système d'exploitation

Le CPC contient plusieurs centaines de routines ou fonctions dont
certaines sont très utiles et parfaitement utilisables par les
programmeurs. On trouve par exemple de telles routines pour
l'interrogation du clavier, pour sortir un caractère sur l'écran, pour
gérer les fenêtres ou pour commander l'imprimante,

Malgré la masse de fonctions dont dispose le système d'exploitation, il y
a cependant des choses que le CPC ne sait pas faire de lui-même, C’est
ainsi qu’il manque par exemple la possibilité de sortir le contenu de
l'écran, texte ou graphisme sur une imprimante connectée au système.

Cette possibilité appelée ‘Hardcopy’, nous allons vous Ia montrer dans
deux exemples, Dans le premier exemple 11 s'agira d’un hardcopy de texte
uniquement, qui fonctionne avec n'importe quelle imprimante connectée. La
seconde routine de hardcopy permet l'impression de tous les caractères, y
compris les caractères graphiques du CPC. Les images réalisées en
graphisme haute résolution peuvent également être imprimée avec cette
routine, Nous avons choisi comme imprimante la NLQ 401, Cette imprimante
bon marché est, en ce qui concerne son jeu de caractères de commande,
étonnamment compatible avec les imprimantes Epson MX/RX/FX. Les deux
programmes tournent donc également sans adaptation sur des imprimantes
Epson (et sur toutes les autres imprimantes compatibles).

A la fin de ce chapitre, vous ne trouverez pas uniquement deux routines
de hardcopy rapides mais vous aurez également une première approche des
routines du système d'exploitation.

Pour sortir le contenu de l'écran sur une imprimante connectée, 11 faut
faire lire les caractères ligne par ligne sur l'écran et les sortir, Du
fait de la structure spéciale de la Ram vidéo, il n'est malheuresement
pas possible de lire les caractères directement.

A travers le ‘détour’ par une routine du système d'exploitation, il est
cependant possible de déterminer quel caractère se trouve dans
l'emplacement actuel du curseur. Cette routine (TXT RD CHAR, gBB60)
transmet le caractère dans l'’accumulateur et met le flag carry lorsqu'un
caractère a été trouvé. Si par contre aucun caractère du jeu de
caractères du CPC ne figure dans l'emplacement du curseur, alors
l'accumulateur contient O0 et le flag carry est nul.

-99-

Il faut en outre une routine qui nous permette de positionner le curseur,
de façon à ce que nous puissions lire les caractères les uns après les
autres, Cette fonction est exécutée par TXT SET CURSOR, gBB75, Lorsque
cette adresse est appelée, le contenu du registre H est interprété comme
colonne et celui de L comme ligne. L'emplacement d'écriture suprérieur
gauche peut donc être ainsi adressé par 80101,

Il se pose ici cependant une petit problème. Après que nous ayons fait
parcourir toute la surface de l'écran à notre curseur, avec
l'interrogation de l'écran, il faudrait qu’il revienne ensuite dans son
emplacement initial, Il nous faut donc pour cela, avant le premier
positionnement du curseur, déterminer et ranger l'emplacement du curseur.
Cela peut se faire grâce à TXT GET CURSOR, g8BB78. Après avoir appelé TXT
GET CURSOR le double registre HL contient la position actuelle du
curseur. Il nous faut ranger cette valeur et la restaurer à la fin du
hardcopy.

Les caractères obtenus grâce à TXT RD CHAR doivent être sortis sur
l'imprimante. Nous pouvons utiliser à cet effet MC SEND PRINTER dont
l'entrée est en &BD31, Le caractère figurant dans l’accumulateur est
sorti avec sur le port d'imprimante avec tous les signaux handshake
nécessaires.

MC SEND PRINTER attend toutefois que l'imprimante soit prête à recevoir.
C'est MC BUSY PRINTER, &BD2E, qui nous permet de constater si c'est le
cas. Si l'imprimante n'est pas prête à recevoir, si elle n’est pas
allumée ou si elle n’est même pas connectée, MC BUSY PRINTER revient avec
un flag carry mis, Dans ce cas, elle doit être appelée à nouveau, jusqu'à
ce que le flag carry soit supprimé, Le caractère voulu peut alors être
sorti,

I1 peut cependant également arriver qu'un hardcopy une fois lancé ne
doive pas être imprimé Jusqu'au bout, L'opération peut être interrompue
en appuyant sur la touche "DEL’. Mais pour cela, il nous faut pouvoir
examiner si cette touche est enfoncée. Si KM TEST KEY, &BB1E, es appelée
avec une code de touche valable dans l’accumulateur, après exécution de
cette routine, le flag zéro est nul si la touche correspondante est
enfoncée. Sinon le flag zéro est mis.

Ainsi avons-nous en fait toutes les routines système nécessaires pour

-100-

écrire une routine de hardcopy. Mais nous nous rendrons compte au plus
tard lorsque nous aurons commencé à écrire notre programme, que nous ne
savons absolument pas si, au moment du hardcopy, il s'agit de représenter
20, 40 ou 80 caractères par ligne,

Bon, on pourrait décider que ce hardcopy ne fonctionne qu’en mode d'écran
x. Mais ce serait une limitation peu élégante.

SCR GET MODE avec entrée en 8BC11 nous communique avec l'accumulateur et
les deux flags carry et zéro, dans que mode écran le CPC se trouve
actuellement. Nous pouvons ainsi réaliser un hardcopy avec le nombre de
caractères qui convient, en fonction des informations ainsi obtenues.

Mais venons-en maintenant au programme lui-même. Les lecteurs n'ayant pas
d'assembleur peuvent utiliser le programme Basic imprimé à la fin de ce
chapitre, Il contient les deux programmes de hardcopy en lignes de Data.

A100 ORG  +#A100

BB78 GETCRS EQU  +BB78

BB75 SETCRS EQU  +#BB75

BB60 RDCHAR EQU  +#BB60

BD2E TSTPTR EQU  +#BD2E

BD31 PRTCHR EQU  +#B031

BCi1 GETMOD EQU +#BC11

BB1E TSTKEY EQU  #BBIE

A100 CD78BB CALL GETCRS ranger ancienne position curseur
A103 2264A1 LD  {OLDPOS)HL

A106 CD11BC CALL GETMOD schercher mode ecran
A109 17 RLA  : snombre de caracteres/20
A10A 326341 LD (MODEJA Et ranger

A10D 210101 LD HL,#0101 :dans angle supérieur gauche
A110 2266A1 LD  (CRSPOS)HL 1€ Curseur

A113 S3A63A1 LL1 LD  A{MODE)

A116 47 LO BA :1,2 ou 4 fois

A117 0E14 LOOP LD C,20 320 caractères par ligne
A119 C5 LLOOP PUSH BC

ATA ES PUSH HL

A11B CD75BB CALL SETCRS ‘Placer le curseur

AÏE E1 POP HL

A11F CD60BB CALL RDCHAR  :€t déterminer

-101-

A122 Ci POP BC ;:le caractère

A123 3802 JR CGOOD ;caractère valable?
A125 3E20 LD A3 ;Sinon sortir

A127 CD58A1 GOOD  CALL PRTOUT  :espace

A12A ES PUSH HL

A12B C5 PUSH BC

A12C 3E42 LD  A66 :ESC enfoncée?

A12E CDiEBB CALL TSTKEY

A131 C1 POP BC

A132 E1 POP  HL

A133 201C JR NZEXIT ;si oui, fin

A135 24 WEITER INC H

A136 0D DEC C

A197 20E0 JR NZLLOOP :20 caractères imprimés?
A139 10DC DJNZ LOOP ;ligne entière?

A13B 3E0D LD À,#0D ssortir CR/LF

A13D CD58A1 CALL PRTOUT

A140 3E0A LD  A,#0A

A142 CD58A1 CALL PRTOUT

A145 2A66A1 LD  HL(CRSPOS) : déterminer

A148 2C INC L :position curseur
A149 2266A1 LD  (CRSPOS)HL :pour ligne suivante
A14C 7D LD AL

A14D FE1A CP 26 325 lignes imprimées?
A14F 20C2 JR  NZLL1

A151 2A64A1 EXIT LD  HL(OLDPOS);Si oui, restaurer
A154 CD75BB CALL SETCRS sancienne position curseur
A157 C9 RET ; set retour

A158 C5  PRTOUT PUSH BC
A159 CD2EBD PI CALL TSTPR printer busy?

A15C 38FB JR  CPi
A15E CD31BD CALL PRTCHR :sortir un caractère
A161 Ci POP BC

A162 C9 RET

A163 00 MODE  DEFB 0
A164 0000  OLDPOS DEFW 0000
A166 0000  CRSPOS DEFW 0000

Les commentaires dans le listing devraient rendre le programme facilement
compréhensible. La seule particularité est constituée par la méthode de

-102-

calcul du nombre de caractères à sortir par ligne. C’est pourquoi nous
voudrions évoquer cette question brièvement.

Après que nous ayons appelé SCR GET MODE, l’accumulateur contient,
suivant le mode, 0, 1 ou 2, En outre les flags carry et zéro ont les
états suivants:

Mode O = Carry 1, Zero O0
Mode 1 = Carry 0, Zero 1
Mode 2 = Carry 0, Zero 0

L'instruction SLA décale le contenu de l'’accumulateur d’un bit vers la
gauche. Cela correspond à une multiplication par deux, L'état du flag
carry est en outre tranféré dans le bit O0 de l'accumulateur et le bit 7
qui a été ‘expulsé’ est placé dans le carry.

En mode O0, le O qui se trouve dans l'accumulateur subit une rotation.
Cela n’a pas d'influence sur le contenu de l'’accumulateur. Mais comme le
flag carry qui a été mis par SCR GET MODE est transféré dans le bit O de
l'’accumulateur, l’accumulateur contient 1 après cette instruction. Ce 1 a
pour effet que une fois 20 caractères seront imprimés par ligne.

En mode 1, l’accumulateur contient un 1, le carry est nul dans ce mode.
Après SLA, l'accumulateur contient un 2, Ce sont donc deux fois 20
caractères qui seront sortis par ligne. Le fonctionnement est analogue en
mode 2. Le résultat de SLA est un 4 dans l’accumulateur, ce qui entraîne
4 fois 20 caractères par ligne d'impression.

Le principe est quelque peu différent quand il s’agit de produire un
hardcopy graphique. Nous ne pouvons pas alors utiliser les routines TXT
SET CURSOR et TXT RD CHAR.

Tout d’abord, GRA INITIALISE active le mode graphique. Ensuite, avec GRA
GET PAPER nous déterminons le numéro de couleur du fond. Tous les points
de l'écran seront comparés à cette valeur. Si la couleur d'un pixel est
différente de celle du fond, un point sera produit sur le papier.

Malheureusement, le CPC ne dispose que d'une connexion 7 bits avec
l'imprimante. Il en résulte certaines complications.

Cela signifie d'abord que nous pouvons sortir en une fois sur
l'imprimante 7 points placés les uns sous les autres. Le graphisme du CPC
a en tout une résolution graphique verticale de 200 points. Mais divisé
par 7, cela ne donne pas une valeur entière. Il y a donc un reste, c'est-

-103-

à-dire des lignes de pixels qui devront être traitées d'une façon
particulière. Le problème est cependant identique, quel que soit le mode
de texte.

La sortie 7 bits pose un autre problème pour la transmission des
instructions à l'imprimante. L'activation du graphisme avec ESC L
nécessite pour les 640 pixels par ligne une indication qui ne peut être
transmise par le CPC, Pour obtenir le nombre voulu de points graphiques
sur l'imprimante, la séquence de commande pour l'imprimante est:

PRINT _ #8, CHR$ (27): "L":CHR$(128)CHR$ (2)

Le problème vient de la valeur 128, Exprimé en terme binaire, 128 est un
nombre dont le huitième bit (le bit 7) est mis. Tous les autres bits sont
nuls. Si nous envoyions cette Valeur sur l'imprimante, celle-ci ne
recevrait qu'un 0, puisque le huitième bit est utilisé comme strobe et
n'est pas sorti vers l'imprimante.

Nous avons contourné ce problème de façon pas très élégante, en ne
sortant horizontalement que 639 points. C'est certes un point de moins
qu'il n'y en a sur l'écran, mais nous réduisons ainsi la première valeur
à transmettre à 127 (maximum).

Avant que nous n'en venions maintenant au listing du hardcopy graphique,
il nous faut encore relever une particularité,

Bien que l'écran ne représente physiquement que 200 lignes de grille,
toutes les routines graphiques du CPC raisonnent à partir d’une
résolution graphique de 400 points. 11 en résulte un meilleur rapport
entre les directions X et Ÿ que si l’on ne comptait que les deux lignes
véritablement existantes.

La conséquence est facile à observer si vous essayez par exemple le
programme de dessin d’un cercle qui vous est proposé dans le manuel du
CPC, Vous voyez en effet que le cercle est presque rond, Sans cette
correction, c'est une ellipse allongée dans le sens de la largeur qui
serait produite.

Cette correction doit également figurer dans notre hardcopy, mais sous
une forme exactement contraire. Nous devons également déterminer les
coordonnées graphique dans la grille de 4O0x640 points, mais sur
l'imprimante, nous ne sortons que 200 points vertcicalement, pour ne pas
avoir de gaspillages trop importants.

-104-

A000

BBBA GRINIT
BBE7 GETPAP
BBF0 TSTPOI
BD2B PRINTO
BD2E TSTPTR
BBIE TSTKEY
A000 CDBABB

A003 CDE7BB

A006 32BDA0

A009 CD6CAO0

A00C 218F01

A00F 22BEA0

A012 110000
A015 3E07

A017 32C0A0

AO1A CD7CAO LLOOP
A01D 0E00  LLI
AO1F SAC0AO

A022 47

A023 ES BYTLP
A024 DS

A025 C5

A026 CDFOBB

A029 C1
AO2A D1
A02B 21BDA0
AOE BE
AOF E
A030 37
A031 2001
A033 A7
A034 CB11  DOT
A036 2B
A037 2B
A038 10E9
A03SA CDAFAO
A03D 79

ORG
EQU
EQU
EQU
EQU
EQU
EQU
CALL
CALE
LD
CALL
LD

LD
LD
CALL
LD
LD
LD
PUSH
PUSH
PUSH
CALL

POP
POP
LD
CP
POP
SCF
JR
AND

DEC
DEC
DJNZ
CALL
LD

+A000
+BBBA
+BBE7
+BBFO
+BD28
#BD2E
#BB1E
GRINIT
GETPAPER
(PAPER) A
INITP
HL,399

;activer mode graphique
:déterminer couleur fond

sfixer imprimante sur 7/72
nous commençons

(Y-MERK)HL : "impression

DE.0

A7
{ANZAHL),A
PRTESC
C0
A(ANZAHU
BA

HL

DE

BC
TSTPOINT

BC

DE
HL,PAPER
(HU)

HL

NZ,DOT
A

C

HL

HL
BYTLP
TEST
ÂC

zen haut et à gauche

:mais avec malheureusement
:seulement 7 aiguilles
:Séquence ESC pour graphisme
;:C contient modèle bits pour
:l'imprimante

:B=compteur de lignes dot

déterminer couleur du pixel
:d’emplacement (h1/de)

:couleur pixel=couleur fond?

:Si pixel <> paper, alors

smettre flag carry, sinon
annuler flag carry

;décaler carry dans bit inférieur
:du registre C

3HL=HL-2, point suivant,

zet le tout 7 fois

stransférer dans accu

traitement spécial du dernier

-105-

A03E
AO41
A042
A043
A046
A047
A049

CDA6AO
13

E5
217F02
37
ED52
Ei

AO4A 3805
A04C 2ABEAO

AO4F
A051
A052
A053

A055
A056
A059
A05C
AOSE
A05F
A061

A062
A063
A065
A067
A06A
A06C
AOGE
A071

A073
A076
A078
A07B
A07C
A07D
AO7F
A082
AÛ83
A085
A086

18CC
23
7C
B5

2B
110000
22BEA0
3E07
BD
20B9
7C

B4
20B5
3E04
32C0A0
18AE
3E1B
CDAG6AO
3E41
CDAGAO
3E07
CDAG6AO
C9

ES
9E42
CD1EBB
Ei

2802

El

C9

NXTROW

INITP

PRTESC

CALL PRINT

INC DE

PUSH HL

LD HL639
SCF

SBC  HLDE
POP. HL

JR  CNXTROW
LD  HL(Y-MERQ
JR

INC. HL

UD AH

OR L

DEC HL

LD DEO

LD  (Y-MERKHL
LD A7

CP L

JR NZLLOOP
{D AH

OR H

JR  NZLLOOP
LD Ad

LD  (ANZAHL)A
JR  LLOOP
LD A27
CALL PRINT

LD  A65
CALL PRINT

LD A7
CALL PRINT
RET

PUSH HL

LD  A66
CALL TSTKEY
POP. HL

JR  ZNOKEY
POP HL

RET ;

smodèle bits et imprimer

sune ligne imprimée

traitement spécial des
:4 dernières

:préparation de la prochaine
sligne d'impression

sdernière ligne de 7?

:alors plus que 4 lignes

spour NLG/MX/RX/FX
3ESC À 7, pour obtenir
:le bon passage à la ligne

: Touche DEL enfoncée?

:S1 ouf, alors interrompre HC

:DEL n'était pas enfoncée
manipuler pile pour
zatteindre le RET

-106-

A087 : 3E0D
A089 CDAGAO
A08C 3E0A
A08E CDABAO
A091 3E1B
A093 CDAGAO
A096 3E4C
A098 CDAGAO
A09B 3E7F
A09D CDA6AO
AOAO 3E02
AOA2 CDA6AO
A0AS C9
A0A6 CD2EBD
AOA9 38FB
A0AB CD2BBD
AOÂE C9
ADAF 3ACOAO
A0B2 FE07
A0B4 C8
A0B5 AF
AOB6 CB11
A0B8 CB{1
AOBA CB11
A0BC C9
A0BD 00
AOBE 0000
AOC 00

NOKEY LD

CALL
LD

PRINT

TEST

RET
PAPER  DEFB

À,#00
PRINT
A,10
PRINT
A7
PRINT
A76
PRINT
A,127
PRINT
A2
PRINT

TSTPTR
C,PRINT
PRINTOUT

A(ANZAHL)

NDOOXZ>N I

0

Y-MERK DEFW 0000

ANZAHL  DEFB

0

:sortir CR/LF

3ESC L 127 2=graphisme
avec 639 points

: imprimante busy?
:imprimer un caractère
:traitement des 4 dernières

:lignes de dot

sdécaler trois fois 0
;:dans le reg, C
à travers carry

Voici enfin le programme de chargement en Basic que nous vous avons
promis, Ce programme vous permet d'utiliser nos programmes, même si vous
ne disposez pas d’un moniteur ou d’un assembleur.

Entrez d'abord la première partie du programme qui contient les messages
et les commentaires:

100 REM Hardcopy graphique pour le CPC 464 avec NLO/MX/RX/FX
110 REM Le hardcopy doit etre appele avec ‘’CALL &A000'
120 REM Hardcopy de texte pour le CPC 464

-107-

130 REM Le hardcopy doit etre appele avec "CALL &A100
280 1F s<>23767 THEN PRINT'erreur dans hc graphique" :END
290 PRINT“Chargement de hc graphique correct”

390 IF s<>11873 THEN PRINT"erreur dans hc de texte":END
400 PRINT”Chargement de hc de texte correct”

140 FOR i = &a000 TO &aObf

150 READ byte : POKE i,byte : s = s + byte : NEXT
160 DATA &CD,&BA,&BB,&CD,&E7,&BB,8&32,8&BD
165 DATA &A0,&CD,8&6C,8A0,821,88F,&01,822
170 DATA &BE,&A0,&11,800,800,&3E,807,432
175 DATA &C0,8&A0,&CD,&7C,&A0,&0E,800,83A
180 DATA &C0,8A0,847,8E5,&D5,&C5,&CD,&FO0
185 DATA &BB,8&C1,84D1,421,&BD,8A0,&BE,&E1
190 DATA &37,820,801,8A7,&CB,8&11,82B,82B
195 DATA &10,8E9,&CD,&AF,&A0,&79,&CD,&A6
200 DATA &A0,813,8E5,821,87F,802,&37,8ED
205 DATA 852,8E1,838,805,&2A,&BE,8A0,818

-108-

210 DATA &CC,8&23,&7C,&B5,&C8,82B,&11,&00
215 DATA &00,822,8BE,&A0,&3E,8&07,&BD,8&20
220 DATA &B9,8&7C,&B4,820,8B5,83E,&04,832
225 DATA &CO0,8A0,&18,8AE,&3E,&1B,&CD,&A6
230 DATA &AO, &3E,&41,&CD,&A6,&A0,&3E,&07
235 DATA &CD,&A6,&A0,&C9,&E5,83E,&42,&CD
240 DATA &1E,&BB,&E1,828,&02,&E1,&C9,&3E
245 DATA &0D,&CD,&A6,8&A0,83E,&0A,&CD,8&A6
250 DATA &A0,&3E,&1B,&CD,&A6,&A0,83E,&4C
255 DATA &CD,&A6,8&A0,83E,&7F,&CD,&A6,8&A0
260 DATA 83E,8&02,&CD,&A6,8&A0,8&C9,&CD,8&2E
265 DATA &BD,838,&FB,&CD,&2B,&BD,&C9,&3A
270 DATA &C0,&AO0,&FE,8&07,&C8,&AF,&CB,& 11
275 DATA &CB,&11,&CB,&11,&C9,800,&00,8&00

300 FOR i = &a100 TO &a162 : s = 0

310 READ byte : POKE i,byte : s = s + byte : NEXT
320 DATA &CD,&78,&BB,8&22,&64,8&A1,&CD,&11
325 DATA &BC,&17,832,863,8A1,821,801,8&01
330 DATA &22,866,8&A1,8&3A,863,&A1,&47,&0E
335 DATA &14,8C5,&E5,&CD,&75,&BB,&E1,&CD
340 DATA &60,&BB,&C1,&38,&02,&3E,820,&CD
345 DATA &58,8A1,8E5,8&C5,83E,&42,&CD,&1E
350 DATA &BB,&C1,8&E1,8&20,&1C,824,8&0D,8&20
355 DATA &E0,&10,&DC,&3E,&0D,&CD,858,&A1
360 DATA &SE,&0A,&CD,&58,8&A1,8&2A,&66,&A1
365 DATA 82C,822,866,8&A1,&7D,&FE,&1A,820
370 DATA &C2,82A,8&64,8&A1,&CD,&75,&BB,&C9
375 DATA &C5,&CD,&2E,&BD,838,&FB,&CD,&31
380 DATA &BD,&C1,&C9

-109-

2.4 Le traitement des interruptions dans le système d'exploitation

La possibilité la plus rapide et la plus puissante de réagir à,
l'intérieur d'un système d'exploitation à certains évènements est sans
doute la technique des interruptions.

Vous savez certainement ce que c'est. Sinon, voici l'essentiel de ce
qu'on peut dire à ce sujet:

une interruption est en général un évènement d'ordre électronique qui
informe un programme en train de tourner qu'il vient de se produire, En
fonction de cet évènement, le logiciel doit entreprendre des actions
correspondantes et ce le plus vite possible, suivant le niveau d'urgence.
Une telle action sera par exemple le scrolling de l'écran pendant la
phase sombre du rayon électronique, de façon à ce que l’image soit le
plus nette possible.

Cette technique d'interruption présente l'avantage de n’interrompre le
déroulement du reste du programme que lorsqu'il y a vraiment une action à
effectuer, de sorte que le logiciel n'est pas constamment obligé de
contrôler s’il se passe ou non quelque chose.

:l y a naturellement de nombreuses possibilités pour intégrer une telle
fonction dans un système d'exploitation mais nous devons reconnaître que
nous n'avions encore jamais rencontré une variante du type de celle qui
fonctionne sur le CPC.

Il s'agit ici d'un mélange raffiné de hardwareinterrupt (interruption
lorsque nécessaire) et de polling (examen régulier de ce qui se passe).
Le programmeur de la routine correspondante décide du niveau d'urgence
d'une ‘demande’. En clair:

Il n'y a qu’une seule interruption dans la machine, le timer (appelé fast
ticker dans le système), qui produit une interruption tous les 300èmes
de seconde, Tout le reste en découle, comme vous allez voir.

Il est maintenant temps d'introduire quelques concepts que vous
rencontrerez souvent à partir de maintenant, y compris dans le listing de
la Rom.

1. EVENT signifie tout simplement évènement. Comprenez qu’il s’agit d’une
   sorte d'interruption commandée par logiciel.
2. FRAME FLYBACK n'est rien d'autre que le retour déjà évoqué du rayon de
   l'écran, ce qui se produit tous les cinquantièmes de seconde.

3, TICKER est un multiple du fast ticker qui apparaît également tous les
cinquantièmes de seconde.

-111-

Le tout est traité de façon à ce que le programmeur, donc éventuellement
vous-même,. quelles routines de son programme devront être appelées
automatiquement, sans aucune intervention supplémentaire, et avec quelle
fréquence elles devront être appelées au moment frame flyback, ticker ou
même fast ticker. Comme préparation, 11 suffit, outre quelques pétits
détails, de communiquer une fois l'adresse de cette ou de ces routines.
Cette information à préparer s'appelle EVENT BLOCK. Ici est indiqué avec
quelle fréquence et quand la routine doit être appelée, si elle est ou
non prioritaire par rapport à d'autres routines, etc...

À l'entrée du Ticker, Fast Ticker ou Frame Fly, le système d'exploitation
regarde s’il y a des Event-Blocks correspondants. Si oui, 11s sont
appelés, en fonction de leur degré de priorité. Certains Event-Blocks
existent en permanence, comme par exemple l’action qui consiste à
alimenter le registre de couleur au moment Frame Fly.

Les blocs affectés à un évènement déterminé sont également reliés
ensemble par le pointeur, de sorte que le système d'exploitation peut
osciller de l'un à l’autre. Il est donc sans importance de savoir à
quelle adresse figure un tel bloc, tant qu’il se trouve dans les 32K
centraux de la Ram. Cette petite réserve doit être faite car cette zone
est la seule à laquelle il soit possible d'accéder en permanence,
indépendamment de la configuration de la Rom.

Si un tel bloc doit être exécuté, il est rangé dans ce qu’on appelle
Pending Queue. Ce procédé est appelé Kicking.

La Pending Queue est traitée à la fin de la routine d'interruption propre
du système. Vous vous dites certainement qu’un bloc existant doit
naturellement être exécuté. Pourquoi donc faut-il le ranger dans une
queue?

En fait les choses ne sont pas aussi simples car vous avez tout à fait la
possibilité de suspendre le traitement d’un bloc pour un certain temps,
sans que vous ayez à l'extraire de la queue primaire; ceci est d’ailleurs
très facile à réaliser avec les Event-Blocks de la Ticker-Queue.

À propos: ne croyez pas qu’il n'y ait que cette interruption dans
l'ordinateur. Les fanas de l'électronique ont tout à fait la possibilité
de produire une interruption à travers le bus d'extension (asynchron),
mais 11 faut bien sûr qu’il y ait une routine correspondante qui puisse
’kicker’ l'Event-Block’ correspondant.

Devenons plus concret. Que faut-il faire lorsque vous voulez utiliser ce

-112-

méchani sme?

11 faut bien sûr commencer par créer un Event-Block dont la structure est
définie ci-après. La partie suivante est commune à toutes les sortes
d'évènements :

Octet O+1Adresse de chaîne pour la Pending Queue. Ce champ ne doit être

Octet 2

Octet 3

Octet4+5
Octet 6

Octet 7

alimenté que par le système d'exploitation!

Compteur

Tant que le compteur est > 0, le bloc reste dans la Pending
Queue, c'est-à-dire que la routine est exécutée jusqu’à ce qu’il
soit égal à O.

Si le compteur est < 0 (c’est-à-dire >127), le bloc reste dans
la chaîne correspondante (Ticker etc...). Le kicking ne conduit
pas non plus dans ce cas à une exécution de la routine, alors
que cela aurait normalement pour effet d'augmenter le compteur
et donc de provoquer un saut à la prochaine occasion.

Classe

BitO = 1 = L'adresse de saut est une Near Address, c'est-à-dire
qu'elle se trouve dans la Ram centrale ou dans la Rom
inférieure.

BitO = 0 = L'adresse de saut est une Far Address, donc à
rechercher dans la Rom supérieure.

Les bits 1-4 déterminent la priorité.

Bit 5 doit toujours être =0!

Bit6 = 1 = Express. Les Express-Events ont une priorité
supérieure à celles des évènements normaux de la plus grande
priorité.

Bit? = 1 = Asynchron Event. Ces évènements n'ont pas de file
d'attente et 1ls sont rangés immédiatement dans 1l’Interrupt
Pending Queue lors du Kicking (KL EVENT), S’il s'agit même d’un
express, cette routine est exécutée Immédiatement, sinon
seulement à la fin de la routine d'interruption.

Attention: la routine pour les évènements asynchrones doit
absolument se trouver dans la Ram centrale!

Adresse de la routine

Rom Select, si l'adresse de saut est du type Far, sinon
inutilisé.

Ici commence le champ de l'utilisateur qui peut être aussi long
que souhaité. I1 peut servir à la transmission de paramètres à
la routine. Lors de l'appel d'une Event-Routine hl contient
l'adresse de l'octet 5 de l’Event Block, s'il s'agit d'une Near

-113-

Address, sinon l'adresse de l’octet 6.

Ceci permet de créer plusieurs blocs pour une même routine qui
peut déterminer, en fonction des paramètres, par quel bloc elle
a été appelée.

Suivant le type de l'évènement, Ticker, Fast Ticker ou Frame Fly, deux ou

six octets sont encore placés avant la partie commune. Dans le tas de

Fast Ticker et Frame Fly, ce ne sont que deux octets pour le chafnage (ne

pas les modifier!) dans la Fast Ticker List ou la Frame Fly List.

Les six octets pour le Ticker ont la signification suivante:

Octet 0+1 Chafnage pour Ticker List (ne pas modifier!)

Octet 2+3 Tick Count détermine combien de fois un Ticker doit apparaître,
avant que le bloc ne soit kické une fois.

Octet 4+5 Reload Count indique quelle valeur doit être chargée dans le
Tick Count après son écoulement.

Après donc que vous ayez alimenté votre bloc avec ces valeurs, pour
autant que vous les connaissiez, (ce devraient être les 5 derniers octets
(Event Count=0) de la partie commune et, pour le ticker, également les
compteurs), vous n'avez plus qu’à charger l'adresse de début de votre
bloc dans h1 puis, suivant le cas, à appeler la routine KL ADD TICKER, KL
ADD FAST TICKER ou KL ADD FRAME FLY.

Pour extraire le bloc de la liste, utilisez les routines KL DEL TICKER,
etc... hl devant cette fois également contenir l'adresse du bloc à
éloigner.

Essayez et observez comment le système d'exploitation procède, car les

procédures qui reviennent sans cesse sont également traitées à travers le
mécanisme des évènements.

-114-

2.5 Le listing de la Rom du système d'exploitation

Nous nous sommes donnés le plus grand mal pour que vous puissiez utiliser
le plus aisément possible ce listing de la Rom, mais il reste encore des
blancs sur notre carte d'état-major, essentiellement d'ailleurs 1à, où il
ne s’agit pas de la structure du système en tant que telle, mais où
certaines fonctions particulières sont exécutées. Il s'agit par exemple
du CASSETTE MANAGER, du GRAPHICS MANAGER et du SOUND MANAGER. De tels
programmes sont naturellement difficiles à interpréter car il est
impossible de reconstituer le processus de pensée de chaque programmeur.
Mais nous pensons que cela ne devrait pas vous gêner dans l’utilisation
des routines,

Vous trouverez dans l'introduction à chaque pack des indications pour
appeler certaines sections de programme souvent utilisées avec les
paramètres à transmettre.

Les paramètres à transmettre de toutes routines dotées de vecteurs,
qu'elles soient utilisables ou non figurent dans le Schneider Firmware
Manual. C'est de la version anglaise de ce manuel que nous avons tiré les
noms des packs que nous n'avons pas traduit pour éviter toute confusion
dans l'esprit des lecteurs possédant ce manuel,

2.5.1 KERNEL (KL)

Le Kernel, comme son nom l'indique est le noyau du système
d'exploitation.

C'est ainsi qu’il est responsable de la commande du déroulement des
programmes, c’est-à-dire pour le traitement des interruptions ainsi que
des Events, le traitement des Restarts, la mise en place d'extensions de
la Rom et la commutation de la configuration de la mémoire.

Les routines liées au mécanisme des Events sont éventuellement
utilisables, Voyez à ce sujet le chapitre 2.4.

-115-

KERNEL

0000 01897F ld bc,7fB9 U Rom dis., Mode 1, res diviseur
0003 ED49 out (c),c

0005 C38005 jp 0580 RESET CONT'D

0008 C382B9 jp B982 (0413) RST 1 LOW JUMP CONT'D
000B C37CB9 jp B97C (O4OD) KL LOW PCHL CONT'D

OO0E C5 push bc

000F C9 ret jp (bc)

0010 C316BA jp BA16 (04A7) RST 2 LOW SIDE CALL CONT'D
0013 C310BA jp BA10 (O4A1) KL SIDE PCHL CONT'D

0016 DS push de

0017 C9 ret Jp (de)

0018 C3BFB9 jp B9BF (0450) RST 3 LOW FAR CALL CONT'D
001B C3B1B9 Jp B9B1 (0442) KL FAR PCHL CONT'D

O01E E9 Jp (h1)

001F 00 nop

0020 C3CBBA Jp BACB (055C) RST 4 RAM LAM CONT'D

0023 C3B9B9 Jp B9B9 (O44A) KL FAR ICALL CONT'D

0026 06 nop

0027 00 nop

0028 C32EBA Jp BA2E (O4BF) RST 5 FIRM JUMP CONT’D
002B 00 nop

002C ED49 out (c),c

002€ D9 exx

002F FB ei

DEN N NME DÉ DU DHEA NE UE DD AUDE DU DE DE OU DU DU DE DE DD DE DE DE D DU DE DE DE D DEN NE DE RST 6 USERO

0030 F3 di RSTO après High Kernel Restore

-116-

KERNEL

0031 D9 exx

0032 212B00 id h1,002B

0035 71 id (h1),c

0036 1808 Jr 0040

0038 C339B9 Jp B939 (03CA) RST 7 INTERRUPT ENTRY CONT'D
003B C9 ret EXT INTERRUPT

003C 00 nop

003D 00 nop

003E 00 nop

003F 00 nop

LÉELILILLLLLILLELLILLILLLLELLLLLLLLLL) Jusqu'ici copié dans la Ram
0040 CBD1 set  2,c L Rom disable

0042 18E8 Jr 002€
LLLILLILLLLLLLLLILLLLLLILILILLLLLLLILILLLELLLLLZLLEL)] Restore High Kernel Jumps
0044 214000 ld h1,0040 003f

0047 2D dec l! à

0048 7E ld a, (hl) 0000

0049 77 id (h1),a copier dans

O0O4A 20FB Jr nz,0047 la Ram

004C 3EC7 ld a, C7 RST O dans

OO4E 323000 ld (0030), a 0030

0051 219103 ld h1,0391 Jump

0054 1100B9 ld de,B900 (0391) Copier

0057 01E901 Id bc,01E9 bloc

005A EDBO ldir

LES LR LS LL LL LL LL LL LL LL LL LL LL LL LL LL SL LL: 2 22222] KL CHOKE OFF

005C F3 di

005D 3AABB1 ld a, (B1AB) (config. Rom act.)
0060 EDSBA9B1 ld de, (B1A9) (Entrée Rom act.)
0064  06C0 ld b, CO Firmware-

0066 2100B1 id h1,B100 Ram

0069 3600 ld (h1),00

006B 23 inc  hl Supprimer

006C 10FB dinz 0069 Jusqu'à B1C0

006E 47 ld b,a

006F  OEFF ld c,FF

-117-

0071
0072
0073
0074
0075
0076

0077
0078
0079
007A
007C
007D
0080
0083
0086
0089
008€
008F
0092
0095
0096
0098

A9
co
4F
5F
57
c9

7C

B5

79
2004
7D
2106C0
32A8B1
32ABB1
22A9B1
21FFAB
114000
01FFBO
3100C0
DF
A9B1
C7

KERNEL

Xor € Y avait-il une Rom active?
ret nz oui >
1d c,a

ld e,a

1d d,a

ret

ld a,h

or l

ld a,C

Jr nz, 0080

ld a, 1 si h1=0

ld h1,C006 Chargement par défaut
ld (B148),a (Rom ext. act.)

ld (B1AB),a (config. Rom act.)

ld (B1A9),h1 (Entrée Rom act.)

ld h1, ABFF Charger params pour
ld de, 0040 RST3

1d bc, BOFF

ld sp, CO00

rst 3 FAR CALL
dw B1A9

rst O0

LL SL RS LS LS SL LL LS LS LL LS LL EL SL LL LL LL SL ELLE TS 22] KL TIME PLEASE

0099
0094
O0SE
O0A1
OOA2

F3
ED5B89B1
2A87B1
FB

C9

di

ld de,(B189)  (Timer high)
ld h1,(B187)  (Timer low)
ei

ret

DEMANDE DD UE AUDE DU UD UE D QU 98 98 De DE D D DU D DU D UD AUDE D OO OU 4 EE 2 KL TIME SET

O0A3
OOA4
OOAS
O0A8
O0AC
OOAF
00BO

F3

AF
328BB1
ED5389B1
2287B1
FB

C9

di

xor a

ld (B18B),a (Timerflag)

ld (B189),de  (Timer high)
ld (B187),h1  (Timer low)

ei

ret

LL is LS SL LL LS LL SL LL SSL SL LL LS LL LL SL ELLES 2 2 222 Scan Events

-118-

00B1
O0B4
COB5
00B6
00B8
OOBA
O0BC
O0BD
OOBF
00C2
O0C3
o0c4
00C7
OOCA
OOCB
O0CC
OOCF
00D2
00D5
00D6
00D7
00D9
OODC
OODF
O0EO
00€ 1
00E2
O0E5
OOE7

00E8
O0E9
O0EB
OOEC
OOEF
00F0
00F2
00F5
00F8
OOFB

2187B1
34

23
28FC
06F5
ED78
1F
3008

ld
inc
inc

KERNEL

h1,B187
(h1)

hl
z,00B4
b,F5

a, (c)

nc, 00C7
h1, (B18C)
a,h

a
nz,0153
h1, (B18E)
a,h

h1, (B190)
a,h

a

Z
hl1,B104
0, (h1)

hl
(h1),00
hl

a, (B101)
a
nz,OOFE
(B100),h1
(B102),h1
h1,B104
6, (h1)

Timer lon
update
Timer

Port B

VSYNC ?

Non >

(Start Frame Fly Chain)

Kick Event
(Start Fast Ticker Chain)

Kick Event
Scan Sound Queues
Count for Ticker’

Update Key State Map
(Start Ticker Chain)

div. flags pour rout. int.
Ticker Chain doit encore
être traité

(Start Int Pending Queue)

div. flags pour rout. int.

-119-

00FD

OOFE
0102
0105
0106
0107
0108
0109

0104
010€
0111
0112
0113
0114
0117
0119
011B
011D
0120
0121
0122
0124
0125
0126
0127
012B
012C
012F
0130
0132
0135
0137
0139
013B
013C
013D
0140
0141

C9

ED5B02B]
2202B1

3187B1

bit

call

KERNEL

de, (B102)
(B102),h1
de,hl
(hl).e

hl

(h1),d

(B105),sp (sp save)
sp,B187 Timer 10w

h1,B104 div. flags pour rout. int,

7, (h1)

h1,(B100) (Start Int Pending Queue)
a,h

a

z,0132

e,(hl)

hl

d,(h1)

(B100),de (Start Int Pending Queue)
hl

0204

011D

h1,B104 div. flags pour rout. int.
0, (h1) Ticker Queue pending ?
z,0149 non

(h1),00

af,af

0189 traiter Ticker Chain
a

af,af'

-120-

KERNEL

0142 2104B1 ld h1,B104 div. flags pour rout. int.
0145 7E ld a,(hl)

0146 B7 or a encore quelque chose à traiter?
0147 20D2 Jr nz,011B oui

0149 3600 ld (h1),00 supprimer tous les flags
O14B C1 pop bc

014C D1 pop de

014D E1 pop hl

O14E ED7B05B1 ld sp,(B105) recharger sp

0152 C9 ret

LALLLL LL ELLE LIL SLI LILI LL LS LL LLLLLLL LL LLL LE LLLLLLZL) Kick Event

0153 5€ ld e,(hl)

0154 23 inc hl

0155 7E ld a,(hl)

0156 23 inc hl

0157 B7 or a

0158 CAE201 Jp Z,01E2 KL EVENT

015B 57 ld d,a

015C D5 push de

015D CDE201 call 01E2 KL EVENT

0160 E1 pop hl

0161 18F0 Jr 0153 Kick Event

LALLLLLLLLEL ESS LLLLLLLLLLLLILLLLLELLLELLLLSLLLLL LL: KL NEW FRAME FLY
0163 ES push hl

0164 23 inc hl

0165 23 inc  hl

0166 CDD201 call 01D2 KL INIT EVENT

0169 E1 pop hl

LELLSLL SELS LE SLI S SIL ES ELLE LLLIL LI LIL L LS ILES LIL LL SL) KL ADD FRAME FLY
016A 118CB1 ld de,B18C Start Frame Fly Chain
016D C37303 Jp 0373 Add Event

0170 118CB1 ld de, B18C Start Frame Fly Chain
0173 C38203 Jp 0382 Delete Event

LL LL LL LL LL LS LES ESS SL LS LL LL LL ELLE LL LES SLELLELE ESS EE) KL NEW FAST TICKER

0176 E5 push hl

-121-

KERNEL

0177 23 inc hl

0178 23 inc hl

0179 CDD201 call 01D2 KL INIT EVENT

017C E1 pop hl
CILILLLILLLLLLLLLLLLLLLILLLLILLLLLLLLLLLLLLLILZL] KL ADD FAST TICKERO
017D 118EB1 ld de,B18E Start Fast Ticker Chain
0180 C37303 Jp 0373 Add Event

LILL LIL LLLLLLLLLLLLLLLLLLLLL LL LL LLLLLLLLLLLS LEZ Delete Fast Ticker
0183 118EB1 ld de,B18E Start Fast Ticker Chain
0186 C38203 Jp 0382 Delete Event

LILLLLI LL LLILLLLLLILLLLLLLLILLLLLLL LL LL LL LILL LL] Ticker Chain bearbeiten
0189 2A90B1 id h1,(B190) (Start Ticker Chain)
018C 7C ld a,h

018D B7 or a

018€ C8 ret 2z

018F SE ld e,(hl)

0190 23 inc hl

0191 56 ld d,(h1)

0192 23 inc hl

0193 4E ld c,(hl)

0194 23 inc hl

0195 46 ld b, (h1)

01% 78 id a,b

0197 B1 or C

0198 2816 Jr z,01B0

019A OB dec bc

019B 78 id a,b

019C B1 or c

019D 200€ Jr nz,01AD

019F DS push de

01A0 23 inc hl

01A1 23 inc hl

01A2 E5 push hl

0143 23 inc hl

OA4 CDE201 call 01E2 KL EVENT

0147 E1 pop hl

0148 46 ld b, (h1)

-122-

KERNEL

0149 2B dec hl

OTAA 4E ld c,(hl)

O1AB 2B dec hl

O1AC D1 pop de

01AD 70 ld (h1),b

O1AE 2B dec hl

O1AF 71 ld (h1),c

01B0 EB ex de,hl

01B1 18D9 Jr 018C

LILLLLLLILLILL LILI LLLLLLLLLL LL LL LLLLLLLLLLLLLLLEL EL) KL ADD TICKER
01B3 ES push hl

01B4 23 inc hl

01B5 23 inc hl

01B6 F3 di

01B7 73 ld (hl)e

01B8 23 inc hl

01B9 72 ld (h1),d

O1BA 23 inc hl

01BB 71 ld (h1),c

01BC 23 inc  hl

01BD 70 ld (h1),b

O1BE E1 pop hl

01BF 1190B1 ld de,B190 Start Ticker Chain
01C2 C37303 jp 0373 Add Event
LILLLLLLLLLLL LL LL LL LL LL LL LLLLLLLLLLLLLLLLLLEL LEZ 2 Delete Ticker
01C5 1190B1 ld de,B190 Start Ticker Chain
01C8 CD8203 call 0382 Delete Event

01CB DO ret nc

01CC EB ex de,hl

01CD 23 inc  hl

01CE SE ld e,(hl)

O1CF 23 inc  hl

01D0 56 id d,(h1)

01D1 C9 ret

LAS LE LES LIL LS EL LL SES SL LE LL ILES LES LLLE LL 2 XXE ZX 2 KL INIT EVENT

01D2 F3 di
01D3 23 inc hl

-123-

01D4
01D5
01D7
01D8
01D9
01DA
01DB
01DC
O1DD
O1DE
O1DF
01E0
01E1

KERNEL

hl
(h1),00

LALLLLLLLLLLL SE LL LL LL LL LL LL LL LL LLLLLLLLLELE 2222222 KL EVENT

01E2
01E3
01E4
O1ES
01E6
O1E7
O1EA
O1EB
O1ED
O1EE
O1EF
01F0
01F1
01F4
01F5
O1F7
01F8
01F9
01FC
O1FD
O1FE
0200
0201
0203
0204

inc
inc
di
ld
inc
Jp
or
Jr
inc
id
dec
or
jp
ex
jr
ex
add
jp
inc
inc
Jr
ex
jr
ei
ex

hl
hl

a,(hl)
(h1)

m, 0206
a

nz, 0200
hl
a,(hl)
hl

a
p,022F
af,af'
nc, 0209
af,af’

Event Cnt >127/<0

Event Cnt >0 & <127

ajouter Sync Event

-124-

KERNEL

0205 C9 ret

0206 35 dec (hl)

0207 18F7 jr 0200

0209 08 ex af,af’

0204 FB ei

020B 7E id a,(hl)

020C B7 or a

020D F8 ret m

020E E5 push hl

020F CD1C02 call 021€

0212 E1 pop hl

0213 35 dec (hl)

0214 C8 ret 2z

0215 F20E02 Jp p,020E

0218 34 inc  (hl)

0219 C9 ret

UTTTTTIILILILLLILILILSLILLLLS LL LILI LILI LILI LL LL 2) KL DO SYNC
0214 23 inc hl

021B 23 inc hl

021C 23 inc hl

021D 7E ld a,(h1l)

O21E 23 inc  hl

021F 1F rra

0220 D2B9B9 Jp nc,B9B9 (O44A) KL FAR ICALL CONT'D
0223 5E ld e,(hl)

0224 23 inc hl

0225 56 ld d,(h1)

0226 EB ex de,h1

0227 E9 Jp (h1)

UTTTTITIILIIIILLLL LI LIL LLLLIL LIL LL LL LL Li iii l)LL::] KL SYNC RESET
0228 210000 1d h1, 0000

022B 2294B1 id (B194),h1

022€ C9 ret

UTIIIIIILLLLLLLLSILLLL LL LLLL I SSL LL LLLLLLL LEE ES 2] Aj outer Sync Event
022F ES push hl

0230 47 id b,a Priorité => b

-125-

0231
0234
0235
0236
0237
0238
0239
023A
023B
023C
023E
023F
0240
0241
0242
0243
0245
0246
0247
0248
0249
O24A
O24B
O24C
024)
O24E
O24F
0250
0251
0253
0254
0255

1196B1

KERNEL

de,B196
de,hl

d,(hl) Adr, prochain
hl Event ‘Block
e, (hl) de

de

a, (de) Prioritat act. >
b priorité trouvée?
nc, 0234 non

LILI LIL LI LL LILI LL LL LL ELLE LI LL LL LL LL LL LL ELLES, EE] KL NEXT SYNC

0256
0257
025A
025B
025C
025E

di
ld
ld
or
Jr
push

h1,(B193) (Start Sync Pending Queue)
a,h

a

z,0275

hl

-126-

KERNEL

025F 5E id e,(h1)

0260 23 inc hl

0261 56 ld d,(hl)

0262 23 inc hl

0263 23 inc hl

0264 3A95B1 ld a, (B195) (Priorité Event act.)
0267 BE Cp (h1)

0268 3004 jr nc. 0274

0264 F5 push af

026B 7E ld a,(h1)

026C 3295B1 id (B195),a (Priorité Event act.)
026F ED5393B1 ld (B193),de (Start Sync Pending Queue)
0273 F1 pop af

0274 E1 pop hl

0275 FB ei

0276 C9 ret

ETTTTTT TITI IT ITIIILILILILI LIL LILI SL LL LL LL LL LS SL EE 2 2] KL DONE SYNC
0277 3295B1 ld (B195),a (Priorité Event act.)
027A 23 inc hl

027B 23 inc hl

027C 35 dec (hl)

027D C8 ret 2z

027E F3 di

027F F22F02 jp p,022F Ajouter Sync Event

0282 34 inc  (hl)

0283 FB ei

0284 C9 ret

VTTTTITTITIIIILILLLILLIILI LILI I LL SL LL LL LL SL LE) KL DEL SYNCHRONOUS
0285 CD8E02 call 028 KL DISARM EVENT

0288 1193B1 id de,B193 Start Sync Pending Queue
028B C38203 jp 0382 Delete Event

EVTT TT TT TTT TITI III I IILILILILI III LILI LL I LS LS 22 2 22 2 22] KL DISARM EVENT
028€ 23 inc hl

028F 23 inc hl

0290 36C0 ld (h1),C0

0292 2B dec hl

0293 2B dec hl

-127-

KERNEL

0294 C9 ret

LRLILILLLLLLLL LL LILI LLLLILLLLLILL LL LL LL LLLILLLEL LE] KL EVENT DISABLE
0295 2195B1 ld h1,B195 Priorité Event act.
0298 CBEE set 5,(hl)

029A C9 ret

CTLLLILILLLLLLLLLLLLLLLLLLLLL IL LLLLLLLILLILLLLLLZ) KL EVENT ENABLE
029B 2195B1 id h1,B195 Priorité Event act.
029,  CBAE res 5,(h1)

02A0 C9 ret

LITIILILLILLLLLLLLLLLILLLILL LIL LL LLILL LILI ILLLLL:] KL LOG EXT
O2A1 ES push hl

02A2 EDSBA6B1 ld de, (B1A6)

02A6 22A6B1 id (B1A6),h1

02A9 73 id (hl),e

O2AA 23 inc  h]l

02AB 72 ld (h1),d

O2AC 23 inc hl

O2AD 71 id (h1),c

O2AE 23 inc h!

O2AF 70 id (h1),b

02B0 E1 pop hl

02B1 C9 ret

LRLLLLLLLLL LL TL LLLL IL IL LLLL IL LL LL LL LL LLLLLLLEL LE] KL FIND COMMAND
02B2 1196B1 ld de,B19%6 instruction à exécuter
02B5 011000 ld bc,0010

02B8 CDAG6BA call BAA6 (0537) Rom off & save config
02BB EB ex de,hl

02BC 2B dec hi

02BD CBFE set 7,(h1)

02BF  2AA6B1 id h1, (B1A6)

02C2 7D id a, 1

02C3 1810 Jr 02D5

02C5 ES push hl

02C6 23 inc hl

0207 23 inc hl

02C8 4E ld c,(hl)

-128-

F1
C30B06

2104C0

KERNEL

inc hl

id b, (h1)
call O2F4
pop de

ret c

ex de,h]l
ld a, (h1l)
inc  hl

id h, (h1)
ld La

or h

Jr nz,02C5
id c,FF
inc c

call BA83 (0514) KL PROBE ROM CONT'D
push af

and 03

ld b,a
call z,02F4
Jr c,02F0
pop af

add  a,a

jr nc,02DA
id a,c

or a

jr Z, 02DA
ret

pop af

JP 060B MC START PROGRAM
ld h1,C004
ld a,b

or a

Jr Z,02FF
ld h,b

ld 1,c

id c,FF

call BA7E (050F) KL ROM SELECT CONT'D
push bc

-129-

B7
20E5
C1
C38CBA

id
inc
ld
inc
ex
Jr
ld
id
cp
Jr
inc
inc
add
Jr
ex
jr
id
inc
add
Jr
inc
inc
inc
id
or
ir
pop
Jp

KERNEL

e,(h1)
hl
d,(hl)
hl
de,hl
0321
bc,B196
a, (bc)
(h1)
nz,0319
hl

bc

a,a

nc, 030D
de,h]l
0325

a, (h1)
hl

a,a

nc, 0319
de

de

de

a, (h1)
a
nz,030A
bc

instruction à exécuter

BA8C (051D) KL ROM DESELECT CONT'D

LAS SL LL SSL LL LE LL LS LL LLLL SELLE L LL EL EE LS ZLLILLL LL) KL ROM WALK

0329
032B
032E
032F
0331

0E07
CD3203
OD
20FA
C9

ld
call
dec
Jr
ret

c,07
0332

C
nz,032B

KL INIT BACK

CLS LS LR DLL LL LL LL LL LIL LL LL ELLE LL RE LL LL LL LL LL LL LE) KL INIT BACK

0332
0333
0335

79
FE08
DO

ld
cp
ret

a,c
08
nc

-130-

KERNEL

0336 CD7EBA call BA7E (050F) KL ROM SELECT CONT'D
0339 3A00C0 id a, (C000)

033C E603 and 03

033E 3D dec a

033F 201F Jr nz,0360

0341 C5 push bc

0342 CDO6CO call CO06

0345 D5 push de

0346 23 inc hl

0347 EB ex de,h}

0348 21AAB1 ld h1,B1AA

034B ED4BA8B1 ld bc,(B1A8) (Rom. ext. act.)
034F 0600 id b, 00

0351 09 add hl,bc

0352 09 add hl,bc

0353 73 ld (hl),e

0354 23 inc hl

0355 72 ld (h1),d

0356 21FCFF id h1,FFFC

0359 19 add hl,de

035A CDA102 call O2A1 KL LOG EXT
035D 2B dec hl

035E D1 pop de

035F C1 pop bc

0360 C38CBA Jp BA8C (051D) KL ROM DESELECT CONT'D
0363 7E ld a, (hl)

0364 BB cp e

0365 23 inc hl

0366 7E id a, (hl)

0367 2B dec hi

0368 2003 Jr nz,036D

036A BA cp d

036B 37 scf

036C C8 ret 2z

036D B7 or a

036E C8 ret 2z

036F 6E id 1,(h1)

0370 67 id h,a

0371 18F0 Jr 0363

-131-

KERNEL

LELLLLLLL SE LLL IL LL SELLES LL SL ELLLLLLLLELL EL ELLES) 2) hdd Event

0373 EB ex de,hl

0374 F3 di

0375 CD6303 call 0363

0378 3806 jr c,0380

037A 73 id (hl1).e

037B 23 inc hl

037C 72 ld (hl),d

037D 13 inc de

037E AF xor a

037F 12 ld (de),a

0380 FB ei

0381 C9 ret
ELILELILLILILLLLLLLLILLLLLLILIIELILLILLLLLLLLLLLLEZEZ EL 2 Delete Event
0382 EB ex de,hl

0383 F3 di

0384 CD6303 call 0363

0387 3006 Jr nc,038F

0389 1A ld a, (de)

038A 77 ld (hl1),a

038B 13 inc de

038C 23 inc hl

038D 1A ld a, (de)

038E 77 id (h1),a

038F FB ei

0390 C9 ret

0391 C35EBA JP BASE (O4EF) KL U ROM ENABLE CONT'D
0394 C368BA Jp BA68 (O4F9) KL U ROM DISABLE CONT'D
0397 C34ABA Jp BAUA (O4DB) KL L ROM ENABLE CONT'D
039A C354BA jp BAS4 (QUES) KL L ROM DISABLE CONT'D
039D C372BA Jp BA72 (0503) KL ROM RESTORE CONT'D
03A0 C37EBA jp BA7E (050F) KL ROM SELECT CONT'D

-132-

KERNEL

O3A3 C3A2BA Jp BAA2 (0533) KL CURR SELECTION CONT'D
03A6 C383BA jp BA83 (0514) KL PROBE ROM CONT'D

O3A9 C38CBA Jp BA8C (051D) KL ROM DESELECT CONT’D
O3AC C3AG6BA Jp BAA6 (0537) KL LDIR CONT'D

O3AF C3ACBA Jp BAAC (053D) KL LDDR CONT'D

LLLLLÉELLLLELLLLL LES LL LL LL LL LLLLLLLLLLLLLLL LL EL): KL POLL SYCHRONOUS

03B2 3A94B1 ld a, (B194)

03B5 B7 or a

03B6 C8 ret 7Zz

03B7 ES push hl

03B8 F3 di

03B9 2A93B1 ld h1,(B193) (Start Sync Pending Queue)
03BC 7C ld a,h

03BD B7 or a

O3BE 2807 jr z,03C7

03C0 23 inc hl

03C1 23 inc hl

03C2 23 inc hl

03C3 3A95B1 1d a, (B195) (Priorité Event act.)
03C6 BE CP (h1)

03C7 E1 pop hl

03C8 FB ei

03C9 cg ret

LÉLSS SL LES SSL ELLE LL LL LL LL LL LL LL LL LL LL.) RST 7 INTERRUPT ENTRY CONT'D
O3CA F3 di

03CB 08 ex af,af'

03CC 3833 jr c,0401 EXT INTERRUPT ENTRY
O3CE D9 exx

O3CF 79 ld a,C

03D0 37 scf

03D1 FB ei

03D2 08 ex af,af'

03D3 F3 di

-133-

KERNEL

03D4 F5 push af

03D5 CB91 res  2,C

03D7 ED49 out  (c),c L Rom enable
03D9 CDB100 call 00B1 Scan Events
O3DC B7 or a

03DD 08 ex af,af'

O3DE 4F ld c,a

03DF 067F ld b,7F

03E1 3A04B1 ld a, (B104) (div. flags pour rout, int.)
O3E4 B7 or a

03E5 2814 Jr Z,03FB

03E7 FAG6AB9 jp m,B96A (03FB)

O3EA 79 1d a,C

O3EB E60C and UC

O3ED F5 push af

O3EE CB91 res  2,C

03F0 D9 exx

03F1 CDOA01 call 0104

03F4 D9 exx

03F5 Eî pop hl

03F6 79 ld a,C

03F7 E6F3 and F3

03F9 B4 or h

O3FA 4F ld c,a

03FB ED49 out (c),c fixer ancienne config.
03FD D9 exx

O3FE F1 pop af

O3FF FB ei

0400 C9 ret

HUE HD HE DD EDEN DEEE EDEN DM DE DEEE EME EXT INTERRUPT ENTRY

0401 08 ex af,af'

0402 E1 pop h1

0403 F5 push af

0404 CBD1 set 2,c

0406 ED49 out (c),c L Rom disable
0408 CD3B00 call 003B

040B 18CF jr 03DC

LÉSL ELLE LES SES S SL SES SLL SSL LLELLEL LL SSL LLLLLLES SZ; KL LOW PCHL CONT'D

-134-

KERNEL

O40D F3 di

OU0E ES push hl

O40F D9 exx

0410 D1 pop de

0411 1806 Jr 0419

LAIT LLI LIL LILI L LILI LT LI LILI LILI LLLLLLLLLILLLS SSL: RST 1 LOW JUMP CONT'D
0413 F3 di

0414 D9 exx

0415 E1 pop hl

0416 5€ id e,(h1)

0417 23 inc hl

0418 56 ld d,(h1)

0419 08 ex af,af'

O41A 7A ld a,d

041B CBBA res 7,d

O41D CBB2 res 6,d

041F 07 rlca

0420 07 rlca

0421 07 rlca

0422 07 rlca

0423 A9 XOr cc

0424 E60C and OC

0426 A9 xor €

0427 C5 push bc

0428 CDA8B9 call B9A8 (0439) préparer config. & exécuter saut
042B F3 di

O42C D9 exx

O42D 08 ex af,af'

O42E 79 id a,C

O42F Ci pop bc

0430 E603 and 03

0432 CB89 res 1,c

0434 CB81 res  O,c

0436 B1 or C

0437 1801 Jr O43A

0439 DS push de adr, de saut sur pile
O43A 4F ld c,a

043B ED49 out (c),c fixer config Rom
043D B7 or a

-135-

KERNEL

OU3E 08 ex af,af'

043F D9 exx

0440 FB ei

Oùu41 C9 ret exécuter saut

PERDUE DE DE DDR DEN UEEDEDEDUDÉUE DEEEUEUEEDUEEEUEEUEUEOUCUE UE DE DE DEEE KL FAR PCHL CONT'D

0442 F3 di

0443 08 ex af,af'’
ouu4 79 ld a,c
O445 ES push hl
0446 D9 exx

Où447 Di pop de
0448 1815 Jr O45F

LELLS SL LL LLLLL LS LIL LL ILE LLLL LL LL LL LL EL LILI LL: LL] KL FAR ICALL CONT D

OUUA F3 di

O4yB ES push hl

O4uC D9 exx

Où4D E1 pop h]l

O4UE 1809 jr 0459
LELLLLLLLLLLILILLLLLILELLLLLELLLLLLLLLLLELLLLZ ZE ZX) RST 3 LOW FAR CALL CONT'D
0450 F3 di

0451 D9 exx

0452 E1 pop hl

0453 5E id e,(h1)

0454 23 inc hl

0455 56 ld d,(h1)

0456 23 inc hl

0457 E5 push hl

0458 EB ex de,hl

0459 GE id e,(h1l)

OUS5SA 23 inc hl

O45B 56 id d,(h1)

O4SC 23 inc  hl

O45D 08 ex af,af'

OUSE 7E ld a, (h1)

O4SF  FEFC cp FC Rom# > 252 ?
0461 30BE Jr nc,0421 oui

0463 O6DF id b,DF activer Expansion Rom

-136-

KERNEL

0465 ED79 out  (c),a

0467 21A8B1 ld h1,B1A8 Rom ext. act.
O46A 46 ld b,(h1)

046B 77 ld (hl),a

046€ C5 push bc

0O46D FDES push jiy

046F 3D dec a

0470 FE07 cp 07

0472 300F Jr nc,0483

0474 87 add  a,a

0475 C6AC add  a,AC

0477 6F ld 1.a

0478 CEB1 adc a,B1

O47A 95 sub ]

047B 67 ld h,a

047C 7E ld a, (hl)

047D 23 inc hl

047E 66 ld h, (h1)

047F 6F id la

0480 ES push hl

0481 FDE1 pop {y

0483 067F ld b,7F

0485 79 id a,c

0486 CBD7 set  2,a L Rom disable
0488 CB9F res 3,a U Rom enable
O48A  CDA8B9 call B9A8 (0439) préparer config. et exécuter saut
O48D FDE1 pop iy

O48F F3 di

0490 D9 exx

0491 08 ex af,af’

0492 59 ld e,C

0493 C1 pop bc restaurer
0494 78 ld a,b ancienne

0495 O6DF ld b,DF configuration
0497 ED79 out (c),a de la Rom
0499 32A8B1 ld (B148),a (Rom ext. act.)
049C  067F ld b,7F

O4SE 7B id a,e

049F  188F Jr 0430

-137-

KERNEL

CTIIII II LILI SSL I II LIL LS LIL Li LE ll); Lil. EL; KL SIDE PCHL CONT’D

OUAT F3 di

OUA2 ES push hl
OuA3 D9 exx

O4A4 D1 pop de
O4AS 1808 Jr OUAF

CTILLIL IST LILI LL L LL LL LL LL LL LL L LL LAS) LS EL AL) RST 2 LOW SIDE CALL

CONT' DO

OUA7 F3 di

OuA8 D9 exx

O4A9 E1 pop hl
OUAA SE ld e,(h1)
O4AB 23 inc hl
O4AC 56 id d,(hl)
OUAD 23 inc  hl
OUAE ES push hl
O4AF 08 ex af,af'’
O4BO 7A ld a,d
O4B1 CBFA set 7,d
O4B3 CBF2 set  6,d
O4B5 E6CO and CO
O4B7 07 rlca

04B8 07 rica

04B9 21ABB1 ld h1,B1AB config. Rom act.
OuBC 86 add  a,(hl)
O4BD 1844 jr 0463

LILILLET I LL LL SIL L ELLE LI LL LILI SL LL LL LL LL LEE SEE 2), RST 5 FIRM JUMP CONT'D

OUBF F3 di

O4CO D9 exx

O4C1 E1 pop hl

O4C2 SE ld e,(h1l)

OùuC3 23 inc hi

O4C4 56 ld d,(h1)

04CS5 CB91 res 2,C

O4C7 ED49 out (c),c L Rom enable

04C9 EDS533FBA ld (BA3F),de charger adr. de saut
O4CD D9 exx

OUCE FB ei

-138-

KERNEL

O4CF  CD3EBA call BA3E (O4CF) et exécuter
O4D2 F3 di

O4D3 D9 exx

O4D4 CBD1 set  2,c L Rom disable
04D6 ED49 out  (c),c

O4D8 D9 exx

O4D9 FB ei

OUDA C9 ret

LALLLLLLLLLLLL LL LL ELLS LLLLL LL LIL LI LLLLLLLL LL LL] KL L ROM ENABLE
OUDB F3 di

O4DC D9 exx

OuDD 79 ld a,C

O4DE CB91 res  2,C

O4EO ED49 out  (c),c L Rom enable
O4E2 D9 exx

OUE3 FB ei

OUE4 C9 ret
LILLLLLLLELELLLLELLLELLLLLELLLEILELLILLLLLLLILLLLL ZX) KL L ROM DISABLE
OUES F3 di

OUE6 D9 exx

OUE7 79 ld a,C

OUE8 CBD1 set  2,c

OUEA ED49 out (c),c L Rom disable
OUEC D9 exx

OUED FB ei

OUEE C9 ret

LÉLLLL LL LL SL ELLES LLLL LILI LLLLLLLLELLLLLLLLLLLLLL] KL U ROM ENABLE

OUEF F3 di

O4FO D9 exx

O4F1 79 ld a,C

O4F2 CB99 res 3,cC

O4F4 EDU9 out  (c),c U Rom enable
04F6 D9 exx

O4F7 FB ei

OuF8 C9 ret

-139-

KERNEL

LL LS SSL SL SL SL LS LS SL LS LS SL SSL 2 LL Li LL iii 1222) KL U ROM DISABLE

O4F9 F3 di

OUFA D9 exx

O4FB 79 ld a,c

O4FC CBD9 set 3,c

O4FE ED49 out  (c),c U Rom disable

0500 D9 exx

0501 FB ei

0502 C9 ret

LL LALL RL LL LIL LI LL LS LS LLLSLILLLLLILILLILILILLII IIS] KL ROM RESTORE
0503 F3 di

0504 D9 exx a contient

0505 A9 xor € l'ancienne

0506 E60C and OC configuration

0508 A9 xor cC

0509 4F id C,a

O50A ED49 out (c),c

O50C D9 exx

050D FB ei

O5S0E C9 ret

LL DLL ILES D LLLLLILILLILELLLILLLSLLL LLLLLLLIILLZS ZT) KL ROM SELECT
050F  CDSEBA call BASE (O4EF) KL U ROM ENABLE CONT'D
0512 180F jr 0523

LELL LL ELLES LL LI LLLELLILLLLLLILLILILLLLILLIILILLII LL) KL PROBE ROM
0514 CD7EBA call BA7E (050F) KL ROM SELECT CONT'D
0517 3A00C0 ld a, (C000)

0O51A 2A01C0 ld h1, (CO01

LES LL LL LL LS LIL LS SL LILI LILI SIL IS LLILLLLLLLLLLLLS) ESS) KL ROM DESELECT
051D F5 push af

051E 78 ld a,b

051F CD72BA call BA72 (0503) KL ROM RESTORE CONT’'D
0522 F1 pop af

0523 ES push h]

0524 F3 di

0525 O6DF ld b, DF Expansion Rom (# dans c)
0527 ED49 out (c),c activer

-140-

KERNEL

0529 21A8B1 ld h1,B1A8 Rom ext. act,
052C 46 ld b, (h1)

052D 71 ld (h1),c

O52E 48 ld c,b

052F 47 ld b,a

0530 FB ei

0531 E1 pop hl

0532 C9 ret

CLS LL SSL LL LIL LIL SL LL LL LL SL LL LL LL LLLLLL LL LEZ 2:

KL CURR SELECTION

0533 3AA8B1 ld a, (B1A8) (Rom ext, act.)
0536 C9 ret
LALLLLLLLELLLLSILLLELLELILILLLILLLLLLESLLLLLLLLLS LS] KL LDIR
0537 CDB2BA call BAB2 (0543)

053A EDBO ldir

053C C9 ret
LELLLLLLLLLELLLLLLLLLLLLLLLLILLLLLLLLLLLLLLL LIL: KL LDDR
053D CDB2BA call BAB2 (0543)

0540 EDB8 lddr

0542 C9 ret

LELLLLLLLLLLLLLLLLLL LILI LLLLLLLLELLLLLLLELLLLLL LL EL] Rom off & save config.
0543 F3 di

0544 D9 exx

0545 E1 pop hl manipuler adr. RET
0546 C5 push bc rangeer ancienne config.
0547 CBD] set  2,c Roms

0549 CBD9 set 3,c disable

O54B ED49 out (c),c

O54D CDC7BA call call (hl)

0550 F3 di

0551 D9 exx rétablir ancienne
0552 C1 pop bc configuration

0553 ED49 out (c),c

0555 D9 exx

0556 FB ei

0557 C9 ret

0558 ES push hl manipuler adr. RET

-141-

KERNEL

0559 D9 exx
O55A FB ei
055B C9 ret

LILI SSL LIL LE LILI LL LL LL LL SL LL Li) LL. ;; Lil; LLL)))] RAM LAM

055C F3 di

055D D9 exx

OSSE 59 ld e,c

055F CBD3 set 2e Roms

0561 CBDB set 3,e disable

0563 ED59 out  (c).e

0565 D9 exx

0566 7E ld a,(hl) aller chercher octet
0567 D9 exx

0568 ED49 out (c),c fixer ancienne config.
0564 D9 exx

056B FB ei

056€ C9 ret

CELLLLLLE LL SSL LLLL LL LL LL LL LL LL LL LL LL LL LL.) LL.) RAM LAM (IX)

056D D9 exx

O56E 79 ld a,C :

056F  F60C or OC Roms

0571 ED79 out  (c),a disable

0573 DD/E00 ld a,(ix+00) aller chercher octet
0576 ED49 out (c),c fixer ancienne config.
0578 D9 exx

0579 C9 ret

O57A C7 rst 0

057B C7 rst O0

057C C7 rst O0

057D C7 rst O0

O57E C7 rst 0

057F C7 rst 0

-142-

MACHINE PACK

2.5.2 MACHINE PACK (MC)

C'est la partie du système d'exploitation qui est la plus proche de la
machine.

C'est ici que sont traités les divers interfaces et éléments
périphériques tels que PIO et PSG. Cette procédure présente l'avantage
qu’en cas de modification éventuelle de l'électronique, seul le MACHINE
PACK devra être adapté comme par exemple le BIOS en CP/M.

De ce fait, seules quelques routines peuvent être utilisées souvent.
Voici celles que nous avons sélectionnées:

MC PRINT CHAR sort le caractère qui se trouve dans a sur le port
centronics. Après retour de la routine, le carry est mis si le caractère
a bien été reçu.

MC SOUND REGISTER est intéressant pour les amateurs de musique. Sans que
vous ne vous torturiez l'esprit avec la transmission de données au PSG
qui est relativement compliquée, il vous suffit de transmettre le numéro
de registre et l'octet souhaités en les plaçant respectivement dans a et
C.

-143-

MACHINE PACK

LS SSL LR LES LS SSL LL SL SL LL LS SSL LLLLL ESS LES SE XL 2] RESET CONT'D

0580 F3 di

0581 0182F7 ld bc,F782 Control

0584 ED49 out  (c),c

0586 0100F4 ld bc.F400 Port À

0589 ED49 out (c),c

058B 0100F6 id bc,F600 Port C

O58E ED49 out  (c),c

0590 017FEF ld bc,EF7F Centronics

0593 ED49 out (c),c

0595 O6F5 ld b,F5 Port B

0597 ED78 in a, (c)

0599 E610 and 10 isoler LK4

059B 210405 id h1,05C4 fin de table 60Hz
059 2003 Jr nz,05A3 SOHz ? non =>
O5AO 21D405 ld h1,05D4 fin de table 50Hz
O5A3 010FBC ld bc, BCOF

O5A6 ED49 out (c),c charger adr. reg. video
O5A8 2B dec hl

OSA9 7E ld a, (h1)

OSAA O4 inc D

O5AB ED79 out (c),a charger reg. video
O5AD 05 dec b

OSAE OD dec c

OSAF F2A605 jp p, 05A6

05B2 1820 Jr 05D4

PERD DEE DE DD QU DD UE AUDE QU DU DD QU DU DE ED DE QUE QU QU QUE DE QUE DD DE EE Table 60HzZ

05B4 3F 28 2E 8& 26 00 19 ÎE
O5BC 00 07 00 00 30 00 CO 00

DEMO D D DE D DU DU D DU EAU NE QU DU DE UE AE AE AU QUE QE QUE QU QU QU QUE QUE DE QUEUE EE QE Table 50Hz

05C4 3F 28 2E 8 1F 06 19 1B
O5CC 00 07 00 00 30 00 CO 00

05D4 115C06 ld de,065C adresse de suite
05D7 210000 ld h1,0000 reset
OSDA 1832 jr 060€

EMEUENUEUE DE DEUE DU DU QU UE QU QU DE DE D DE QU DE QU DD DE EU OU DU QUE QU GED DE DE 98 DE DE MC BOOT PROGRAM

-144-

MACHINE PACK

05DC 3100C0 id sp, C000

O5DF ES push hl

OSEO CD681E call 1E68 SOUND RESET

O5E3 F3 di

O5E4 O01FFF8 id bc,F8FF rétablir peripherie
O5E7 ED49 out  (c),c

O5E9 CD5CO0 call 005C KL CHOKE OFF

OSEC E1 pop hl

O5ED D5 push de

OSEE C5 push bc

OSEF ES push hl

OSFO CDIE1A call 1AÏE KM RESET

O5F3 CD8810 call 1088 TXT RESET

O5F6 CDB10A call OAB1 SCR RESET

05F9 CD5EBA call BASE (O4EF) KL U ROM ENABLE CONT'D
OSFC E1 pop  hl

O5FD CD7507 call 0775 Jp (hl)

0600 C1 pop bc

0601 D1 pop de

0602 3807 jr c,060B MC START PROGRAM

0604 EB ex de,hl

0605 48 ld c.b

0606 11E806 id de, 06E8 erreur de chargement
0609 1803 Jr 060€

LLLLL LL LL LLELILLILI TILL SSL LILLLLILILLLLS III I III) MC START PROGRAM
060B 112607 ld de,0726 rencontre RET après 0654
O60E F3 di

060F  ED56 im 1

0611 D9 exx

0612 0100DF ld bc, DFO0 Palette Pointer reset
0615 ED49 out (c),c

0617 O01FFF8 ld bc,F8FF reset périphérie

061A ED49 out  (c),c éventuellement connectée
061C 2100B1 ld h1,B100 vider firmware-

061F 1101B1 ld de,B101 Ram

0622 O01FF07 ld bc,07FF

0625 3600 ld (h1),00

0627 EDBO ldir

0629 01897F ld bc,7F89 Ü Rom off & L Rom on

-145-

062€
062E
062F
0630
0631
0634
0635
0636
0637
0634
063D
0640
0643
0646
0649
064C
064F
0652
0653
0654
0657
0658
0659

Ç

D5
CD4400
CD8808
CDE019
CD681E
CDAOOA
CD7810
CDB015
CD7023
CDE607
FB
E1
CD7507
C1
E1
C37700

out
exx
xor
ex
ld
push
push
push
call
call
call
call
call
cal]
call
call
call
ei
pop
call
pop
pop
jp

MACHINE PACK

(c),c

a
af,af'
sp, C000
hl

bc

de

0044
0888
19E0
1E68
OAA0O
1078
15B0
2370
07E6

Screen Mode 1

Restore High Kernel Jumps
JUMP RESTORE

KM INITIALISE

SOUND RESET

SCR INITIALISE

TXT INITIALISE

GRA INITIALISE

CAS INITIALISE

MC RESET PRINTER

Jp (h1)

U initialiser Rom

LÉ L ELLE LL LS LL EL LL ILE STI TESTS ITS I LT II III) 11111) Reset

065€
065F
0662
0665
0668
066B

CD1207
CDEB06
216D06
CDEB06
219306
187E

call
call

0712
O6EB
h1,066D
O6EB
h1,0693
O6EB

sortir nom de société

sortir messages

message de mise sous tension
sortir messages

Copyright

Sortir messages

LL LE SSL LL LL LL LL EL LL LIL LE LIL LL) Message de mise sous tension
20 36 34 4B 20 HD 69 63
72 6F 63 6F 6D 70 75 74
65 72 20 20 28 76 31 29
OD OA OD OA 00 43 6F 70
79 72 69 67 68 74 20 A4
31 39 38 34 20 41 6D 73

066D
0675
067D
0685
068D
0695

64K Mic
rocomput
er (vi)
0. COP
yright

19846AmS

-146-

MACHINE PACK

69) 74 72 61 64 20 43 6F 6€ trad Con

06A5 73 75 6D 65 72 20 45 6C sumer El

06AD 65 63 74 72 6F 6E 69 63 ectronic

06B5 73 20 70 6C 63 OD OA 20 s pic..

06BD 20 20 20 20 20 20 20 20

06C5 20 20 61 6E 64 20 4C 6F and Lo

06CD 63 6F 6D 6F 74 69 76 65 comotive

06D5 20 53 6F 66 74 77 61 72 Softwar

06DD 65 20 4C 74 64 2E OD OA e Ltd...

06E5 OD OA 00 AE

06E8 21F406 ld h1,06F4 message erreur de chargement
LALLE LIL L LL LIL LT LS LLILIL III IILL III I LILI 121122) Sortir messages
06EB 7E id a,(h1)

O6EC 23 inc hl

O6ED B7 or a

O6EE C8 ret 7z

O6EF  CD0014 call 1400 TXT OUTPUT

06F2 18F7 Jr O6EB Sortir messages
CILLLILILLI TILL IIIIILLLILLLLILELLLLLLIL] message erreur de chargement
06F4 2A 2A 2A 20 50 52 4F 47 *#** PROG

06FC 52 41 4D 20 4C 4F 41 44 RAM LOAD

0704 20 46 41 49 4C 45 44 20 FAILED

070C 2A 2A 2A OD OA 00 LES

0712 O6F5 ld b,F5

0714 ED78 in a,(c) Port B

0716 2F cpl

0717 E6OE and 0€ isoler LK1..,3
0719 OF rrca /2

071A 212707 ld h1,0727 nom de société.
071D 3C inc a

071E 47 id b,a

071F 7E id a,(h1)

0720 23 inc  hl

0721 B7 or a

0722 20FB Jr nz,071F

0724 10F9 djinz 071F

-147-

MACHINE PACK

0726 C9 ret

UTTITTIIILIILS LILI LILI LILI LL SLI ALL LL 2 2 222 Nons de société
0727 41 72 6E 6F 6C 64 00 OA Arnold..

072F 20 41 6D 73 74 72 61 64 Amstrad

0737 00 OA 20 4F 72 69 6F 6E .. Orion

073F 00 OA 20 53 63 68 6E 65 .. Schne

0747 69 64 65 72 00 OA 20 41 ider.. À

O74F 77 61 00 OA 20 53 6F 6C wa.. Sol

0757 61 76 6F 78 00 OA 20 53 avox., S

075F 61 69 73 68 6F 00 OA 20 aisho..

0767 54 72 69 75 6D 70 68 00 Triumph.

076F OA 20 49 73 70 00 . ISp,

0775 E9 Jp (hl1)

CTTITITIIIIILILLLII LIL LI LL SL LS LL LL LL Li LL LL LL 2 22 MC SET MODE
0776 FEO3 cp 03 Mode > 2 ?

0778 DO ret nc Oui =>

0779 F3 di

077A D9 exx

077B CB89 res 1,C reset Mode Bits
077D CB81 res 0,

077F BI or c

0780 4F ld c.a fixer nouveau mode
0781 ED49 out (c),c

0783 FB ei

0784 D9 exx

0785 C9 ret

ADN ANA U QU UD OU OU UE QU OA OU UE UE QU 0 4 DO QU QUO QUE QU DE QUEUE UE 4 MC CLEAR INKS

0786 C5 push bc

0787 DS push de

0788 01107F 1d bc,7F10

078B CDABO7 call O7AB Sortir couleur
078E 0E00 ld c,00

0790 CDABO7 call O7AB Sortir couleur
0793 1B dec de

0794  20FA jr nz,0790

0796 D1 pop de

-148-

MACHINE PACK

0797 C1 pop bc
0798 C9 ret

LR L LL LIL LL LS LL LL LLLL LL LL LL SL LIL LL LL LLLZLZ SEE EE] MC SET INKS

0799 C5 push bc

079A D5 push de

079B 01107F ld bc,7F10 couleur bord
079E  CDABO7 call O7AB Sortir couleur
07A1 0E00 ld c,00 Adr, Ink 0

07A3 CDABO7 call O7AB Sortir couleur
0746 20FB Jr nz,07A3 charger toutes les
0748 D1 pop de mémoires couleurs
O7A9 C1 pop bc

O7AA C9 ret

CILILLLLLLLLLLLLI LL LL LL LLLLLELELLLLL LL LL LLLZLEL ELLE: Sortir couleur
07AB ED49 out (c),c Palette Pointer
O7AD 1A ld a, (de)

O7AE 13 inc de

O7AF E61F and 1F

07B1 F640 or 40

07B3 ED79 out  (c),a couleur

07B5 OC inc c

07B6 79 ld a,c

07B7 FE10 cp 10

07B9 C9 ret
LILLILLLILLLLLLLLLILLELLLILLELELL LL LIL LLLLLLLLEL) MC WAIT FLYBACK
07BA F5 push af

07BB C5 push bc

07BC O6F5 ld b,F5 Port B

O7BE ED78 in a, (c)

07C0 1F rra VSYNC ?

07C1 30FB Jr nc,07BE non => attendre
07C3 C1 pop bc

O7C4 F1 pop af

07C5 C9 ret

LALLEL LL LL LL LL LL LS LL LL LL RL LS LS LE LL SSL LES LS ELLE ES 2) MC SCREEN OFFSET

07C6 C5 push bc

-149-

MACHINE PACK

07C7 OF rrca

07C8 OF rrca

07C9 E630 and 30

O7CB 4F ld c,a

O7CC 7C id a,h

O7CD 1F rra

O7CE E603 and 03

07D0 B1 or C

07D1 010CBC ld bc,BCOC

07D4 ED49 out (c),c Video Contr Reg 12
07D6 O4 inc bb

07D7 ED79 out (c),a Début écran Hi

07D9 05 dec ”b

O7DA OC inc cc

07DB ED49 out  (c),c Reg 13

07DD O4 inc b

O7DE 7C ld a,h

O7DF 1F rra

07E0 7D ld a, 1

O7E1 1F rra

07E2 ED79 out  (c),a Début écran Lo

O7E4 C1 pop bc

O7E5 C9 ret

LLLLLILLL LILI LL LIL LLILLLLLILLLLLSLLLLLLLLLLLES SE: MC RESET PRINTER
07E6 21EC07 ld h1,07EC Restore Printer Indirection
07E9 C38A0A Jp OA8A Move (h1+3)=>((h1+1)),cnt=(h1)
O7EC 03 db 03 3 octets

O7ED F1BD dn BDF1 adresse objet

O7EF C3F807 Jp 07F8 MC WAIT PRINTER

LELLLLL LL LL LL LL LL LLLLLLLLLLLLLLL LL LL LL LLLLLLZLLEL)] MC PRINT CHAR
07F2 C5 push bc

07F3 CDF1BD call BDF1 MC WAÏT PRINTER

07F6 C1 pop bc

07F7 C9 ret

LELLL LES EL LL LL LS SEL LS DELL LL LL LL D LES 522) LEE) MC WAIT PRINTER

07F8 013200 id bc, 0032

-150-

MACHINE PACK

07FB CD1B08 call 081B MC BUSY PRINTER
O7FE 3007 Jr nc, 0807 MC SEND PRINTER
0800 10F9 dinz 07FB

0802 OD dec c

0803 20F6 Jr nz,07FB

0805 B7 or a

0806 C9 ret

LALLLLILLLILLLLLILLLLL LILI LLLILLLILILLLILL LL LLLLZLEL: MC SEND PRINTER
0807 C5 push bc

0808 O6EF id b,EF

0804 E67F and 7F octet sans strobe
080C ED79 out  (c),a vers l'imprimante
080E F680 or 80

0810 F3 di

0811 ED79 out  (c),a Strobe mis

0813 E67F and 7F

0815 FB ei

0816 ED79 out  (c),a Strobe éteint
0818 C1 pop bc

0819 37 scf

O81A C9 ret

LILLLLLLLLLLLILLLLLILLL LL LL LLLILLLLLLLLLLLLLLLL EL: MC BUSY PRINTER
081B C5 push bc

081C 4F ld c,a

081D 06F5 ld b,F5 Port B

081F ED78 in a, (c)

0821 17 rla Printer Busy

0822 17 rla => Carry

0823 79 ld a,c

0824 C1 pop bc

0825 C9 ret

LL LL ESS LL LL LL LS LL LL LL LL Li LL LL LL LL L LL: SZ 222 MC SOUND REGISTER

0826 F3 di

0827 06F4 ld b,F4 Port A
0829 ED79 out  (c),a Sound Reg#
082B 06F6 ld b,F6 Port C
082D ED78 in a, (c) Sound Chip

-151-

082F
0831
0833
0835
0837
0839
083B
083D
083E
0840
0842
0844
0845

F6C0
ED79
E63F
ED79
06F4
ED49
06F6
4F

F680
ED79
ED49
FB

C9

or
out
and
out
ld
out

ld
or
out
out
el
ret

MACHINE PACK

co
(c),a
3F
(c),a
b,F4
(c),c
b,F6
c,a
80
(c),a
(c),c

sur entrée
& Strobe mis

Strobe éteint
Port À
Données sound
Port C

latcher données

LiLi LL lili Li Lili: li::11:::):,):::1::111):11:] Scan Keyboard

0846
0849
O84B
O84D
O84F
0851
0852
0854
0856
0858
0859
085B
085D
085€
0860
0862
Ô864
0866
0868
0869
086A
086B
086C
086D
086€

010EF4
ED49
06F6
ED78
E630
4F
F6C0
ED79
ED49
04
3E92
ED79
C5
CBF1
06F6
ED49
06F4
ED78
46
77
A0
2F
12
23inc
13

ld
out
ld
in
and
ld
or
out
out
inc
ld
out
push
set
ld
out
ld
in
ld
ld
and
cpl
ld

inc

bc,F40E
(c),c
b,F6

a, (c)
30

Port À
Sound Reg 14 (Keyb X Input)
Port C

Strobe mis
Strobe éteint

Port AgB=Input
Control

Port C

Keyb Y Outp & X Inp

Port A

Données (Keyb X Inp) => a

-152-

MACHINE PACK

086F OC inc c Keyb Y +1
0870 79 ld a,C

0871 E60OF and OF

0873 FEOA cp OA tous canaux Ÿ traités ?
0875 20€9 Jr nz, 0860 non => suivant
0877 C1 pop bc

0878 3E82 ld a,82 Port À Output
087A ED79 out  (c),a Control

087C 05 dec b

087D ED49 out  (c),c Port C

087F C9 ret

0880 C7 rst O0

0881 C7 rst O0

0882 C7 rst O0

0883 C7 rst O0

0884 C7 rst 0

0885 C7 rst O0

0886 C7 rst O0

0887 C7 rst O0

-153-

JUMP RESTORE

2.5.3 JUMP RESTORE (JRE)

Ce pack sert uniquement à affecter à nouveau aux adresses MAIN JUMP leurs
valeurs par défaut.

Pour les FIRM JUMPS, un RST1 est placé devant, pour les ARITHMETIK JUMPS,
c'est un RSTS.

Si vous pensez que vous avez modifié trop de vecteurs, tirez simplement
la manette d'alarme en appelant JUMP RESTORE.

C'est également conseillé lorsque vous sortez d’un programme dans lequel
vous avez généreusement offert au système d'exploitation vds propres
routines.

-154-

JUMP RESTORE

SDNEDE DD DE DE DEDE DEEE DE DEEE HE DE DE DE DH DE DE DEEE DE MED DE DE DEDE DD DE DE DE DE DEEE JUMP RESTORE

0888 11AC08 id de,O8AC Main Jump Adr.
088B 2100BB ld h1,BB00

088E 01CFBF id bc, BFCF Cnt => b , RSTI => c
0891 CD9708 call (0897

0894 01EF30 ld bc, 30EF Cnt => b , RSTS => c
0897 71 ld (h1),c

0898 23 inc h1l

0899 1A ld ä, (de)

089A 77 ld (hl1),a

089B 13 inc de

089C 23 inc hl

89) EB ex de,hl

O8 79 ld a,C

089F 2F cpl

0840 07 rlca

O8A1 07 rlica

0842 E680 and 80

08A4 B6 or (h1)

08A5 EB ex de,hl

0846 77 Id (h1),a

0847 13 inc de

0848 23 inc hl

0849 10EC djnz 0897

08AB C9 ret

LELLL LE LILI LS ELLE LLLLILLLLLILLILLLLLILLLLLLILL LL LL LS: Main Jump Adr.,
O8AC E019 dw 190 KM INITIALISE
O8AE 1E1A du 1A1E KM RESET

08B0 3C1A dw 1A3C KM WAIT CHAR
08B2 421À dw 1A42 KM READ CHAR
08B4 771A dw 1477 KM CHAR RETURN
08B6 BDIA dn 1ABD KM SET EXPAND
08B8 2E1B dw 1B2E KM GET EXPAND
08BA 7BIA dw 1A7B KM EXP BUFFER
08BC 561B dw 1B56 KM WAIT KEY
O8BE S5C1B dw 1B5C KM READ KEY
08CO BD1C dw 1CBD KM TEST KEY
08C2 B31B dn 1BB3 KM GET STATE
08C4 5C1C dw 1C5C KM GET JOYSTICK

-155-

JUMP RESTORE

08C6 521D dw 1D52 KM SET TRANSLATE
08C8 3E1D dw 1D3€ KM GET TRANSLATE
O8CA 571) dw 1D57 KM SET SHIFT
O8CC 431D dn 1D43 KM GET SHIFT
O8CE 5C1D dw 1D5C KM SET CONTROL
08D0 481) dw 1D48 KM GET CONTROL
08D2 ABIC dw 1CAB KM SET REPEAT
08D4 A61C dw 1CA6 KM GET REPEAT
08D6 6D1C dn 1C6D KM SET DELAY
08D8 691C dw 1C69 KM GET DELAY
O8DA 711C dw 1071 KM ARM BREAK
08DC 821C du 1C82 KM DISARM BREAK
O8DE 901C dw 1C90 KM BREAK EVENT
08E0 7810 dw 1078 TXT INITIALISE
082 8810 dn 1088 TXT RESET

O8E4 5114 dn 1451 TXT VDU ENABLE
O8E6 4B14 dn 144B TXT VDU DISABLE
08E8 0014 dn 1400 TXT OUTPUT

O8EA 3413 du 1334 TXT WR CHAR

O8EC AB13 dw 13AB TXT RD CHAR

O8EE A713 dw 13A7 TXT SET GRAPHIC
08F0 0C12 du 120€ TXT WIN ENABLE
08F2 5612 dw 1256 TXT GET WINDOW
O8F4 4015 dw 1540 TXT CLEAR WINDOW
08F6 5E11 dw 115E TXT SET COLUMN
08F8 6911 dw 1169 TXT SET ROW

O8FA 7411 dn 1174 TXT SET CURSOR
08FC 8011 dw 1180 TXT GET CURSOR
O8FE 8912 dw 1289 TXT CUR ENABLE
0900  9A12 dw 129À TXT CUR DISABLE
0902 7912 dn 1279 TXT CUR ON

0904 8112 dw 1281 TXT CUR OFF

0906 CE11 du 11CE TXT VALIDATE
0908 6812 dw 1268 TXT PLACE/REMOVE CURSOR
090A 6812 dn 1268 TXT PLACE/REMOVE CURSOR
090€ A912 dn 12A9 TXT SET PEN

O90E BD12 dn 12BD TXT GET PEN

0910 AE12 dw 12AE TXT SET PAPER
0912 C312 dw 1203 TXT GET PAPER
0914 C912 dw 1209 TXT INVERSE

-156-

JUMP RESTORE

0916 7A13 dn 137A TXT SET BACK

0918 8713 dw 1387 TXT GET BACK

091A D312 dw 12D3 TXT GET MATRIX
091C F112 dw 12F1 TXT SET MATRIX
O91E FD12 dn 12FD TXT SET M TABLE
0920 2A13 dw 132A TXT GET M TABLE
0922 CB14 dw 14CB TXT GET CONTROLS
0924 E810 dw 10E8 TXT STR SELECT
0926 0711 du 1107 TXT SHAP STREAMS
0928 B015 dw 15B0 GRA INITIALISE
O92A DF15 dw 15DF GRA RESET

092C F415 dn 15F4 GRA MOVE ABSOLUTE
092€ F115 dn 15F1 GRA MOVE RELATIVE
0930 FC15 dw 15FC GRA ASK CURSOR
0932 0416 dw 1604 GRA SET ORIGIN
0934 1216 dw 1612 GRA GET ORIGIN
0936 3417 dw 1734 GRA WIN WIDTH
0938 7917 dw 1779 GRA WIN HEIGHT
093A A617 dw 17A6 GRA GET W WIDTH
093C BC17 dw 17BC GRA GET W HEIGHT
093E C517 dw 1705 GRA CLEAR WINDOW
0940 F617 dn 17F6 GRA SET PEN

0942 0418 dw 1804 GRA GET PEN

O944 FD17 dw 17FD GRA SET PAPER
0946  OA18 dw 180A GRA GET PAPER
0948 1318 dn 1813 GRA PLOT ABSOLUTE
O94A 1018 dw 1810 GRA PLOT RELATIVE
O94C 2718 dw 1827 GRA TEST ABSOLUTE
O9UE 2418 dw 1824 GRA TEST RELATIVE
0950 3918 dn 1839 GRA LINE ABSOLUTE
0952 3618 dn 1836 GRA LINE RELATIVE
0954 4519 dw 1945 GRA WR CHAR

0956  AOOA dw OAAO SCR INITIALISE
0958 B10A dw OAB1 SCR RESET

O95A 3COB dw OB3C SCR SET OFFSET
095C 450B dw OB45 SCR SET BASE

O95E 500B dw 0B50 SCR GET LOCATION
0960  CADA dn OACA SCR SET MODE

0962 ECOA dw OAEC SCR GET MODE

0964 F70A dw OAF7 SCR CLEAR

—157-

JUMP RESTORE

0966 570B dw 0B57 SCR CHAR LIMITS
0968 640B dw OB64 SCR CHAR POSITION
096A  A9OB dw OBA9 SCR DOT POSITION
096C F90B dn OBF9 SCR NEXT BYTE
096E 050€ dw OC05 SCR PREV BYTE
0970 130€ dw 0C13 SCR NEXT LINE
0972 2D0C dw OC2D SCR PREV LINE
0974 860€ dn 0C86 SCR INK ENCODE
0976  AO0C dw OCAO SCR INK DECODE
0978 ECOC dw OCEC SCR SET INK

097A 140D dw OD14 SCR GET INK

097C F10C dw OCF1 SCR SET BORDER
097E 190D dw OD1S SCR GET BORDER
0980 EU4OC dw OCE4 SCR SET FLASHING
0982 E80C dw OCE8 SCR GET FLASHING
0984: B30D dw ODB3 SCR FILL BOX
0986 B70D dw ODB7 SCR FLOOD BOX
0988 DFOD dn ODDF SCR CHAR INVERT
098A FAOD dw ODFA SCR HW ROLL

098C 3E0E dn 0E3E SCR SW ROLL

098E F30E dn 0EF3 SCR UNPACK

0990  490F dw 0F49 SCR REPACK

0992 490€ dn OC49 SCR ACCESS

0994  6BOC dn OC6B SCR PIXELS

0996  C40F dw OFC4 SCR HORIZONTAL
0998 2F10 dw 102F SCR VERTICAL
099A 7023 dn 2370 CAS INITIALISE
099€ 7F23 dn 237F CAS SET SPEED
099%  8E23 dw 238E CAS NOISY

O9AO 4B2A dn 2AUB CAS START MOTOR
O9A2 4F2A dw 2AUF CAS STOP MOTOR
O9A4 512A dw 2A51 CAS RESTORE MOTOR
09A6 9223 dw 2392 CAS IN OPEN

09A8 FC23 du 23FC CAS IN CLOSE
O9AA 0124 dw 2401 CAS IN ABANDON
O9AC 3524 dw 2435 CAS IN CHAR

O9AE  AB24 dw 24AB CAS IN DIRECT
O9BO 9A24 dn 249A CAS RETURN

O9B2 9624 dw 2496 CAS TEST EOF
O9B4  AB23 dn 23AB CAS OUT OPEN

-158-

O9B6
09B8
O9BA
O9BC
O9BE
09C0
09c2
09C4
09C6
09C8
O9CA
O9CC
O9CE
09D0
09D2
O9D4
09D6
09D8
O9DA
O9DC
O9DE
09E0
092
O4
O%E6
OSE8
OSEA
OSEC
OSEE
09F0
09F2
09F4
09F6
09F8
09FA
O9FC
O9FE
0400
0A02
OAO4

1524
2E24
5B24
EA24
2825
3F28
3628
5128
681E
9F1F
6C20
8920
4A20
CBIE
E61E
3823
3D23
4923
LE 23
5C00
2903
3203
A102
B202
6301
6A01
7001
7601
7D01
8301
B301
C501
D201
E201
2802
8502
5602
1402
7702
9502

dW
dw
du
dW
dw
dw
dw
dy
dw
dw
dw
dw
du
dy
du
dw
du
dw
dy
dw
dw
du
du
dw
dn
dw
dw
dw
dn
du
du
dw
du
dw
dw
dy
dw
dn
dn
du

2415
242E
245B
2UEA
2528
283F
2836
2851
1E68
1F9F
206€
2089
20UA
1ECB
1EE6
2338
233D
2349
234E
005C
0329
0332
O2A1
02B2
0163
016A
0170
0176
017D
0183
01B3
01C5
01D2
O1E2
0228
0285
0256
0214
0277
0295

JUMP RESTORE

CAS OÙ
CAS OÙ
CAS OU
CAS OÙ
CAS CA
CAS WR
CAS RE

T CLOSE

T ABANDON
T CHAR

T DIRECT
TALOG

ITE

AD

CAS CHECK

SOUND
SOUND
SOUND
SOUND
SOUND
SOUND
SOUND
SOUND
SOUND
SOUND
SOUND
KL CHO
KL ROM
KL INI
KL LOG
KL FIN
KL NEW
KL ADD
KL DEL
KL NEW
KL ADD
Delete
KL ADD
Delete
KL INI
KL EVE
KL SYN
KL DEL
KL NEX
KL DO

RESET

QUEUE

CHECK

ARM EVENT
RELEASE

HOLD
CONTINUE
AMPL ENVELOPE
TONE ENVELOPE
A ADDRESS

T ADDRESS

KE OFF

WALK

T BACK

EXT

D COMMAND
FRAME FLY
FRAME FLY
FRAME FLY
FAST TICKER
FAST TICKER
Fast Ticker
TICKER
Ticker

T EVENT

NT

C RESET
SYNCHRONOUS
T SYNC

SYNC

KL DONE SYNC

KL EVE

-159-

NT DISABLE

0A06
0A08
OADA
OAOC
OA0E
0A10
OA12
OA14
0A16
0A18
OA1A
OA1C
OA1E
OA20
OA22
OA24
0A26

9B02
8E02
9900
A300
DCO5
0B06
BA07
7607
C607
8607
9907
E607
F207
1B08
0708
2608
8808

dw
du
dw
du
du
du
dw
du
dn
du
dn
dw
dw
dw
du
du
dy

JUMP RESTORE

029B
028E
0099
O0A3
O5DC
060B
O7BA
0776
07C6
0786
0799
07E6
07F2
081B
0807
0826
0888

KL EVENT ENABLE
KL DISARM EVENT
KL TIME PLEASE
KL TIME SET

‘MC BOOT PROGRAM

MC START PROGRAM
MC WAIT FLYBACK
MC SET MODE

MC SCREEN OFFSET
MC CLEAR INKS

MC SET INKS

MC RESET PRINTER
MC PRINT CHAR

MC BUSY PRINTER
MC SEND PRINTER
MC SOUND REGISTER
JUMP RESTORE

LELEELL LL L LL LL LL LL); Lili iii) iii) il): ;:;:] Basic Jump Adr,

OA28
OA2A
OA2C
OA2E
0A30
OA32
OA34
0A36
0A38
OA3A
DA3C
OA3E
OAUO
OA42
OA
OA46
OAUS
OAUA
OAUC
OAUE
OA50

982A
182E
292E
552E
662€
8E2E
A12E
AC2E
B62E
1D2F
3F33
3733
3B33
1534
SE 34
7835
9A35
F835
E835
AE31
A331

dn
dw
dn
du
dn
dn
dw
du
dn
du
dW
dw
(]
dw
dW
dn
dn
dn
dw
dw
dW

2A98
2E18
2E29
2E55
2E66
2E8E
2EA1
2EAC
2EB6
2F1D
333F
3337
333B
3415
349E
3578
3594
35F8
35E8
31AE
3143

EDIT

FLO Var, (de) => (hl)
FLO Int => Flo

FLO 4-octets => F10
FLO Flo => Int

FLO Flo => Int

FLO Fix

FLO Int

FLO nombre * 10°a
FLO Add

FLO Sub

FLO Sub

FLO Mul

FLO Div

FLO nombre * 2°a
FLO Cmp

FLO +/-

FLO Sgn

FLO Deg/Rad

FLO Pi

-160-

OA52
OAS4
0A56
OA58
OASA
OASC
OASE
OA60
0A62
0A64
0A66
0468
OAG6A
OA6C
OA6E
OA70
OA72
OA74
0A76
OA78
OA7A
OA7C
OA7E
0A80
0A82
OA84
0A86
0488

JUMP RESTORE

FLO
FLO
FLO
FLO
FLO
FLO
FLO
FLO
FLO
FLO
FLO
FLO
FLO
FLO

INT
INT
INT
INT
INT
INT
INT
INT

Racine
Puissance

Log

Log10

Exp

sin

Cos

Tan

Atn

4-Octets * 256 => Flo
RND Init

SET RANDOM SEED
RND

GET LAST RND

mettre Sgn dans b
Add

Sub

Sub

Mul

Div

Mod

Mul unsigned

hl/de => hl, Rest => de

INT
INT
INT

Cmp
+/-
Sgn

LELLS LE SL LS SSL LES LL SL LL LL. LL ;) Move (h1+3)=>( (h1+1 ) ), cnt= (h1 )0

OA8A
OA8B
OA8D
OA8E
OA8F
OA90
DA91
0A92
OA94

0A31 dn 3104
0D31 (ei 310D
1430 du 3014
0F30 dw 300F
9030 dw 3090
BC31 du 31BC
B231 dW 31B2
3132 dw 3231
4132 dw 3241
SE2E dw 2E5E
942F dw 2F94
A12F dn 2FA1
B72F dw 2FB7
E62F dw 2FE6
0837 dw 3708
0E37 dw 370€
1537 dw 3715
2837 dn 3728
3137 du 3731
3037 dW 3730
3937 dn 3739
7A37 dn 3774
8137 dW 3781
5037 dw 3750
8C37 dn 378C
E937 dW 37E9
D437 dw 37D4
E037 dw 37E0
UE id c,(hl)
0600 ld b,00
23 inc hl
DE ld e,(h1)
23 inc hl
56 id d,(hl)
23 inc hl
EDBO ldir

cg ret

-161-

OA95
0A96
OA97
OA98
DA99
OA9A
OA9B
OA9C
OA9D
OAE
OA9F

C7
C7
C7
C7
C7
C7
C7
C7
C7
C7
C7

rst
rst
rst
rst
rst
rst
rst
rst
rst
rst
rst

O0CGOGOOoOCOoOooo

JUMP RESTORE

-162-

SCREEN PACK

2.5.4 SCREEN PACK (SCR)

Le SCREEN PACK est subordonné au TEXT PACK et au GRAPHICS PACK, Il se
charge de la réalisation pratique des tâches ordonnées par ces deux
packs. Il est en effet responsable du traitement direct de l'écran.

Voici les routines que nous voulons vous présenter :

SCR NEXT BYTE et SCR PREV BYTE fournissent dans hl l'adresse écran de la
prochaine ou de la dernière position d'octet, lorsque vous placez dans
hl, avant d'appeler la routine, l’ancienne adresse. C'est aussi pratique
que cela semble superflu, En effet, du fait de l'organisation de l'écran,
il n'est pas facile de déterminer la position d'octet. La distance dépend
en outre du mode. Notez que si la prochaine ou la dernière position sort
du cadre de l’écran, l'adresse fournie en retour n'a pas de sens, Elles
se trouve en effet alors dans la zone des 48 derniers octets de la Ram
vidéo, qui ne sont pas utilisés pour la représentation sur l'écran,

SCR NEXT LINE et SCR PREV LINE travaillent de façon similaire, si ce
n'est que l'adresse écran est calculée une ligne entière avant ou après.
Ici également, l'adresse n’a pas de signification lorsqu'on sort de la
zone représentable.

SCR HW ROLL décale l'écran d'une ligne vers le bas lorsque b=0 et d’une
ligne vers le haut lorsque b<>0.

a doit recevoir la couleur que devra avoir la nouvelle ligne (vide) qui
sera ajoutée.

SCR SW ROLL décale une zone de l'écran. a et b doivent étre servis comme
ci-dessus, h doit en outre recevoir le numéro de colonne du bord gauche
de la zone à décaler, 1 la ligne supérieure, d la colonne droite et e la
ligne inférieure de cette zone.

Notez que colonne et ligne O0 ccrrespondent à l'angle supérieur gauche de
l'écran. Faites vous-même très attention à ce que les paramètres transmis
marquent bien une zone comprise dans la Ram vidéo.

-162-

SCREEN PACK

DEN NEED DEEE DE DE DE DEEE NE DE DEEE DE DD NE DE DE HE DE DE DE DEEE DE DE DE DE DE DEN NE SCR INITIAL ISE

OAAO 114D10 ld de, 104D Couleurs par défaut
OAA3 CD8607 call 0786 MC CLEAR INKS

OAA6 3EC0O ld a, C0

OAA8 32CBB1 ld (B1CB),a (High Byte début écran)
OAAB CDB10A call OAB1 SCR RESET

OAAE C3F20A JP OAF2

RENONCE DE DE DEEE DE DE DE DE DE DE DE DEN HE D HD DE DE DE DE DE DD DE DE DD DE DE DE DE DE DE DE DEEE SCR RESET

OAB1 AF xor a

OAB2 CD490C call 0C49 SCR ACCESS

OABS 21BEOA ld h1,0ABE Restore SCR Indirections

OAB8 CD8A0A call OA8A Move (h1+3)=>((h1+1)),cnt=(h1)
OABB C3D20C Jp OCD2 Reset couleurs

OABE 09 db 09 9 octets

OABF ESBD du BDES Adresse objet

OAC1 C3820C Jp 0C82 SCR READ

OACH C3680C Jp 0C68 SCR WRITE

OAC7 C3F70A Jp OAF7 SCR CLEAR

LL SLLLL LL LES LS LL LL LL LL LL LL LL L LS LLLLLL LS ILE L SEL) SCR SET MODE

OACA E603 and 03

OACC FEO3 cp 03

OACE DO ret nc

OACF F5 push af

OADO CD4FOD call OD4F

OAD3 F1 pop af

OAD4 SF ld e,a

OADS CDB710 call 10B7

OAD8 F5 push af

OAD9 CDD615 call 15D6

OADC E5 push hl

OADD 7B ld a,e

OADE CD110B call O0B11 charger cartes bits
OAE1 CDEBBD call BDEB SCR MODE CLEAR
OAE4 E1 pop hl

OAES CDB615 call 15B6

-163-

SCREEN PACK

OAE8 F1 pop af

OAE9 C3D510 jp 10D5

LILSLE LILI LL LLELLS LILI SS RSS LLLLLLLS LL TILL LS RTL TILL. SCR GET MODE
OAEC 3AC8B1 ld a, (B1C8) (curr. Screen Mode)
OAEF  FE01 cp 01

OAF1T C9 ret

OAF2 3E01 ld a,01

OAF4 CD110B call 0B11 Charger cartes bits

Ç

LEE LES LS LEE L ELLES ILES ELLES LL LIL LL TELL LL LL LL SL.) SCR MODE CLEAR

OAF7 CD4FOD call OD4F

OAFA 210000 1d h1,0000

OAFD CD3COB call OB3C SCR SET OFFSET

OB0OO 2ACAB1 ld h1,(B1CA)  (Adr. Screen Start)
0B03 2E00 ld 1,00

0BOS 54 ld d,h hl=adresse de base
0B06 1E01 ld e,01 de=adresse de base +1
OBO8 01FF3F ld bc, 3FFF 16k

OBOB 75 id (h1),1

OBOC EDBO idir vider l'écran

OBOE C33C0D Jp OD3C
LLLLLLLILILLLSSLLILILLLSSLLLLLSLLLLLLLLLSLSLSLSLZSLZ Charger cartes bits
0B11 213A0B ld h1,0B3A Cartes bits Mode 0
OB14 FEO1 cp 01

0B16 3808 Jr c,0B20

0B18 21360B ld h1,0B36 Cartes bits Mode 1
0B1B 2803 jr z,0B20

OB1D 212E0B ld h1,0B2E Cartes bits Mode 2
0B20 11CFB1 id de,B1CF Cartes bits suivant Mode
OB23 010800 ld bc,0008

0B26 EDBO ldir

OB28 32C8B1 ld (B1C8),a (curr, Screen Mode)
OB2B C37607 Jp 0776 MC SET MODE

LÉLES SELS SSL S LS SELS LL LL SL LL LE LL) LL LI LLLEZELLES EE): ) Cartes bits Mode 2

OB2E 80 40 20 10 O8 O4 02 01

-164-

SCREEN PACK

HN HE DE DE MED HE DE DEEE DH DEEE D DE D DE DH D DE ED DE DE Cartes bits Mode 1

0B36 88 44 22 11

MONDE DH HE HE HE DEEE DEEE DEEE OU DEEE DEEE DE DEEE DE DEEE Cartes bits Mode 0

OB3A AA 55

DEN HD DD DD HE DE DEEE DE DE DEEE D DEEE DEEE ÉD DE DE EE SCR SET OFFSET

OB3C 7C ld ah
OB3D E607 and 07
OB3F 67 ld ha
OB4O 22C9B1 ld  (B1C9),h]
0B43 1805 Jr OB4A

PODEHE DE DH DH HE HE HD DE DÉH DE DEDE DEEE DE DD DEEE OH SCR SET BASE

0B45 E6C0 and CO

OB47 32CBB1 ld (B1CB),a (High Byte Screen Start)
OB4A CD500B call OB50 SCR GET LOCATION

OB4D C3C607 Jp 07C6 MC SCREEN OFFSET

MERDE DEDEDEHE DEEE DD DE DEEE DE DEEE DEEE DEEE DE DE DE DD DE DEEE SCR GET LOCATION
OB50 2AC9B1 ld h1, (B1C9)

0OB53 3ACBB1 ld a, (B1CB) High Byte Screen Start)
0B56 C9 ret

Ç

LÉLSL SELS IL LL LS SILLLLILLLLLILILLLLILILLLLLIILI LIT TT) SCR CHAR LIMITS
OB57 CDECOA call OAEC SCR GET MODE

OBSA 011813 ld bc,1318

OBSD D8 ret c

OBSE 0627 id b,27

0B60 C8 ret 2z

0B61 O64F ld b,4F

OB63 C9 ret

EDEME DD HE MED DD DE HE DM DE DE DD HE DE DEEE HE DE DE CHE DE DD DE DE DEEE DEEE SCR CHAR POSITION

OB64 DS push de

OB65 CDECOA call OAEC SCR GET MODE
OB68 0604 1d b,04

OB6A 3805 Jr c,0B71

OB6C 0602 1d b,02

-165-

10FD

Sub

SCREEN PACK

z,0B71

0B81

de, (B1C9)

hl,de
a,h

07

h,a

a, (B1CB)
a,h

h,a

bc

de

a,€

a, a
a, à
a, à
e,a
a,d

(High Byte Screen Start)

-166-

SCREEN PACK

OBJŒE 3C inc a

OB9F 57 ld d,a
OBAO CD640B call OB64 SCR CHAR POSITION
OBA3 AF xor a

OBA4 82 add  a,d
OBAS 10FD djnz OBA4
OBA7 57 ld d,a
OBA8 C9 ret

DD DEN DE DD DE DD DE DD DE DE DE DE DD DE DD DD DE D DE DE DU DE DE DE DE DEHE HD DE DD DE DE DE DE DE SCR DOT POSITION
OBA9 DS push de
OBAA EB ex de,hl
OBAB 21C700 ld h1,00C7
OBAE B7 or a

OBAF ED52 sbc hl,de
OBB1 7D ld a, |
OBB2 E607 and 07

OBB4 87 add  à,a
OBB5 87 add  a,a
OBB6 87 add a.a
OBB7 4F ld c,a
OBB8 7D ld a, l
OBB9 E6F8 and F8
OBBB 6F ld 1,a
OBBC 54 ld d,h
OBBD 5D ld e, 1
OBBE 29 add hlhl
OBBF 29 add hl,hl
OBCO 19 add hl,de
OBC1 29 add hl,hl
OBC2 D1 pop de
OBC3 CDECOA call OAEC SCR GET MODE
OBC6 0601 ld b,01
OBC8 3806 Jr c,0BDO
OBCA 0603 ld b,03
OBCC 2802 jr Z, OEDO
OBCE 0607 ld b,07
OBDO 78 ld a,b
OBD1 A3 and e

OBD2 F5 push af

-167-

OBD3
OBD4
OBD5
OBD7
OBD9
OBDA
OBDC
OBDD
OBE1
OBE2
OBE3
OBES
OBE6
OBE9
OBEA
OBEB
OBEC
OBED
OBEE
OBFO
OBF1
OBF4
OBF5
OBF6
OBF7
OBF8

78

OF
CB3A
CB1B
OF
38F9
19
ED5BC9B1
19

7C
E607
67
3ACBB1
84

81

67

F1

ES
1600
5F
21CFB1
19

LE

EB

ET

C9

ld
rrca
srl
rr
rrca
jr
add
ld
add
ld
and
ld
ld
add
add
ld
pop
push
ld
ld
ld
add
Id
ex
POP
ret

SCREEN PACK

a,b

d
€

c,OBD5
hl,de
de, (B1C9)
hl,de
a,h

07

h,a

a, (B1CB)
a,h

a,C

ha

af

hl

d,00
e,a
h1,B1CF
hl,de
c,(hl)
de,hl

hl

High Byte Screen Start)

Cartes bits suivant Mode

HOMME DEDEHE DEEE HE DD HDMI DE DE DEEE HD DEEE DE EH DE DEDÉDE HD MED MEDE DE DE SCR NEXT BYTE

OBF9
OBFA
OBFB
OBFC
OBFD
OBFF
OC00
OC01
DCO3
OCO4

2C
Co
24
7C
E607
co
7C
D608
67
C9

inc
ret
inc
ld

and
ret
ld

sub
ld

ret

1
nz
h
a,h
07
nz
a,h
08
ha

EDEN DD DE DD DE DEEE OH DE DE DD DE DD DEDEDE DEEE DEEE DE DE DEEE DE DE DE SCR PREV BYTE

-168-

SCREEN PACK

0C05 7D id a,l
0C06 2D dec l!I
OC07 B7 or a
0C08 C0 ret nz
0C09 7C 1d a,h
OCOA 25 dec h
OCOB E607 and 07
OCOD CO ret nz
OCOE 7C 1d a,h
OCOF C608 add  a,08
OC11 67 ld h,a
0C12 C9 ret

AE DIE DE DH DD DD DD DÉ DD DEEE DE DE DD DEEE DEEE DE DE DEEE DE DE DE DEEE SCR NEXT LINE

0C13 7C ld ah
OC14 C608 add  a,08
0C16 67 ld ha
0C17 E638 and 38
0C19 CO ret nz
OCIA 7C Id a,h
OC1B D640 sub 40
OC1D 67 ld h,a
OC1E 7D ld a,l
OC1F C650 add  a,50
OC21 6F 1d l,a
OC22 DO ret nc
OC23 24 inc Oh
OC24 7C id ah
OC25 E607 and 07
OC27 CO ret nz
OC28 7C Id a,h
OC29 D608 sub 08
OC2B 67 ld ha
OC2C cg ret

MEOHOHODEME DE HE DEN DH DE DE HE DE HE DEEE DE DE DE DE DE DEEE DE NEED HE DEEE DE DE JDE DE DE JE SCR PREV LINE
OC2D 7C ld ah
OC2E D608 sub 08
0C30 67 ld ha
0C31 E638 and 38

-169-

SCREEN PACK

0C33 FE38 cp 38

OC35 CO ret nz

0C36 7C ld a,h

OC37 C640 add  a,40

0C39 67 ld h,a

OC3A 7D ld a, ]

OC3B D650 sub 50

OC3D 6F ld 1,a

OC3E DO ret nc

OC3F 7C ld a,h

OC40 25 dec h

OC41 E607 and 07

OC43 C0 ret nz

OCu4 7C ld a,h

OC45S C608 add  a,08

OC47 67 ld h,a

OCu8 C9 ret

LELLLLLLLSLLLLSLÉELLLLS SSL LL SL LLLSLSRLLS SSL SSL LLL LS LL) SCR ACCESS
OC49 E603 and 03

OCuB 216B0C ld h1,0C6B SCR PIXELS (FORCE Mode)
OCUE  280F Jr Z, OC5F

OC50 FEO02 cp 02

0C52 21720C ld h1,0C72 XOR Mode
OC55 3808 jr c,OC5F

0C57 21770C ld h1,0C77 AND Mode
OCSA 2803 Jr z,OC5F

OC5C 217D0C ld h1,0C7D OR Mode
OC5F  3EC3 ld a, C3 jp

OC61 32CCB1 ld (B1CC),a (SCR Write Indirection)
OC64 22CDB1 ld (B1CD),h1

OC67 C9 ret

MERDE DEDE DEMO DE DEEE DE DE DEEE DE DD DE DEEE DH DEEE NEED DE DEMO DE DE DE SCR WRITE

0C68 C3CCB1 jp B1CC SCR Write Indirection

MDN DD DEN DE DE MED DD HÉ DEEE HE DU DE DE DEEE D DEEE DE DU EDEENÉDE DENE SCR PIXELS (FORCE Mode)
OC6B 7E ld a, (hl)

OC6C A8 xor D

OC6D B1 or C

-170-

SCREEN PACK

OC6E A9 Xor C

OC6F A8 xor bb

OC70 77 ld (hl),a

0C71 C9 ret
LELLILLLSLLLLSLLLLLSLLSÉLLLLLSLSLLLSS LS LL LIL LSS ESS]
OC72 78 ld a,b

OC73 A1 and c

OC74 AE xor (hl)

OC75 77 ld (h1),a

0OC76 C9 ret
LELLLLLLLLLLLLLLLLS LIL LIL LLSLLSLILL SSL SSL SSL LLS ES]
0C77 79 ld a,C

OC78 2F cpl

0C79 BO or D

OC7A A6 and (hl)

OC7B 77 ld (hl1),a

OC7C C9 ret

MED DE NE DE DE DE DEMO DEEE MEN DE DEEE DE DEHE DE DE DE DE DEEE DEEE DE DEN DD DE DE DE JE
OC7D 78 ld a,b

OC7E A1 and c

OC7F B6 or (h1)

OC80 77 ld (h1),a

0C81 C9 ret

LELLLL LL SL LLS LS LES SI LILI LL LL LL LL ELLE LLLLL LL LLL ZX)
OC82 7E id a, (hl)

0C83 C3ACOC Jp OCAC
LELLÉLSLLLLSÉILLSLLLLLLLLLLILLSLLLLS LL SSSR LLS LL SSL SSL LS)
0C86 C5 push bc

0C87 DS push de

0C88 CDC20C call OCC2

OC8B 5F ld e,a

OC8C 0608 id b,08

OC8E 3ACFB1 ld a, (B1CF)

0C91 4F ld c,a

0C92 CBOB rrc e

-171-

XOR Mode

AND Mode

OR Mode

SCR READ

SCR INK ENCODE

(Cartes bits suivant Mode)

OC94
OC95
0C97
0C99
OC9B
OC9D
OCSE
OC9F

17
CBO9
3802
CBO3
10F5
D1
C1
C9

rla
rrc
jr
ric
djinz
pop
pop
ret

SCREEN PACK

C
c,0C9B
e

0C92
de

bc

LÉLLLLLSL LS LES LL LS LL LS LL SL LL. LS SSL SL LSLLLLS SES) SCR INK DECODE

OCAO
OCAI
OCA2
OCAS
OCA6
OCA7
OCAA
OCAB

OCAC
OCAD
OCBO
OCB1
OCB3
OCB5
OCB7
OCB9
OCBA
OCBC
OCBD
OCCO
OCC1

OCC2
OCC3
OCC6
OCC7
OCC8
OCC9
OCCA

C5
47
3ACFB1
4F
78
CDACOC
c1
c9

D5
110800
OF
CB12
CB09
3802
CB1A
1D
20F4
7A
CDC20C
D1

C9

57
CDECOA
7A

DO

OF

OF
CE00

push
ld
ld
ld
ld
call
pop
ret

push
ld
rrca
rl
rrc
jr
rr
dec
Jr
ld
call
pop
ret

ld
call
id
ret
rrca
rrca
adc

bc

b,a

a, (B1CF)
C,a

a, D
OCAC

bc

de
de, 0008

,0CB9

vanne

nz, OCBO
a,d
OCC2

de

d,a
OAEC
a,d
nc

a, 00

(Cartes bits suivant Mode)

SCR GET MODE

=172>

SCREEN PACK

OCCC OF rrca

OCCD 9F sbc a,a
OCCE E606 and 06
OCDO AA xor  d
OCD1 C9 ret

LÉLLELLLI LE ELLES LL LL LS LILL LL LL LL LLE LE LLSLLLEZE SZ) Reset couleurs

OCD2 214D10 id h1,104D Couleurs par défaut

OCD5 11D9B1 ld de,B1D9 mémoire couleur 2èmes couleurs
OCD8 012200 1d bc, 0022

OCDB EDBO ldir

OCDD AF xor a

OCDE 32FBB1 id (B1FB),a (Flag jeu de couleurs act.)
OCET 210404 1d h1,0A0A (OA0A )=9900

LÉLLEL LL LL LE LL ELLE LIL LL LLLE SSL LISLLSLSSLLSLILSLLLLSSSS SCR SET FLASHING

OCEH 22D7B1 ld (B1D/7),hl (Flash Periods)
OCE7 C9 ret

HE MEHEHE HN HD DE DH DEEE DD DD DE DEN DH DEÉNEDEDEDEEDENEE SCR GET FLASHING

OCE8 2AD7B1 id hl,(B1D7) (Flash Periods)
OCEB C9 ret

CELLES SSL EL LL LS LL LL SSL LS LLELLLL LL LEZ LS SZ LL) SCR SET INK

OCEC E60F and OF
OCEE 3C + inc à
OCEF 1801 Jr OCF2 Set Colour

ROROHOHOHOMONHEONOHEHEEHEDEMEDE DÉMO DEDÉHE ED DÉHEHEDEHE DE DE DEEE DENE SCR SET BORDER

OCF1 AF xor a

HOHOMOMOMHHHMOHME DE HE OHE DD DEEE DD DÉMO DE D HÉDEDDEDE HE DE DD DE DE DE DONNE Set Colour

OCF2 SF ld e,a

OCF3 78 id a,b

OCF4  CDOAOD call ODOA Aller chercher entrée matrice
couleurs

OCF7 46 ld b,(h1)

OCF8 79 Id a,C

OCF9 CDOAOD call ODOA Aller chercher entrée matrice
couleurs

=173-

SCREEN PACK

OCFC 4E id c,(hl)

OCFD 7B id a,e

OCFE CD2FOD call OD?F aller chercher adr. Ink
0D01 71 ld (h1),c

OD02 EB ex de,hl

0D03 70 ld (h1),b

ODO4 3EFF 1d a, FF

0D06 32FCB1 ld (B1FC),a

OD09 C9 ret

PE % MX A] ler chercher entrée matrice couleurs
ODOA E61F and 1F

ODOC C693 add  a,93
ODOE 6F 1d 1,a
ODOF CEOD adc  a,0D
0D11 95 sub !
0D12 67 ld ha
0D13 C9 ret

DEEE DE DEDE HE HE HE DE DE DH DD DHEHE HE DD DEEE DE DE DE DEDE DE DEHEDEHE DD DE DEEE SCR GET INK

OD14 E60F and OF
0D16 3C inc a
0D17 1801 jr OD1A Get Colour

DEDEDENE DE DE EH HEDEHE HE DH ED DE DEDE DH HE DEEE DE DEEE DEEE DEEE DEEE DEN SCR GET BORDER

0D19 AF xor à

HEDEHDEHEHE DE DD DD DEDE DEEE HD DEEE DE CHE ÉD DEDEDEDE EH CHE DEEE DE DD DEN DE DEN Get Colour

OD1A CD2FOD call OD?F aller chercher adr. Ink
OD1D 1A ld a, (de)

ODIE SE id e,(hl)

OD1F CD240D call OD24

OD22 41 id b,c

0D23 7/B ld a,e

OD24 0E00 ld c,00

0D26 21930D ld h1,0D93 Matrice couleur
0OD29 BE cp (h1)

OD2A C8 ret 2Zz

OD2B 23 inc hi

OD2C OC inc  c

-174-

OD2D
Ç

18FA

jr

SCREEN PACK

0D29

HOHHUEUE DEN DD DEN HEHEHE DE DEDE DH DEN ED DE DEEE DE EE aller chercher adr, Ink

OD2F
0D30
0D32
OD35
0D36
OD37
OD3A
OD3B

OD3C
OD3F
OD4O
OD43
OD46
OD49
OD4B
OD4C

OD4F
OD52
OD55

OD58

5F
1600
21EAB1
19

EB
21EFFF
19

C9

21FEB1
E5
CD7001
CD6DOD
115B0D
0681
E1
C36301

21FEB1
CD7001
CD810D

C38607

Id
id
1d
add
ex
ld
add
ret

ld
push
call
call
ld
ld
pop
jp

id
call
call

JP

e,à

d,00

h1,B1EA mémoire couleur 1ères couleurs
hl,de

de,hl

h1,FFEF

hl,de

h1,B1FE Event Block: Set Inks
hl

0170 KL DEL FRAME FLY

0D6D Flash Inks

de, OD5B Set Inks on Frame Fly
b,81

hi

0163 KL NEW FRAME FLY

hl,B1FE Event Block: Set Inks

0170 KL DEL FRAME FLY

0D81 aller chercher params de Jeu
de couleurs actuel

0786 MC CLEAR INKS

DE DE DE DE DE DE DE DD DE DE DD DH DE DE DE DEEE DEEE DE DD DEDEDEDE DH DEDEHE DE DE DE DE DE DE Set Inks on Frame Fly

OD5B
ODSE
OD5F
0D61
0D62
0D63
OD64
0D65

0D68
OD6B

21FDB1
35
280€
2B

7E

B7

C8
CD810D

CD9907
180F

id
dec
jr
dec
ld
or
ret
call

call
jr

h1,B1FD curr, Flash Period
(hl)

z,OD6D Flash Inks

hl

a, (hl)

a

Z

0D81 aller chercher params de jeu
de couleurs actuel

0799 MC SET INKS

OD7C

-175-

SCREEN PACK

Lis SSL SL LL SSL LL SSL LL LL LL LL SSL LL LL LL ill: )L LL): Flash Inks

OD6D CD810D call OD81 aller chercher params de Jeu
de couleurs actuel

OD70 32FDB1 ld (B1FD),a (curr, Flash Period)

OD73 CD9907 call 0799 MC SET INKS

0D76 21FBB1 ld h1,B1FB Flag jeu de couleurs act.

OD79 7E ld a, (hl)

OD7A 2F cpl

OD7B 77 ld (hl),a

OD7C AF Xor a

OD7D 32FCB1 ld (B1FC),a

OD80 c9 ret

OOOONOUEREEEEEEEEX Aller chercher params de Jeu de couleurs actuel

0OD81 11EAB1 ld de,B1EA mémoire couleurs 1ères couleurs

OD84 3AFBB1 id a, (B1FB) (Flag Jeu de couleurs act.)

OD87 B7 or a

OD88 3AD8B1 ld a, (B1D8) (Flash Period 1.Colour)

OD8B C8 ret 2z

OD8C 11D9B1 ld de, B1D9 Mémoire couleurs 2èmes couleurs

OD8F 3AD7B1 ld a, (B1D7) (Flash Periods)

0D92 C9 ret

LELLLELL LL LEZ LL LL LL LE LL LL LL LL LL LL LL LL LL) LL. :] Matrice de couleurs

0D93 14 04 15 1C 18 1D OC 05
OD9B OD 16 06 17 1E 00 1F CE
ODA3 07 OF 12 02 13 14 19 1B
ODAB OA 03 OB 01 08 09 10 11

MH DEEE DEODE HE DD DEEE DEEE DEEE DE DD DE DEEE DE DE DE DEEE ED EEE SCR FILL BOX

ODB3 4F ld c,a

ODB4 CD950B call OB95

LÉLLLLLL LL LIL LL LL LL LLLILLELLLLILLLLLLLLLLL LIL LL Z) SCR FLOOD BOX
ODB7 ES push hl

ODB8 7

A ld a,d

ODB9 CDES80E call OEES8

ODBC 3009 jr nc, ODC7

ODBE 42 ld b,d

-176-

SCREEN PACK

ODBF 71 ld (h1),c

ODCO CDF90B call OBF9 SCR NEXT BYTE
ODC3 10FA djnz ODBF

ODC5 1810 Jr ODD7

ODC7 C5 push bc

ODC8 D5 push de

ODC9 71 ld (h1),c

ODCA 15 dec d

ODCB 2808 Jr Z,0DD5

ODCD 4A ld c,d

ODCE 0600 ld b,00

ODDO 54 ld d,h

ODD1 5D ld e,l

ODD2 13 inc de

ODD3 EDBO ldir

ODD5 D1 pop de

ODD6 C1 pop bc

ODD7 E1 pop hl

ODD8 CD130C call 0C13 SCR NEXT LINE
ODDB 1D dec e

ODDC 20D9 jr nz,ODB7 SCR FLOOD BOX
ODDE C9 ret

PTTTIIIILILILILILLLLLS IIS LILI S ILE LIL LS LS); SCR CHAR INVERT

ODDF 78 ld a,b

ODEO A9 xor C

ODE1 4F ld c,a

ODE2 CD640B call OB64 SCR CHAR POSITION
ODES 1608 ld d,08

ODE7 ES push hl

ODE8 C5 push bc

ODE9 7E ld a, (h1)

ODEA A9 XOr €

ODEB 77 ld (hl),a

ODEC CDF90B call OBF9 SCR NEXT BYTE
ODEF 10F8 djnz ODE9

ODF1 C1 pop bc

MEME DD DD DEEE DE DD EH DE DD DÉDUDEDEDÉDEDÉDEDEDÉDEEDEE DEHE DEDÉOEDEDEDE DE DE DE Adresser mémoire couleurs

ODF2 E] pop hl

-177-

ODF3
ODF6
ODF7
ODF9

CD130C
15
20EE
C9

call
dec
Jr
ret

SCREEN PACK

0013 SCR NEXT LINE
d
nz, ODE7

LL RSS SEL LES LL LL SL.) LELL LL LE LL LIL LL LL LL: SCR HW ROLL

ODFA
ODFB
ODFC
ODFF
0E01
0EO4

0E05
0E08
0E09
OEOA
OEOC
OEOF
0E12
0E15
0E17
0E19
0E1C
0E1F
0E22
OE24
0E27
0E28
0E29
0E2B
OE2C
0E2F
0E30
0E31
0E32
0E34

0E37
OE3A
0E3B

4F

C5
11D0FF
0630
CD240E
C1

CDBAO7
78

B7
200D
11B0FF
CD370E
110000
0620
180B
115000
CD370E
11B0FF
0620
2AC9B1
19

7C
E607
67
3ACBB1
84

67

50
1E08
C3B70D

2AC9B1
19
C33C0B

ld
push
ld
id
call
pop

call
ld
or
Jr
ld
call
ld
ld
Jr
ld
call
ld
ld
ld
add
ld
and
ld
ld
add
ld
ld
ld
jp

ld
add
jp

c,a
bc

de, FFDO

b,30

0E24

be

07BA MC WAIT FLYBACK
a,b

a

nz,0E19

de, FFBO

0E37

de, 0000

b,20

CE24

de, 0050

0E37

de, FFBO

b, 20

h1, (B1C9)

h1, de

a,h

07

h,a

a, (B1CB) (High Byte Screen Start)
a,h

ha

d,b

e,08

ODB7 SCR FLOOD BOX

h1, (B1C9)

hl,de
OB3C SCR SET OFFSET

—178-

SCREEN PACK

ED DE DEEE DD DD DD DE DE DEEE DE DE DE DE EH HEO DEEEHE HE EHE DD DE DE DE DE DEEE JE SCR SW ROLL

OE3E F5 push af

0E3F 78 ld a,b

0E4O B7 or à

OE41 2830 Jr Z,0E73

0E43 ES push hl

0E44 CD950B call OB95

0E47 E3 ex (sp),hl

0E48 2C inc 1

OE49 CD640B call OB64 SCR CHAR POSITION
OEUC UA ld c,d

OE4D 7B ld a,e

OEUE  D608 sub 08

OES0O 47 ld b,a

OE51 2817 Jr Z,0E6A

0E53 D1 pop de

OE54 CDBAO7 call O07BA MC WAIT FLYBACK
0E57 C5 push bc

0E58 ES push hl

0E59 DS push de

OESA CDAUOE call OEA4

OESD Eli pop hl

OESE CD130C call (0C13 SCR NEXT LINE
0E61 EB ex de,hl

0E62 E1 pop hl

0E63 CD130C call 0C13 SCR NEXT LINE
0E66 C1 pop bc

0E67 10EE djnz OE57

0E69 DS push de

OE6A E1 pop hl

0E6B 51 ld d,c

0E6C 1E08 ld e,08

OE6E F1 pop af

0E6F 4F ld c,a

0E70 C3B70D Jp ODB7 SCR FLOOD BOX
0E73 ES push hl

OE74 DS push de

0E75 CD950B call OB95

0E78 4A ld c.d

-179-

0E79
OE7A
OE7C
OE7D
OE7E
OE7F
0E81
0E82
0E83
0E84
0E85
0E88
0E89
OE8C
OE8D
0E90
0E93
0E94
0E95
0E98
0E99
OE9A
OE9D
OE9E
0E9F
OEAO
OEA2
OEA4
OEAG
OEA9
OEAB
OEAE
OEBO
OEB1
OEB2
OEB3
OEBL
OEB6
OEB7
OEB8

7B
D608
u7

Di

E3
28E9
C5

6B

54

1C
CD640B
EB
CD640B
c1
CDBAO7
CD2D0C
E5

EB
CD2D0C
ES

C5
CDAUOE
c1

Di

Et
10EE
18C6
0600
CDE6OE
3816
CDEGOE
3025
C5

AF

95

UF
EDBO
ci

2F

3C

ld
sub
ld
pop
ex
jr
push
ld
ld
inc
call
ex
call
pop
call
call
push
ex
call
push
push
call
Pop
Pop
pop
djnz
jr
ld
call
jr
call
jr
push
xor
sub
ld
ldir
pop
cpl
inc

SCREEN PACK

a,e
08

b,a

de
(sp),hl
Z,0E6A
bc

Le
dh

e

OB64
de,h]
OB64
bc
07BA
0C2D
hl
de,hl
OC2D
hl

bc
OEA4
bc

de

hl
0E90
0E6A
b,00
OEE6
c,O0EC1
0EE6
nc, OEDS
bc

a

1

c,a

bc

a

SCR CHAR POSITION

SCR CHAR POSITION

MC WAIT FLYBACK
SCR PREV LINE

SCR PREV LINE

—180-

SCREEN PACK

OEB9 81 add a,c
OEBA 4F ld c,a
OEBB 7C ld a,h
OEBC D608 sub 08
OEBE 67 ld h,a
OEBF 1814 jr 0EDS5
OECT CDEGOE call OFE6
OEC4 3812 jr c,0ED8
OEC6 C5 push bc
OEC7 AF xor a
OCEC8 93 sub e
OEC9 4F ld c,a
OECA  EDBO idir”

OECC C1 pop bc
OECD 2F cpl

OECE 3C inc a

OECF 81 add a,c
OEDO 4F ld c,a
OED1 7A ld a,d
OED2 D608 sub 08
OED4 57 ld d,a
OEDS EDBO ldir

OED7 C9 ret

OED8 41 ld b,c
0OED9 7E ld a, (hl)
OEDA 12 ld (de),a
OEDB CDF90B call OBF9 SCR NEXT BYTE
OEDE EB ex de,hl
OEDF  CDF90B call OBF9 SCR NEXT BYTE
OEE2 EB ex de,hl
OEE3 10F4 djnz OED9
OEES5 C9 ret

Çç

OEE6 79 ld a,C
OEE7 EB ex de,hl
OEE8 3D dec a
CEE9 85 add a,1
OEEA DO ret nc
OEEB 7C ld a,h

-181-

OEEC
OEEE
OEFO
OEF1
0EF2

E607
EE07
Co
37
C9

and
xor
ret
scf
ret

SCREEN PACK

07
07
nz

MERDE DEEE MEME DEEE DEN DM DEMI DM MEMEMEME DEEE ÉD DEMO DEN DEEE SCR UNPACK

OEF3
0EF6
OEF8
OEFA
OEFC
OEFF
OFO1

OF02
0F03
OFO4
0F05
0F06
0F08
OFOB
OFOC
OFOE
0F10
0F11
0F12
OF14
0F15
0F16
0F18
0F1B
OF1C

OF20
OF21
0F22
OF24
OF25
0F26

CDECOA
0608
3831
2806
010800
EDBO
C9

LE

23

ES

C5
0604
21CFB1
AF
CB01
3001
B6

23
10F8
12

13
0604
21CFB1
AF
CBO1

3001
B6
23
10F8
12
13
C1

call
ld
Jr
jr
id
idir
ret

ld
inc
push
push
ld
ld
xor
rlc
Jr
or
inc
djnz
ld
inc
ld
1d
xor
ric

jr
or
inc
djnz
ld
inc
pop

OAEC
b,08
c,0F2B
z,0F02
bc,0008

c,(hl)
hl

hl

bc

b,04
h1,B1CF
a

(o
nc,0F11
(hl)

hl

OFOC
(de),a
de

b,04
h1,B1CF
a

C

nc, 0F21
(h1)

hl
OF1C
(de),a
de

bc

SCR GET MODE

Cartes bits suivant Mode

Cartes bits suivant Mode

-182-

OF27
0F28
OF2A

OF2B
OF2C
OF2D
OF2E
OF2F
0F31
0F32
0F35
OF37
0F39
OF3A
0F3B
OF3D
OF3F
OF40
OF41
OF42
OF44
OF45
OF46
OF48

EL
10D8
cg

LE
23
ES
C5
0604
AF
21CFB1
CB01
3001
7E
23
CB01
3001
B6
12
13
10ED
C1
E1
10E3
C9

pop
djnz
ret

ld
inc
push
push
ld
xor
ld
ric
Jr
ld
inc
ric
Jr
or
ld
inc
djnz
pop
pop
djnz
ret

SCREEN PACK

hl
0F02

c,(h1l)
hl

hl

bc

b,04

a
h1,B1CF
(es

nc, OF3A
a,(hl)
hl

C

nc, 0F40
(h1)
(de),a
de

0F31

bc

hl

OF2B

Cartes bits suivant Mode

MED DD DE DD DE DEDE DEEE DE HEC EME ED DE DE SCR REPACK

OF49
OF4A
OF4D
OF50
0F52
OF54
0F56
0F57
OF58
OF59
OF5A
OF5B
OFSE

UF
CD640B
CDECOA
0608
3845
280B
7E

A9

2F

12

13
CD130C
10F6

ld
call
call
ld
ir
jr
ld
xor
cpl
ld
inc
call
djnz

c,a
OB64
OAEC
b, 08
c,0F99
z,0F61
a, (hl)
(à

(de),a
de
0c13
0F56

SCR CHAR POSITION
SCR GET MODE

SCR NEXT LINE

-183-

0F60

0F61
0F62
CF63
OF64
0F65
0F66
0F69
OF6B
OF6C
OF6D
OF6F
OF70
OF72
OF73
OF74
OF75
OF77
OF78
OF7B
OF7C
OF7D
0F80
0F82
0F83
OF84
OF 86
OF87
0F89
OF8A
OF8B
OF8C
OF8E
OF8F
OF90
0F91
0F92
0F93
OF

C9

ES

D5

ES

7E

A9
21CFB1
1604
F5

A6
2001
37
CB13
23

F1

15
20F4
E
CDF90B
7E

A9
21CFB1
1604
F5

A6
2001
37
CB13
23

F1

15
20F4
E1

73

EB

13

E1
CD130C
10C9

ret

push
push
push
id
xor
id
id
push
and
jr
scf
rl
inc
pop
dec
jr
pop
call
ld
xor
ld
ld
push
and
jr
scf
rl
inc
pop
dec
jr
pop
id
ex
inc
pop
call
djnz

SCREEN PACK

hl

de

hl

a, (h1)
C
hli,B1CF
d,04

af

(h1)
nz,0F70

e
hi

af

d
nz,0F6B
hl

OBF9
a,(hl)
C
h1,B1CF
d,04

af

(h1)
nz,0F87

e
h1

af

d
nz,0F82
h1
(hl),e
de,hl
de

hl

0C13
0F61

Cartes bits suivant Mode

SCR NEXT BŸTE

Cartes bits suivant Mode

SCR NEXT LINE

-184-

0F98

0F99
OF9A
OF9B
OF9D
OFJE
OF9F
OFAO
OFAI
OFA4
OFA5
OFA7
OFA8
OFAA
OFAB
OFAC
OFAD
OFAF
OFBO
OFB2
OFB3
OFB6
OFB7
OFB9
OFBA
OFBB
OFBC
OFBD
OFBE
OFC1
OFC3

C9

E5

D5
1604
7E

E5

A9

F5
21CFB1
A6
2001
37
CB13
F1

23

A6
2001
37
CB13
E1
CDF90B
15
20E4
E1

73

EB

13

ET
CD130C
10D6
C9

ret

push
push
ld
ld
push
xor
push
id
and
jr
scf
rl
pop
inc
and
jr
scf
rl
pop
call
dec
jr
pop
ld
ex
inc
pop
call
djnz
ret

SCREEN PACK

hi

de

d,04

a, (hl)
hl

C

af
h1,B1CF
(h1)
nz,OFA8

e
af

hl

(h1)
nz, 0FBO

e

hl
OBF9

d

nz, OF9D
hl
(hl)e
de,hl
de

hl
0C13
0F99

Cartes bits suivant Mode

SCR NEXT BYTE

SCR NEXT LINE

MEDIA DE DD DD DD DE DE DD DE DD DH DE DE DE DD DE DE DE DD DEEE DE ED DE ED DE DE DEEE SCR HORIZONTAL

OFC4
OFC5
OFC6
OFC7
OFC8
OFC9

F5
ES
7A
2F
67
7B

push
push
1d
cpl
ld
Id

af
hl
a,d

h,a
a,e

-185-

SCREEN PACK

OFCA 2F cpl

OFCB 6F ld L,a

OFCC 23 inc hl

OFCD 09 add hl,bc
OFCE 23 inc hl

OFCF E3 ex (sp},hl
OFDO AF xor a

OFD1 93 sub e

OFD2 F5 push af

OFD3 CDA9OB call OBA9 SCR DOT POSITION
OFD6 ES push hl

OFD7 78 ld a,b

OFD8 2F cpl

OFD9 6F ld 1,a

OFDA 26FF ld h,FF

OFDC 2207B2 ld (B207),h1
OFDF El pop hl

OFEO F1 pop af

OFE1 AO and b

OFE2 47 ld b,a

OFE3 2845 jr Z,102A
OFES E3 ex (sp),hl
OFE6 1803 jr OFEB

OFE8 A ld a, (de)
OFE9 B1 or C

OFEA 4F ld c,a

OFEB 2B dec hl

OFEC 7C ld a,h

OFED B5 or 1

OFEE 2834 jr z,1024
OFFO 13 inc de

OFF1  10F5 djnz OFE8
OFF3 EB ex de,hl
OFF4 EI pop hl

OFFS5 F1 pop af

OFF6 47 ld b,a

OFF7 CDES8BD call BDE8 SCR WRITE
OFFA CDF90B call OBF9 SCR NEXT BYTE
OFFD ES push hl

OFFE  2A07B2 ld h1, (B207)

-186-

SCREEN PACK

1001 19 add hl,de

1002 300€ jr nc,1010

1004 EB ex de,hl

1005 EI pop hl

1006 OEFF ld c,FF

1008 CDE8BD call BDE8 SCR WRITE
100B CDF90B call OBF9 SCR NEXT BYTE
100E 18ED jr OFFD

1010 7B ld a,e

1011 B7 or a

1012 280€ jr z,1022

1014 AF xor a

1015 21CFB1 ld h1,B1CF Cartes bits suivant Mode
1018 B6 or (h1)

1019 23 inc  hl

101A 1D dec e

101B 20FB jr nz, 1018

101D 4F ld c,a

101E El pop hl

101F C3ES8BD jp BDE8 SCR WRITE
1022 E1 pop hl

1023 C9 ret

1024 El pop hl

1025 F1 pop af

1026 47 ld b,a

1027 C3E8BD jp BDE8 SCR HRITE
102A D1 pop de

102B F1 pop af

102C 47 ld b,a

102D 18CE jr OFFD

LÉLSSLLELLS ELLE LL LL LS LES LE LL LL LL LL LL LL ll SL) SCR VERT Ï CAL

102F F5 push af

1030 E5 push hl
1031 7C id a,h
1032 2F cpl

1033 67 Id ha

-187-

SCREEN PACK

1034 7D ld a, l

1035 2F cpl

1036 6F ld 1.a

1037 23 inc  hl

1038 09 add hl,bc

1039 23 inc hl

1034 E3 ex (sp),hl

103B CDA9OB call OBA9 SCR DOT POSITION
103€ D1 pop de

103F F1 pop af

1040 47 ld b,a

1041 CDES8BD call BDES8 SCR WRITE
1044  CD2D0C call OC2D SCR PREV LINE
1047 1B dec de

1048 7A ld a,d

1049 B3 or e

104A 20F5 jr nz, 1041

104C C9 ret

CELLES LLL LL SL LL SLI LS SELLE LIL LLLLL LILI LL ELLE LL.) Couleurs par défaut

104D O4 O4 OA 13 OC OB 14 15
1055 OD 06 1E 1F 07 12 19 O4
105D 17 O4 O4 OA 13 OC OB 14
1065 15 OD 06 1E 1F 07 12 19

106D OA 07

106F C7 rst O0
1070 C7 rst O0
1071 C7 rst O0
1072 C7 rst 0
1073 C7 rst O0
1074 C7 rst O0
1075 C7 rst 0
1076 C7 rst O0
1077 C7 rst O0

-188-

2.5.5 TEXT SCREEN (TXT)

Ce pack est responsable de la gestion de textes, ce qui comprend
également l'organisation des fenêtres,

Quelques remarques sont nécessaires en ce qui concerne la manipulation du
curseur :

Les coordonnées réclamées ou fournies par les routines du curseur doivent
etre comprises comme des indications logiques, c'est-à-dire qu'elles se
rapportent à la fenêtre actuelle. Les coordonnées 1,1 correspondent à
l'angle supérieur gauche de la fenêtre. Si vous voulez par exemple
positionner, avec TXT SET CURSOR, le curseur en dehors de la fenêtre, il
sera automatiquement fixé sur la prochaine position possible à
l'intérieur de la fenêtre, si le curseur est activé ou si un caractère
doit être représenté ensuite.

La position actuelle (que vous pouvez lire avec TXT GET CURSOR) est ainsi
également modifiée.

Si le curseur est désactivé, la nouvelle position souhaitée, jusqu’à ce
qu’un caractère soit représenté ou jusqu’à ce que le curseur soit activé.

Deux routines permettent d'activer ou de désactiver le curseur. TXT CUR
ON/OFF est une routine subordonnée à la routine TXT CUR ENABLE/DISABLE.
Cela signifie que le curseur, après ENABLE, ne peut apparaître que s’il a
été également autorisé avec ON.

Voici une routine que nous avons pas évoquée au chapitre 2.3:

TXT OUTPUT amène le caractère qui se trouve dans a sur le fenêtre
actuelle de l'écran ou exécute ce caractère, s’il s’agit d’un caractère
de commande.

Notez que cette routine utilise l’indirection TXT OUT ACTION! Si vous
avez également détourné cette routine, c’est votre routine et non celle
de la Rom qui sera utilisée,

-188-

TEXT SCREEN

REED UE UE UE DENAIN TT INITIALISE

1088 TXT RESET

a

(B295),a

h1,0001

113D TXT fixer param. défaut

10A3 Reset Params (toutes les fenêtres)

PERDUE HUE D UE DD DE EH DE DD D DD DD DD DD EURE ED D D DEEE DE DE DEEE TNT RESET

h1,1091 Restore TXT Indirections

OA8A Move (h1+3)=>((h1+1)),cnt=(h1)
145B

OF 15 octets

BDCD adresse objet

1263 TXT DRAW/UNDRAW CURSOR

1263 TXT DRAW/UNDRAW CURSOR

134A TXT WRITE CHAR

13C0 TXT UNWRITE

140C TXT OUT ACTION

PURE DE AUDE DE DE DIE DDR DE DE UE DE DUREE EEE ER HP OCOT Params (toutes fenêtres)

1078 CD8810 call
107B AF xor
107C 3295B2 ld
107F 210100 ld
1082 CD3D11 call
1085 C3A310 JP
1088 219110 ld
108B CD8A0A call
108€ C35B14 Jp
1091 OF db
1092 CDBD dw
1094 C36312 jp
1097 C36312 jp
109A C34A13 Jp
109) C3C013 Jp
1040 C30C14 Jp
1043 3E08 ld
10A5 110DB2 ld
1048 2185B2 ld
10AB 010F00 ld
10AE EDBO ldir
10B0 3D dec
10B1 20F5 Jr
10B3 320CB2 ld
10B6 C9 ret
10B7 3AOCB2 ld
10BA 4F ld
10BB 0608 ld
10BD 78 ld

a,08

de,B20D Start Params Fenêtre 0
h1,B285 pos. curseur act.(Row,Col)
bc,000F

a
nz, 1048
(B20C),a (fenêtre écran act.)

a, (B20C) (fenêtre écran act.)
c,a

b,08

a,b

-189-

TEXT SCREEN

10BE 3D dec a

10BF CDE810 call 10E8 TXT STR SELECT
10C2 CDDOBD call BDDO TXT UNDRAW CURSOR
10C5 CDC312 call 12C3 TXT GET PAPER
10C8 3290B2 ld (B290),a (TXT Paper act.)
10CB CDBD12 call 12BD TXT GET PEN
10CE  328FB2 ld (B28F),a (TXT Pen act.)
10D1 10EA djnz 10BD

10D3 79 ld a,C

10D4 C9 ret

10D5 4F ld c,a

10D6 0608 id b,08

1018 78 ld a,b

10D9 3D dec a

10DA CDE810 call 10E8 TXT STR SELECT
10DD C5 push bc

10DE 2A8FB2 ld h1,(B28F) (TXT Pen act.)
10E1 CD3D11 call 113D TXT fixer paramètres défaut)
10E4 C1 pop bc

10E5 10F1 djnz 10D8

10E7 79 ld a,c

DD DM DUR EDEN UD DE DEEE DD DD DD DE ED DD DE DE DER D DB DE DD D D D DD DD AE DE DD UR DE ED IRD III T OT STR SELECT

10E8 E607 and 07

10EA 210CB2 ld h1,B20C Fenêtre écran act.

10ED BE cp (h1)

10EE C8 ret 2Zz

10EF C5 push bc

10F0 DS push de

10F1 4E ld c,(h1)

10F2 77 ld (hl1),a

10F3 47 ld b,a

10F4 79 ld a,C

10F5S CD2A11 call 112A Adr. params fenêtre => de
10F8 CD2211 call 1122 idir cnt=15

10FB 78 ld a,b

10FC CD2A11 call 112A Adr, params fenêtre => de
10FF EB ex de,hl

1100 CD2211 call 1122 ldir cnt=15

-190-

TEXT SCREEN

1103 79 ld a,C
1104 D1 pop de
1105 C1 pop bc
1106 C9 ret

DU DD DE DEEE DEEE D DEAD DE DE AE DU D DEEE DE DEEE DESERT CT SWAP STREAMS

1107 3AOCB2 ld a, (B20C) (fenêtre écran act.)

1104 F5 push af

110B 79 ld a,C

110C CDE810 call 10E8 TXT STR SELECT

110F 78 ld a,b

1110 320CB2 ld (B20C),a (fenêtre écran act.)

1113 CD2A11 call 112A Adr. params fenêtre => de
1116 DS push de

1117 79 ld a,c

1118 CD2A11 call 112A Adr, params fenêtre => de
111B El pop hl

111C CD2211 call 1122 ldir cnt=15

111F F1 pop af

1120 18C6 Jr 10E8 TXT STR SELECT

DMDDDEDEDEUEDE DURE ND ED DE DE DD DEEE DE DEEE EEE DE DE TL cnt=15
1122 C5 push bc

1123 010F00 ld bc,000F

1126 EDBO ldir

1128 C1 pop bc

1129 C9 ret

LELLEL ILE ILE LIL LL EL ELLE LL LL); l)l;;;) SL: S) Adr. params fenêtre => de

112A E607 and 07

112C SF ld e,a
112D 87 add  a,a
112E 87 add  a,a
112F 87 add  a,a
1130 87 add  a,a
1131 93 sub e
1132 C60D add  a,0D
1134 SF ld e,a
1135 CEB2 adc a,B2
1137 93 sub e
1138 57 id d,a

-191-

TEXT SCREEN

1139 2185B2 ld h1,B285 pos, curseur act.(Row,Col)
113C C9 ret

PR D DD DD DEEE DEEE DEEE DE DEAD DE DEEE DE DE DDR DEUST OT fixer params défaut

113) EB ex de,hl

113E 3E03 ld a,03

1140 328DB2 ld (B28D),a (flag curseur act.)
1143 7A ld a,d

1144 CDAE12 call 12AE TXT SET PAPER
1147 7B ld a,e

1148 CDA912 call 1249 TXT SET PEN
114B AF xor a

114C CDA713 call 13A7 TXT SET GRAPHIC
114F CD7A13 call 137A TXT SET BACK
1152 210000 ld h1,0000

1155 117F7F ld de, 7F7F

1158 CDOC12 call 120C TXT WIN ENABLE
1158 C35114 Jp 1451 TXT VDU ENABLE

PUR UEDE DEEE DE DE D DE DE DD DD DD ND HE DEAD DA DE DEEE T NT SET COLUMN

115E 3D dec à

115F 2189B2 ld h1,B289 fenêtre act. gauche

1162 86 add  a,(hl)

1163 2A85B2 ld h1,(B285) (Pos. curseur act.(Row,Col))
1166 67 ld ha

1167 180E Jr 1177
LÉLLLLLLELLLLESSILILISLLILILLLLLLLLLIILILL IL: II] TXT SET ROW
1169 3D dec a

116A 2188B2 ld h1,B288 fenêtre act, haut

116D 86 add a,(hl)

116E  2A85B2 ld h1,(B285) (Pos. curseur act.(Row,Col))
1171 6F ld l,a

1172 1803 Jr 1177

LELELLLLL LL LL LL LL SL LLLLLLLILELLELL SSL LLLLLLE EX ZE) TXT SET CURSOR
1174 CD8A11 call 1184 lfd Fenst. haut,gauche+hl
1177 CDDOBD call BDDO TXT UNDRAW CURSOR

1174 2285B2 ld (B285),h1 (Pos. curseur act,(Row,Col))
117D C3CDBD Jp BDCD TXT DRAH CURSOR

-192-

TEXT SCREEN

CTILIILIELIILSILIELILSLSLLI LILI SELS RS ELS EEE LE) TXT GET CURSOR

1180 2A85B2 ld h1,(B285) (Pos. curseur act.(Row,Col))
1183 CD9711 call 1197 fenêtre act. haut,gauche-h1
1186 3A8CB2 ld a,(B28C) (Roll Count act.)

1189 C9 ret

ÉTILILIIIILLLILLLLLILLIELLLLLILISLLS SL SLLLLS ES DLL) fenêtre act. haut, gauche+h1
118A 3A88B2 ld a,(B288) (fenêtre act. haut)

118D 3D dec a

118 85 add  a,1

118F 6F ld 1,a

1190 3A89B2 1d a, (B289) (fenêtre act. gauche)

1193 3D dec a

1194 84 add ah

1195 67 ld h,a

1196 C9 ret

PTTTLIIIILILILIIILIÉELILLLLIISISILLLLRLLSS LEE ES D) fenêtre act. haut, gauche-h1
1197 3A88B2 ld a, (B288) (fenêtre act. haut)

1194 95 sub ]

1198 2F cpl

119C 3C inc a

119D 3C inc a

119 6F ld l,a

119F 3A89B2 ld a,(B289) (fenêtre act. gauche)

1142 94 sub Oh

1143 2F cpl

1144 3C inc a

1145 3C inc a

1146 67 ld ha

11A7 C9 ret

HE MEME DE DE DE DEN DD DE D DE DEN DE DÉHDÉ DE DEEE DEEE D DEEE DE DE DE DEEE move Cursor

1148 CDDOBD call BDDO TXT UNDRAH CURSOR

11AB 2A85B2 ld h1,(B285) (Pos. curseur act.(Row,Col))
11AE CDDA11 call 11DA h1 dans limites fenêtre?
11B1 2285B2 ld (B285),h1 (Pos. curseur act.(Row,Col))
11B4 D8 ret c

11B5 ES push hl

11B6 218CB2 ld h1,B28C Act. Roll Count

-193-

TEXT SCREEN

11B9 78 ld a,b

11BA 87 add  à,a

11BB 3C inc a

11BC 86 add  a,(hl)

11BD 77 ld (h1),a

11BE CD5612 call 1256 TXT GET WINDOW

11C1 3A90B2 ld a, (B290) (TXT act. Paper)

11C4 F5 push af

11C5 DC3EOE call c,0E3E SCR SW ROLL

11C8 F1 pop af

11C9 D4FAOD call nc,ODFA  SCR HW ROLL

11CC E1 pop hi

11CD C9 ret
LÉELLLLLELLILILILILLLLLLLLLLLLLLLLLLLLLLLLLISILLLS. TXT VALIDATE
11CE CD8A11 call 118A fenêtre act. haut,gaucherhl
11D1 CDDA11 call 11DA hl dans limites fenêtre?
11D4 F5 push af

11D5 CD9711 call 1197 fenêtre act. haut,gauche-hl
11D8 F1 pop af

11D9 C9 ret

LELLLLLLLLL LIL LLLLLL LL LL LI LLLLLL LL LLLLLI LILI LLLL LEZ) hl dans limites fenêtre?
11DA 3A8BB2 ld a, (B28B) (fenêtre act. droite)
11DD BC cp h

11DE F2E611 jp p,11E6

11E1 3A89B2 ld a, (B289) (fenêtre act. gauche)
11E4 67 ld h,a

11E5 2C inc !

11E6 3A89B2 ld a, (B289) (fenêtre act. gauche)
11E9 3D dec a

11EA BC Cp h

11EB FAF311 jp m,11F3

11EE 3A8BB2 ld a, (B28B) (fenêtre act. droite)
11F1 67 id h,a

11F2 2D dec |]!

11F3 3A88B2 ld a, (B288) (fenêtre act. haut)
11F6 3D dec a

11F7 BD CP l

11F8 F20612 Jp p, 1206

-194-

TEXT SCREEN

11FB 3A8AB2 ld a, (B28A) (fenêtre act. bas)
11FE BD cp 1

11FF 37 scf

1200 FO ret p

1201 6F ld l,a

1202 O6FF ld b,FF

1204 B7 or a

1205 C9 ret

1206 3C inc a

1207 6F ld l,a

1208 0600 ld b, 00

1204 B7 or a

120B C9 ret
LILLILISLILSLSLLSLLLLILLLLLLILILLLLLLLLLILLLLLLLL2. TXT WIN ENABLE
120C CD570B call OB57 SCR CHAR LIMITS
120F 7C ld a,h

1210 CD4412 call 1244

1213 67 ld ha

1214 7A ld a,d

1215 CD4412 call 1244

1218 57 ld d,a

1219 BC cp h

1214 3002 Jr nc, 121E

1210 54 ld d,h

121D 67 ld h,a

121E 7D ld a, 1

121F CD4D12 call 124D

1222 6F ld 1,a

1223 7B ld a,e

1224 CD4D12 call 124D

1227 5F ld e,a

1228 BD Cp l

1229 3002 Jr nc, 122D

122B 5D ld e,l

122C 6F ld 1,a

122D 2288B2 ld (B288),h1 (fenêtre act. haut)
1230 ED538AB2 ld (B28A),de (fenêtre act, bas)
1234 7C ld a,h

-195-

TEXT SCREEN

1235 B5 or l

1236 2006 jr nz,123E

1238 7A ld a,d

1239 A8 xor bb

123A 2002 Jr nz, 123€

123C 7B ld a,e

123D A9 Xxor cc

123E 3287B2 ld (B287),a (Flag fenêtre (0=écran entier))
1241 C37711 jp 1177

1244 B7 or a

1245 F24912 Jp p,1249

1248 AF "xor a

1249 B8 cp b

124A D8 ret c

124B 78 ld a,b

124C C9 ret

124D B7 or a

124E F25212 Jp p, 1252

1251 AF xor a

1252 B9 cp c

1253 D8 ret c

1254 79 ld a,c

1255 C9 ret

LRLRLL ELLE LLLSILLLLLLLLLLLILLLLLILLLLI LILI III IS.) TXT GET WINDOW
1256 2A88B2 ld h1, (B288) (fenêtre act. haut)

1259 EDSB8AB2 ld de, (B28A) (fenêtre act. bas)

125D 3A87B2 ld a,(B287) (flag fenêtre (0=écran entier))
1260 C6FF add  a,FF

1262 C9 ret

HENNEEMEDHEDEHE DE DH DE DEEE DE DE DE DE DE DÉ DE DE DE DE DE DE DE DE DE DE DE DE DE DEEE HD DE TXT DRAW/UNDRAW CURSOR

1263 3A8DB2 ld a,(B28D) (act, Cursor Flag)
1266 B7 or a
1267 CO ret nz

SOI DEN DD DE DE DEN DE ÉD DE DH DE DE DE DD DE DE D DEEE DE DE DE DE DE DE DE DE DJ JE DEEE TXT PLACE/REMOVE CURSOR

1268 C5 push bc

—196-

TEXT SCREEN

1269 DS push de

1264 E5 push hl

126B CDAB11 call 11AB

126E ED4B8FB2 ld bc, (B28F) (TXT act. Pen)
1272 CDDFOD call ODDF SCR CHAR INVERT
1275 El pop hl

1276 D1 pop de

1277 C1 pop bc

1278 C9 ret

DHEMEHMEMHDE HE DE DD DD DE DEEE DE DE HE HD DD DE DEEE DEEE DEEE DE DE DEEE TXT CUR ON

1279 F5 push af

127A 3EFD ld a,FD

127C CD8B12 call 128B Cur Enable Cont'd
127F F1 pop af

1280 C9 ret

DEN DED DD DE DEHME DE ED DE DD DD DEN DE DE DE DEEE DEEE DEEE DE DEEE DE DE DE DE DE DEN TXT CUR OFF

1281 F5 push af

1282 3E02 ld a, 02

1284 CD9C12 call 129C Cur Disable Cont'’d
1287 F1 pop af

1288 C9 ret

DU NE DEN DE DH DE DE DD HE DE DE CHE HE DEEE DD DEEE DD HE DEEE DEN DEEE DEN DEN TXT CUR ENABLE

1289 3EFE ld a, FE

HD DE DEEE DEMO HN DE HD HN DE DE DEN DEEE DDE DEEE DEEE DE DE DEEE DEMI DD EDEN Cur Enable Cont'd

128B F5 push af

128C CDDOBD call BDDO TXT UNDRAW CURSOR
128F F1 pop af

1290 E5 push hl

1291 218DB2 ld h1,B28D act. Cursor Flag
1294 A6 and (hl)

1295 77 ld (h1),a

1296 El pop hl

1297 C3CDBD Jp BDCD TXT DRAW CURSOR

RERONEDEHE DIEM HD HE HDEDEH NE DE DE DE DÉDEDEDEDE DEEE NEED DEEE EDEN TXT CUR DISABLE

129A 3E01 ld a,01

—197-

TEXT SCREEN

DEDEDEDE DE D DED DE NEED HE DD HE DE DE DE DE DE DE DD DEN HD DE DD DE DEEE DIE EE DE DE DE DE ED Cur Disable Cont ’ d0

129C F5 push af

129D CDDOBD call BDDO TXT UNDRAW CURSOR
1240 F1 pop af

12A1 E5 push hl

1242 218DB2 id h1,B28D act. Cursor Flag
12A5 B6 or (hl)

1246 77 ld (hl1),a

12A7 E1 pop hl

1248 C9 ret

LL SSL LL IL LE LE LL LI LLELLLLLLL LL LL LL EL LLLEL LL LS] TXT SET PEN

12A9 218FB2 id h1,B28F TXT act. Pen

12AC 1803 Jr 12B1

ADDED D DD DEDEDE DE DD DEEE DETTE ED EEE TUT SET PAPER
12AE 2190B2 ld h1,B290 TXT act. Paper

12B1 F5 push af

12B2 CDDOBD call BDDO TXT UNDRAW CURSOR

12B5 F1 pop af

12B6 CD860C call O0C86 SCR INK ENCODE

12B9 77 id (hl1),a

12BA C3CDBD Jp BDCD TXT DRAW CURSOR

D DDR DETTES DEEE DEEE DE DE D DEEE TXT GET PEN
12BD 3A8FB2 ld a, (B28F) (TXT act. Pen)

12C0 C3AOOC jp OCAO SCR INK DECODE

HD DD DD DD DEAD DD DD DEEE DOTE EE TUT GET PAPER
12C3 3A90B2 ld a, (B290) (TXT act, Paper)

12C6 C3A0OC jp OCA0 SCR INK DECODE

RD D NID DE DE DEEE DE DEEE DEEE DEEE TT INVERSE
12C9 2A8FB2 Id h1,(B28F) (TXT act. Pen)

12CC 7C ld a,h

12CD 65 ld h,1

12CE 6F ld la

12CF 228FB2 ld (B28F),h1 (TXT act, Pen)

12D2 C9 ret

—198-

TEXT SCREEN

LES RS LL EL SSL LL LS SSL LL LS LL LL LL SLI LL LL LL LL IX.) TXT GET MATRIX

12D3 D5 push de

12D4 5F ld e,a

12D5 CD2A13 call 132A TXT GET M TABLE
12D8 3009 Jr nc, 12E3

12DA 57 ld d,a

12DB 7B ld a,e

12DC 92 sub d

12DD 3F ccf

12DE 3003 Jr nc, 12E3

12E0 5F ld e,a

12E1 1803 jr 12E6

12E3 210038 ld h1,3800

126 F5 push af

12E7 1600 ld d,00

12E9 EB ex de,hl

12EA 29 add hLhl

12EB 29 add hLhl

12EC 29 add hlh]l

12ED 19 add hl,de

12EE F1 pop af

12EF D1 pop de

12F0 C9 ret

LELLLLLLLLELL LL EL LLLLL LL LL LLLLLLL LL LL LES SL LS LSS SSL: TXT SET MATRIX
12F1 EB ex de,hl

12F2 CDD312 call 12D3 TXT GET MATRIX
12F5 DO ret nc

12F6 EB ex de,hl

12F7 010800 ld bc, 0008

12FA  EDBO ldir

12FC C9 ret

LES S SSL SSL SL SELS LL L LL SELS LL SL LL SL LL LLLILLLL EL LS LL LL) TXT SET M TABLE

12FD ES push hl

12FE 7A ld a,d
12FF B7 or a

1300 1600 ld d,00
1302 2019 Jr nz,131D
1304 15 dec d

—199-

1305
1306
1307
1308
1309
130€
130)
130E
1310
1311
1312
1314
1315
1318
1319
131A
131€
131D
1320
1324
1325
1329

D5

4B

EB

79
CDD312
7C

AA
2004
7D

AB
2808
C5
CDF712
C1

OC
20EC
D1
CD2A13
ED5394B2
D1
ED5396B2
cg

TEXT SCREEN

push de

ld c,e

ex de,hl

ld a,C

call 12D3 TXT GET MATRIX
id a,h

xor  d

Jr nz,1314

ld a, 1

xor e

Jr z,131C

push bc

call 12F7

pop bc

inc c

Jr nz, 1308

pop de

call 132A TXT GET M TABLE

ld (B294),de (1er Caractère User Matrix)
pop de

ld (B29%6),de (Adr. User Matrix)

ret

CILILEL ITS SL EL SIL SI SE LL LL LL) ELLE) LL) LL, ));),,,,:, JET GET M TABLE

132A
132D
132E
132F
1330
1333

2A94B2
7C
OF
7D
2A96B2
C9

ld h1,(B294) (ler Caractère User Matrix)
id a,h

rrca

ld a, l

ld h1, (B296) (Adr, User Matrix)

ret

MOHHENE DE HE HEOHE DE DE DE DEEE DE DE DE DEEE DE HUE EEE DEEE DE DEEE DE DD EEE TXT ‘WR CHAR

1334
1335
1338
1339
133A
133B
133E

47
3A8EB2
B7
C8
C5
CDA811
24

ld b,a

ld a,(B28E) (VDU Flag (O-disabled))
or a

ret 2z

push bc

call 1148 move Cursor

inc h

—200-

TEXT SCREEN

133F  2285B2 ld (B285),h1 (Pos. curseur act.(Row, Col))
1342 25 dec h

1343 F1 pop af

1344  CDD3BD call BDD3 TXT WRITE CHAR
1347  C3CDBD Jp BDCD TXT DRAW CURSOR
LLLLLLLLLLEE SI LED E SSL LL LLILLLLLILLLLILLILILLIILILILS:] TXT WRITE CHAR
134A ES push hl

134B CDD312 call 12D3 TXT GET MATRIX
134E 1198B2 id de, B298

1351 D5 push de

1352 CDF30E call OEF3 SCR UNPACK

1355 D1 pop de

1356 El pop hl

1357 CD640B call OB64 SCR CHAR POSITION
135A 0E08 ld c,08

135C C5 push bc

135) ES push hl

135€ C5 push bc

135F DS push de

1360 EB ex de,hl

1361 4E id c,(hl)

1362 CD/7613 call 1376

1365 CDF90B call OBF9 SCR NEXT BYTE
1368 D1 pop de

1369 13 inc de

136A C1 pop bc

136B 10F1 djnz 135E

136D El pop hi

136E CD130C call 0C13 SCR NEXT LINE
1371 C1 pop bc

1372 OD dec c

1373 20E7 jr nz,135C

1375 C9 ret

1376 2A91B2 ld h1,(B291) (act. Background Mode)
1379 E9 jp (h1)

HER HDEENE HE HD DE HE DE DE DEDE DEEE HD DE DE DEDE DE DE HE DE DE DE DE DE DE DE DE TXT SET BACK

137A

219113

1d

h1,1391

-201-

137D
137E
1380
1383
1386

B7
2803
219F13
2291B2
C9

or
Jr
ld
ld
ret

TEXT SCREEN

a

Z,1383

h1,139F

(B291),h1 (act. Background Mode)

DE HER DE DE DEDE MED DD DE DEDE DE DE DE DE DEEE DIE DEEE DE DE DEEE DEDE DE DEEE DE DE DEEE DE TXT GET BACK

1387
138A
138D
138E
138F
1390

1391
1394
1395
1396
1397
1398
1399
139A
139B
139D
139F
13A2
13A3
13A4

2A91B2
116FEC
19
7C
B5
C9

2A8FB2
79

2F

AU

47

79

A5

BO
OEFF
1803
3A8FB2
u7

EB
C36BOC

ld
id
add
id
or
ret

1d
ld
cpl
and
id
ld
and
or
ld
Jr
ld
id
ex
jp

h1,(B291) (act. Background Mode)
de,EC6F

hl,de

a,h

1

h1,(B28F) (TXT act, Pen)
a,C

h

b,a

a, C

1

b

c,FF

13A2

a,(B28F) (TXT act. Pen)
b,a

de,h]

OC6B SCR PIXELS

PETETILIIIILIIILIILILIIILLILLLLISL SL LIL SLL LS SL SL LS.) TXT SET GRAPHIC

1347
13AA

3293B2
C9

id
ret

(B293),a (GRA CHAR WR Mode (0=disabl))

JDE EH DH DE DE DEEE HE HE DE DE DE DE DE DE DE HE DIE DEEE DE DE HE DEDE DIE DEEE DEEE TXT RD CHAR

13AB
13AC
13AD
13AE
13B1
13B4

E5
D5
C5
CDDOBD
2A85B2
CDD6BD

push
push
push
cal]
id

call

hl

de

bc

BDDO TXT UNDRAW CURSOR

h1,(B285) (Pos. curseur act,(Row, Col))
BDD6 TXT UNWRITE

-202-

TEXT SCREEN

13B7 F5 push af

13B8 CDCDBD call BDCD TXT DRAW CURSOR
13BB F1 pop af

13BC C1 pop bc

13BD D1 pop de

13BE El pop hl

13BF C9 ret

LÉLLLLSLLL LIL LLLLLLELLLLLL LL LS LLLLLLLSLLLLLESS LS) TXT UNWRITE
13C0 3A8FB2 ld a,(B28F) (TXT act, Pen)
13C3 1198B2 ld de,B298

13C6 ES push hl

1307 D5 push de

13C8 CD490F call O0F49 SCR REPACK
13CB CDE313 call 13E3

13CE D1 pop de

13CF E1 pop hl

13D0 3001 Jr nc, 13D3

13D2 CO ret nz

13D3 3A90B2 ld a, (B290) (TXT act. Paper)
13D6 DS push de

13D7 CD490F call OF49 SCR REPACK
13DA Di pop de

13DB 0608 ld b,08

13DD 1A ld a, (de)

13DE 2F cpl

13DF 12 ld (de),a

13E0 13 inc de

13E1 10FA dJnz 13DD

13E3 0E00 ld c,00

13E5 79 ld a,C

13E6 CDD312 call 12D3 TXT GET MATRIX
13E9 1198B2 id de,B298

13EC 0608 id b,08

13EE 1A ld a, (de)

13EF BE cp (h1)

13F0 2009 Jr nz, 13FB

13F2 23 inc hl

13F3 13 inc de

13F4  10F8 dJnz 13EE

-203-

13F6
13F7
13F9
13FA

13FB
13FC
13FE
13FF

79
FE20
37
C9

oc
20E7
AF
C9

ld
cp
scf
ret

inc
Jr

xor
ret

TEXT SCREEN

a, €
20

C
nz, 13E5
a

LRLÉLS SELS Li LS Si iii ss is il LL ii LL LL. LL, LE EX] TXT OUTPUT

1400
1401
1402
1403
1404
1407
1408
1409
140A
140B

F5
C5
D5
E5
CDD9BD
E1
D1
C1
F1
C9

push
pusfi
push
push
call
pop
pop
pop
pop
ret

af
bc
de
hl
BDD9
hl
de
bc
af

TXT OUT ACTION

ELLES LE LS LES LL LL LLLL LL SLI ELLE LLLLLELLLLLLLLL LL ZX) TXT OUT ACTION

140C
140D
1410
1411
1412
1415
1418
1419
141A
141C
14E
141F
1421
1422
1424
1427
1428

UF
3A93B2
B7

79
C24519
21B8B2
46

78
FEOA
3028
B7
2006
79
FE20
D23413
04

70

id
ld
or
ld
Jp
ld
ld
ld
cp
Jr
or
ir
ld
cp
JP
inc
ld

c,a

a, (B293)
a

a,c

nz, 1945
h1,B2B8
b, (h1)
a,b

OA

nc, 1446
a

nz, 1427
a, C

20

nc, 1334
b
(h1),b

(GRA CHAR WR Mode (0=disabl))

GRA WR CHAR
Compteur de caractères Control Buffer

Control Buffer plein ?
oui =>
vide ?
non =>

caractère de commande?

non => TXT WR CHAR
Compteur +1

-204-

1429
142A
142C
142D
142E
1431
1432
1435
1436
1437
1438
1439
143A
143B
143C
143D
143E
143F
1442
1443
1446
1447
T4HA

58
1600
19

71
3AB9B2
5F
21C3B2
19

19

19

7E

B8

DO

23

SE

23

56
21B9B2
79
CD1600
AF
32B8B2
C9

ld
ld
add
ld
ld
ld
ld
add
add
add
ld
cp
ret
inc
ld
inc
ld
ld
ld
call
xor
ld
ret

TEXT

e,b
d,00
hl,de
(h1),c
a, (B2B9)
e,a
h1,B2C3
hl,de
hl,de
hl,de
a,(hl)
b

nc

hl
e,(h1)
hl
d,(h1)
h1,B2B9
a,c
0016

a
(B2B8),a

SCREEN

(Start Control Buffer)

Table de saut caractère de commande

nombre requis
atteint paramètre de commande ?
non =>

Start Control Buffer
call (de)

(Compteur caractères Control Buffer)

LELL DSL LS É SSL SLR SSSR ESS SL ESS SSL SSL LL LL ELLLLLLL ELLES) TXT VDU DISABLE

14uB
14UE
14uF

CD9A12
AF
1805

call
xor
Jr

129A
a
1456

TXT CUR DISABLE

MED DE DE DE DEEE DEEE DE DD OUI DE DE DE DE DEEE DE DE DEEE DE DE DE DE ED DE DE DE TXT VDU ENABLE

1451
1454
1456
1459
145B
145C
145F
1462
1465
1468

CD8912
3EFF
328EB2
18EB
AF
32B8B2
216B14
11C3B2
016000
EDBO

call
ld
ld
jr
xor
ld
ld
ld
id
ldir

1289
a,FF
(B28E),a
1446

a
(B2B8),a
h1,146B
de, B2C3
bc, 0060

TXT CUR ENABLE

(VDU Flag (0=disabled))

(compteur caractères Control Buffer)
Saut défaut caractère de commande
Table de saut caractère de commande

-205-

TEXT SCREEN

146A C9 ret

LELLSÉRSLSLSLLLRS SSL SRS RSS SSL SSLSLSSLLSSSSSSÉES SES: Saut défaut Caractère
commande

146B 00 db 00

146C E214 dy 14E2 00 aucun effet

146E 00 db 00

146F 3413 dw 1334 01 TXT WR CHAR

1471 00 db 00

1472 9A12 dw 129A 02 TXT CUR DISABLE

1474 00 db 00

1475 8912 du 1289 03 TXT CUR ENABLE

1477 01 db 01

1478 CAOA dw OACA O4 SCR SET MODE

147A O1 db 01

147B 4519 dy 1945 05 GRA WR CHAR

147D 00 db 00

147E 5114 du 1451 06 TXT VDU ENABLE

1480 00 db 00

1481 D814 dW 14D8 07 bip

1483 00 db 00

1484 OA15 dn 1504 08 CRSR LEFT

1486 00 db 00

1487 0OF15 dy 150F 09 CRSR RGHT

1489 00 db 00

1484 1415 du 1514 OA CRSR DOWN

148C 00 db 00

148D 1915 dw 1519 OB CRSR UP

148F 00 db 00

1490 4015 dn 1540 OC TXT CLEAR WINDONW

1492 00 db 00

1493 3015 dw 1530 OD CRSR sur début de ligne
1495 01 db 01

1496  AE12 dw 12AE OE TXT SET PAPER

1498 01 db 01

1499 A912 dy 12A9 OF TXT SET PEN

149B 00 db 00

149C 4F15 dn 154F 10 supprimer caractère sur CRS Pos
149 00 db 00

149F 8E15 du 158E 11 supprimer ligne jusqu’à CRS Pos

-206-

de

TEXT SCREEN

12 supprimer ligne à partir de CRS

13 vider fenêtre Jusqu'à CRS Pos
14 vider fenêtre à partir de CRS Pos
15 TXT VDU DISABLE

16 Transparentmode mis/éteint

17 SCR ACCESS

18 TXT INVERSE

19 =SYMBOL (instruction)

1A definir fenêtre

1B aucun effet

1C =INK (instruction)

1D =BORDER (instruction)

1E CRSR HOME

1F =LOCATE (instruction)

ENNDEDEHE DD DD HD DE DU DEDEHDEDDEEDÉHEDIDE DEEE DEEE DE EDEN TXT GET CONTROLS

14A1 00 db 00
14A2 8415 dw 1584
Pos

14A4 00 db 00
14AS 6D15 du 156D
14A7 00 db 00
14A8 5615 dw 1556
14AA 00 db 00
1UAB 4B14 dw 144B
14AD 01 db 01
VAE E314 dw 14E3
14B0 01 db 01
14B1 490C dw 0C49
14B3 00 db 00
14B4 C912 dw 12C9
14B6 09 db 09
14B7 0415 dW 1504
14B9 Où db 04
14BA F814 dn 14F8
14BC 00 db 00
14BD E214 dn 14E2
14BF 03 db 03
14C0 E814 du 14E8
14C2 02 db 02
14C3 F114 dn 14F1
14C5 00 db 00
14C6 2A15 dw 152A
14C8 02 db 02
14C9 3815 dn 1538
14CB 21C3B2 ld h1,B2C3
14CE C9 ret

14CF 87 add  a,a
14D0 00 nop

14D1 00 nop

14D2 5A ld e,d
14D3 00 nop

14D4 00 nop

Table de saut caractère de commande

—207-

TEXT SCREEN

14D5 OB dec bc

14D6 14 inc d

14D7 00 nop

PETITS TITITIITIILITITILILLLLLLLLLLLLLLLLL LL ELLE EE] 07 Bip
14D8 DDES push ix

14DA 21CF14 ld h1,14CF

14DD CD9F1F call 1F9F SOUND QUEUE

14E0 DDE1 pop ix

14E2 C9 ret

CILLIIILILILSLSI LL LLL SSL LLLLLLE EEE) 16 Transparentmode mis/éteint

14E3 OF rrca
14E4 9F sbc  a,a
14ES C374A13 Jp 137A TXT SET BACK

DEEE DE DD DE DEN DDR DE DE DU DE DE DE DE DE DD DE DD DEN DE DIRE DE DE DE DEEE DD DE 1C =INK (instruction)

14E8 23 inc hl

14E9 7E id a,(h1)

1HEA 23 inc hl

14EB 46 ld b,(h1)

14EC 23 inc hl

TUED 4E id c,(h1)

Î4EE C3ECOC Jp OCEC SCR SET INK

DEN END DE SEE DE DE DE DE DEEE DD DD DE DE DE DE DE DE DD DE EDEN DD DE DIE 1D =BORDER (instruction)

14F1 23 inc hi

14F2 46 ld b,(h1)

14F3 23 inc hl

14F4 UE ld c,(hl)

14F5 C3F10C Jp OCF1 SCR SET BORDER

CTISITLIIIIILILIIILIS LILI LIL LS SL SIL SSL LL Li LS; ;:) 1A définir fenêtre

14F8 23 inc hi

14F9 56 ld d,(hl)
14FA 23 inc hl
14FB 7E id a,(h1)
14FC 23 inc hi
14FD 5€ ld e,(hl)
14FE 23 inc hl

-208-

TEXT SCREEN

14FF GE 1d 1,(h1)
1500 67 ld ha
1501 C30C12 Jp 120C TXT WIN ENABLE

LELLLLE LL LILI ELLE LL LL LL LL LL LL LLLL LL LL ;; LL...) 19 =SYMBOL (instruction)

1504 23 inc hl

1505 7E ld a,(hl)

1506 23 inc  hl

1507 C3F112 jp 12F1 TXT SET MATRIX
LELLLLLLILLLILILLLLLLLLLLSLLLILLLSLSLLLLS ELLES ES) 08 CRSR LEFT
150A 1100FF ld de,FF00

150D 180D jr 151€
CETÉELILLEZILLLILILLLLILLLLLLLLLS LILI LL ELLES LL S SE] 09 CRSR RGHT
150F 110001 ld de,0100

1512 1808 jr 151C

ÉRILLLLILLILLLLILSILLILSLLSLLS ESS LES SLS LES SSL SELS EE) OA CRSR DOWN
1514 110100 ld de, 0001

1517 1803 jr 151C

LILIILLLLLLLSLLLLLLLLSLSLSLLSRSL LIL LS SSII LS LL SIL LL LL) 0B CRSR UP
1519 11FF00 ld de, OOFF

151C D5 push de

151D CDA811 call 1148 move Cursor

1520 D1 pop de

1521 7D id a, l

1522 83 add a,e

1523 6F ld 1,a

1524 7C ld a,h

1525 82 add a,d

1526 67 ld ha

1527 C374A11 jp 117A

LETLLILLLILSLLLLLLLLLLILLLSLLLS LS LS SSL S LS LL LL S LL L] 1E CRSR HOME
152A 2A88B2 ld h1, (B288) (fenêtre act, haut)

152D C37711 jp 1177

DD DD DH DDÉ HE DE DE D HE DE D DH DE DEDE DEEE D DE DE DE DEEE HD DE DE DE HD DEEE 0D CRSR sur début de

-209-

TEXT SCREEN

ligne

1530 CDA811 call 1148 move Cursor

1533 3A89B2 ld a, (B289) (fenêtre act. gauche)
1536 1S8EE Jr 1526

DEHEDENEDEHEDE DE HN DE DE HE DE DE DD DEN DD DE HEIN DE DE DH DEEE DEEE DEMO DE DE DEN 1F =LOCATE (instruction)

1538 23 inc  hl

1539 56 ld d,(hl)

153A 23 inc hl

153B 5€ ld e,(hl)

153C EB ex de,hl

153D C37411 jp 1174 TXT SET CURSOR
LÉLÉELSLSLLLLLLLLLLLLLELSLLLLLELLLLLLLLLLLSLLS IS LS ESS) TXT CLEAR WINDOW
1540  CDDOBD call BDDO TXT UNDRAW CURSOR

1543 2A88B2 id h1,(B288) (fenêtre act. haut)

1546 2285B2 ld (B285),h1 (Pos. curseur act,(Ron,Col))
1549 EDSB8AB2 ld de, (B28A) (fenêtre act. bas)

154D 1848 Jr 1597

CELLES LL LLLELLLLL LES LL LLLLLLLLLLLLLLL:) 10 supprimer caractère sur CRS
Pos

154F CDA811 call 1148 move Cursor

1552 54 ld d,h

1553 5D ld e,l

1554 1841 jr 1597

LELLLLLLLLLLLLLELLLLLLLLLLLLLLLLLSZ EX S)] 14 vider fenêtre à partir de CRS Pos
1556 CD8415 call 1584 12 supprimer ligne à partir de CRS
Pos

1559 2A88B2 ld h1,(B288) (fenêtre act. haut)

155C EDSB8AB2 ld de, (B28A) (fenêtre act. bas)

1560 3A85B2 ld a,(B285) (Pos. curseur act.(Ron,Col))

1563 6F ld La

1564 2C inc  ]!

1565 BB CP e

1566 3A90B2 ld a,(B290) (TXT act. Paper)

1569 DCB30D call c,0DB3 SCR FILL BOX

156C C9 ret

—210-

TEXT SCREEN

CELLLELL EL EL LL LL LL LS LIL LL LL LL LL LL LL LL) 13 vider fenêtre Jusqu'à CRS Pos

156D CD8E15 call 158€ 11 supprimer ligne jusqu’à CRS Pos
1570 2A88B2 ld h1,(B288) (fenêtre act. haut)

1573 3A8BB2 id a, (B28B) (fenêtre act. droite)

1576 57 ld d,a

1577 3A85B2 1d a,(B285) (Pos. curseur act.(Row,Col))
157A 3D dec a

157B SF ld e,a

157C BD Cp 1

157D 3A90B2 ld a,(B290) (TXT act. Paper)

1580 D4B30D call nc,ODB3  SCR FILL BOX

1583 C9 ret

LELLLILLESLLLLELLLLSILIILILLLLLLLILLILIL2: 12 supprimer ligne à partir de CRS Pos
1584 CDA811 call 1148 move Cursor

1587 SD ld e,l

1588 3ASBB2 ld a, (B28B) (fenêtre act. droite)

158B 57 ld d,a

158C 1809 jr 1597

LILLILLI LL LILILLLILLLILLILLLLLL ILE LILI ZI] 11 supprimer ligne jusqu'à CRS Pos
158E CDA811 call 1148 move Cursor

1591 EB ex de,hl

1592 6B ld Le

1593 3A89B2 ld a,(B289) (fenêtre act. gauche)

1596 67 ld ha

1597  3A90B2 ld a,(B290) (TXT act. Paper)

159A CDB30D call ODB3 SCR FILL BOX

159) CDCDBD call BDCD TXT DRAW CURSOR

15A0 C9 ret

15A1 C7 rst O0

15A2 C7 rst O0

15A3 C7 rst O0

15A4 C7 rst O0

1545 C7 rst O0

15A6 C7 rst O0

15A7 C7 rst O0

1548 C7 rst O0

15A9 C7 rst 0

-211-

15AA
15AB
15AC
15AD
15AE
15AF

C7
C7
C7
C7
C7
C7

rst
rst
rst
rst
rst
rst

OOOoOCOOO

TEXT SCREEN

—212-

GRAPHICS SCREEN

2.5.6 GRAPHICS SCREEN (GRA)

Ce pack sert exclusivement à la manipulation de la fenêtre graphique, Au
sujet des indications de coordonnées qui sont réclamées par les
différentes routines, il convient de faire les remarques suivantes:

Les coordonnées sont transmises en 3 (4) étapes, L'étape la plus proche
de l'utilisateur est la position relativement à l'origine des coordonnées
(ORIGIN) qu'il a lui-même fixée. Cette position est convertie en une
position relativement à l'origine de l'écran (bas gauche).

Ces deux étapes dépendent du mode!

La dernière étape est l'adresse physique du point. Celle-ci dépend du
mode actuel!

Une étape supplémentaire peut éventuellement être ajoutée auparavant,
lorsqu'une paire de coordonnées relatives doit être convertie en une
position absolue relativement à ORIGIN. ‘

Les routines intéressantes sont: GRA PLOT ABSOLUTE qui fixe un point dans
la position absolue fournie par de (coordonnée X) et hl (coordonnée Y),
si ces coordonnées ne sortent pas de la fenêtre graphique.

Notez que cette routine fonctionne à travers l’indirection GRA PLOT au
cours du déroulement de laquelle l’indirection SCR WRITE est également
utilisée!

GRA LINE ABSOLUTE dessine une ligne à partir du curseur graphique actuel
jusqu’à la position absolue déterminée par de (coordonnée X) et hl
(coordonnée Y), si cette position ne sort pas du cadre de la fenêtre
graphique. Ici aussi des indirections sont utilisées: GRA LINE et SCR
WRITE!

GRA WR CHAR amène le caractère contenu dans a sur l'écran et ce dans la
position actuelle du curseur GRAPHIQUE. Celle-ci détermine l'angle
supérieur gauche du caractère. Le curseur graphique est ensuite déplacé
de la distance correspondant à la largeur du caractère. Cette distance
dépend du mode!

-212-

GRAPHICS SCREEN

RNEIEIE HD DE RUE DE DE DEN DE DE DD EE DE DE DE DE DE HE DE DE NEED EH MEME MEME DE DEEE GRA INITIALISE

15B0 CDDF15 call 15DF GRA RESET

15B3 210100 1d h1,0001 Pen 1 , Paper 0
15B6 7C ld a,h

15B7 CDFD17 call 17FD GRA SET PAPER
15BA 7D id a, |

15BB CDF617 call 17F6 GRA SET PEN
15BE 210000 ld h1,0000 Origin sur 0,0
1501 54 ld d,h

15C2 5D ld e,l

15C3 CD0416 call 1604 GRA SET ORIGIN
15C6 110080 id de, 8000

15C9 21FF7F ld h1,7FFF

15CC ES5 push hl

15CD D5 push de

15CE CD3417 call 1734 GRA WIN WIDTH
15D1 E1 pop  hl

15D2 D1 pop de

15D3 C37917 JP 1779 GRA WIN HEIGHT
15D6 CDOA18 call 180A GRA GET PAPER
159 67 ld h,a

15DA CD0418 call 1804 GRA GET PEN
15DD 6F ld 1,a

15DE C9 ret

LÉELLLSL SELS LLLELL LL LL LLLLLLLLLLLLLLLLLLLLLLLLZLEL XXL] GRA RESET
15DF 21E515 ld h1,15E5 Restore GRA Indirections
15E2 C38A0A jp OA8A Move (h1+3)=>((h1+1)),cnt=(h1)
15E5 09 db 09 9 octets

15E6 DCBD dy BDDC Adresse objet
15E8 C31618 Jp 1816 GRA PLOT

15EB C32A18 jp 182A GRA TEST

15EE C33C18 JP 183C GRA LINE

RD H DD DIE DE DE DID DD HE D DE DE DE DEN DE DEEE DE TEE EEE CRA MOVE RELATIVE

15F1 CD5716 call 1657 Add coord, act. + coord. rel.

-213-

GRAPHICS SCREEN

15F4  ED532CB3 Id (B32C),de (Coord, X act.)

15F8 222EB3 ld (B32E),h1 (Coord. Y act.)

15FB C9 ret

DDR DEEE DEN DEAD DEEE CRA ASK CURSOR
15FC EDSB2CB3 1d de, (B32C) (Coord, X act.)

1600 2A2EB3 ld hl1, (B32E) (Coord. Y act.)

1603 C9 ret

1604 ED5328B3 1d (B328),de (X Origin)

1608 222AB3 Id (B32A),h1 (Y Origin)

160B 110000 Id de, 0000

160€ 62 ld hd

160F 6B ld 1,e

1610 182 Jr 15F4 GRA MOVE ABSOLUTE

DD DADD DEEE DD DEEE DEEE DEAD GCRA GET ORIGIN
1612 ED5B28B3 1d de, (B328) (X Origin)

1616 2A2AB3 ld h1,(B32A) (Y Origin)

1619 C9 ret

EEE EREEEAAEES ]ler chercher position de départ physique

161A CDFC15 call 15FC GRA ASK CURSOR
eee # j]ler chercher position objet physique et fixer Cur
161D CDF415 call 15F4 GRA MOVE ABSOLUTE
1620 E5 push hl

1621 CDECOA call OAEC SCR GET MODE

1624 2F cpl

1625 C601 add  a,01

1627 CEO2 adc  a,02

1629 2600 ld h,00

162B 6F ld 1,a

162C CB7A bit 7,d

162E 2803 Jr Z,:1633

1630 EB ex de,h]l

1631 19 add hl,de

1632 EB ex de,hl

1633 2F cpl

1634 A3 and e

-214-

GRAPHICS SCREEN

1635 5F ld e,a
1636 7D ld a,l

1637 2A28B3 ld  hl,(B328) (X Origin)
1634 19 add hl,de

163B OF rrca

163C DC7417 call c,1774

163F OF rrca

1640 DC7417 call c,1774

1643 D1 pop de

1644 E5 push hl

1645 7A ld  a,d

1646 07 rlca

1647 3001 Jr nc,164A

1649 13 inc de

164A 7B ld a,e

164B E6FE and FE

164D 5F ld e,a

164E  2A2AB3 1d  h1,(B32A) (Y Origin)
1651 19 add hl,de

1652 CD7417 call 1774

1655 D1 pop de

1656 C9 ret

CELL SL LS SL SL LL EL LL LL ii LL lis Li Lili] Add coord. act. + coord. rel.

1657 ES push hl

1658 2A2CB3 1d h1,(B32C) (coord, X act.)
165B 19 add hl,de

165€ D1 pop de

165D ES5 push hl

165E 2A2EB3 ld h1,(B32E) (coord. Ÿ act.)
1661 19 add hl,de

1662 D1 pop de

1663 C9 ret

1664 DS push de

1665 E5 push hl

1666 2A30B3 ld h1,(B330) (Coord. X Fenêtre GRA Gauche)
1669 2B dec h]l

166A B7 or a

-215-

166B
166D
1670
1673
1674
1676
1679
167A
167D
167E
1680
1683
1686
1687
1688
168A
168D
1691
1694
1695
1696
1698
169B
16%Æ
169F
16A1
16A4
16A8
16A9
16AA
16AB

16AC
16AD
16AE
16AF

16B0
16B1
16B2

ED52
F2AC16
2A32B3
B7
ED52
FAAC16
D1
2A34B3
B7
ED52
FAAD16
2A36B3

sbc
Jp
ld
or
sbc
jp
pop
1d
or
sbc
Jp
ld
dec

sbc
Jp
ld
id
dec

sbc
Jp
ld
or
sbc

ld
ex
pop
scf
ret

Pop
pop
or

ret

push
push
ex

GRAPHICS SCREEN

hl,de

p, 16AC
h1, (B332)
a

h1,de

m, 16AC
de

h1, (B334)
a

hl,de

m, 16AD
h1, (B336)
hl

a

h1,de

m, 1691
de, (B336)
h1, (B336)
hl

a

hl1,bc

p, 16AD
h1, (B334)
a

hl,bc

p, 1648
bc, (B334)
de,hl

de

hl
de

hl
de
de,hl

(Coord

(Coord

(Coord

(Coord
(Coord

(Coord

(Coord

-216-

. X Fenêtre GRA droite)

. Y Fenêtre GRA Haut)

. Ÿ Fenêtre GRA Bas)

. Y Fenêtre GRA bas)
. Y Fenêtre GRA bas)

. Ÿ Fenêtre GRA haut)

. Y Fenêtre GRA haut)

16B3
16B6
16B7
16B8
16BA
16BD
16C0
1601
16C3
16C6
16C7
16CA
16CB
16CD
16D0
16D3
16D4
16D5
16D7
16DA
16DE
16E1
16E2
16E3
16E5
16E8
16EB
16EC
16EE
16F1
16F5
16F6
16F7

16F8
16F9
16FA
16FB

16FC

2A36B3
2B

B7
ED52
F2F816
2A34B3
B7
ED52
FAF816
Di
2A32B3
B7
ED52
FAF916
2A30B3
2B

B7
ED52
FADE16
ED5B30B3
2A30B3
2B

B7
ED42
F2F916
2A32B3
B7
ED42
F2F516
ED4B32B3
E1

37

C9

D1
EÎ
B7
C9

CD1D16

ld
dec
or
sbc

ld
or
sbc
jp
pop
ld
or
sbc
Jp
id
dec
or
sbc
jp
ld
ld
dec
or
sbc
Jp
ld
or
sbc
Jp
ld
pop
scf
ret

Pop
pop
or

ret

call

GRAPHICS SCREEN

h1,(B336) (Coord.
hl

a

hl,de

p,16F8

h1,(B334) (Coord,
a

hl,de

m, 16F8

de

h1,(B332) (Coord.
a

hl,de

m,16F9

h1,(B330) (Coord.
hl

a

hl,de

m, 16DE

de, (B330) (Coord.
h1,(B330) (Coord.
hl

a

h1,bc

p,16F9

h1,(B332) (Coord.
a

h1,bc

p,16F5

bc, (B332) (Coord.
hl

de
hl

Y Fenêtre GRA bas)

Y Fenêtre GRA haut)

X Fenêtre GRA droite)

X Fenêtre GRA gauche)

X Fenêtre GRA gauche)
X Fenêtre GRA gauche)

X Fenêtre GRA droite)

X Fenêtre GRA droite)

161D aller chercher pos ob} phys

-217-

GRAPHICS SCREEN

16FF ES push hl et fixer Cur

1700 2A30B3 ld h1,(B330) (Coord. X Fenêtre GRA gauche)
1703 2B dec hl

1704 B7 or a

1705 ED52 sbc hl,de

1707 F22D17 Jp p,172D

170A 2A32B3 ld h1,(B332) (Coord. X Fenêtre GRA droite)
170D B7 or a

170E ED52 sbc hi,de

1710 FA2D17 Jp m, 172)

1713 E1 pop hl

1714 D5 push de

1715 EB ex de,hl

1716 2A36B3 ld h1,(B336) (Coord, YŸ Fenêtre GRA bas)
1719 2B dec hl

171A B7 or a

171B ED52 sbc hl,de

171D F23017 Jp p,1730

1720 2A34B3 id h1,(B334) (Coord. Y Fenêtre GRA haut)
1723 B7 or a

1724 ED52 sbc hl,de

1726 FA3017 Jp m, 1730

1729 EB ex de,hl

1724 Di pop de

172B 37 scf

172C C9 ret

172) Ei pop hl

172E B7 or a

172F C9 ret

1730 EB ex de,hl

1731 Di pop de

1732 B7 or a

1733 C9 ret

LÉELLS ETS ES LS LLS LS LLS RS SL LSLSILÉLSLLSILLSILLSLLSS: GRA WIN WIDTH
1734 ES push hl

1735 CD6017 call 1760

1738 D1 pop de

-218-

GRAPHICS SCREEN

1739 ES push h]

173A CD6017 call 1760
173) D1 pop de
173E 7B ld a,e
173F 95 sub !

1740 7A ld a,d
1741 9C sbc a,h
1742 3801 jr c,1745
1744 EB ex de,hl
1745 7B ld a,e
1746 E6F8 and F8
1748 5F ld e,a
1749 7D ld a, 1
174A F607 or 07
174C 6F ld 1,a
174D CDECOA call OAEC SCR GET MODE
1750 3D dec a

1751 FC7017 call m,1770
1754 3D dec a

1755 FC7017 call m,1770
1758 ED5330B3 ld (B330),de (Coord. X Fenêtre GRA gauche)
175C 2232B3 ld (B332),h1 (Coord. X Fenêtre GRA droite)
175F C9 ret

1760 7A ld a,d
1761 B7 or a

1762 210000 ld h1,0000
1765 F8 ret m

1766 217F02 ld h1,027F
1769 7B ld a,e
1764 95 sub 1!

176B 7A ld a,d
176C 9C sbc a,h
176D DO ret nc

176E EB ex de,hl
176F C9 ret

1770 CB2A sra d

1772 CBIB rr e

1774 CB2C sra h

-219-

GRAPHICS SCREEN

1776 CB1D rr 1
1778 C9 ret

CELL LL LL LL LL LL LL LIL LIL LIL LLLL LL LLLL ELLE: ELLE): ) GRA WIN HEIGHT

1779 ES push h1l

177A CD9217 call 1792
177D D1 pop de

177E ES push hl

177F  CD9217 call 1792
1782 D1 pop de

1783 7D ld a, l
1784 93 sub e

1785 7C ld a,h
1786 9A sbc a,d
1787 3801 Jr c,178A
1789 EB ex de,hl
178A ED5334B3 Id (B334),de (Coord. Ÿ Fenêtre GRA haut)
178E 2236B3 ld (B336),h1 (Coord. Y Fenêtre GRA bas)
1791 C9 ret

1792 7A ld a,d
1793 B7 or a

1794 210000 ld h1,0000
1797 F8 ret m

1798 CB3A srl d

179A CB1B rr e

179C 21C700 ld h1,00C7
179F 7B ld a,e
17A0 95 sub !

17A1 7A ld a,d
17A2 9C sbc a,h
1743 DO ret nc

17A4 EB ex de,hl
17A5 C9 ret

LLLLI LI LLLLLL LL LL ELLE LL LL LL LIL LL LL LL LL. LLLLE, EX): : GRA GET W WIDTH

17A6 ED5B30B3 id de, (B330) (Coord. X Fenêtre GRA gauche)
17AA 2A32B3 ld h1,(B332) (Coord. X Fenêtre GRA droite)
17AD CDECOA call OAEC SCR GET MODE

-220-

GRAPHICS SCREEN

17B0 3D dec a

17B1 FCB617 call m,17B6
17B4 3D dec a

17B5 F0 ret p

17B6 29 add hl,hl
17B7 23 inc hl
1788 EB ex de,hl
17B9 29 add hl,hl
17BÀA EB ex de,hl
17BB C9 ret

LLLLLLLLLLLLLE LI LLELL LL I LLLI LL LL SELS SL L LILI LILI LL] GRA GET W HEIGHKT

17BC ED5B34B3 ld de, (B334) (Coord, Ÿ Fenêtre GRA haut)
17C0 2A36B3 ld h1,(B336) (Coord. Y Fenêtre GRA bas)
17C3 18F1 Jr 17B6
LELLLELLILESLLLILILILLILILILILLILLILILLLLLILLLLSS.. GRA CLEAR WINDOW
17C5 CDA617 call 1746 GRA GET W WIDTH

17C8 B7 or a

17C9 ED52 sbc hl,de

17CB 23 inc hl

17CC CD7417 call 1774

17CF CD7417 call 1774

17D2 CB3D srl 1

17D4 45 ld b, 1

17D5 EDSB36B3 id de, (B336) (Coord. Y Fenêtre GRA bas)
17D9 2A34B3 ld h1,(B334) (Coord, Y Fenêtre GRA haut)
17DC E5 push hl

17DD B7 or a

17DE ED52 sbc hl,de

17E0 23 inc  hl

17E1 UD ld c, 1

17E2 ED5B30B3 ld de, (B330) (Coord. X Fenêtre GRA gauche)
17E6 El pop hl

17E7 C5 push bc

17E8 CDA9OB call OBA9 SCR DOT POSITION

17EB D1 pop de

17EC 3A39B3 ld a, (B339) (GRA Paper)

17EF 4F ld c,a

17F0 CDB70D call ODB7 SCR FLOOD BOX

-221-

GRAPHICS SCREEN

17F3 C30B16 jp 160B

ADD ADD DD DEEE DD DEAD CRA SET PEN
17F6 CD860C call OC86 SCR INK ENCODE

17F9 3238B3 id (B338),a (GRA Pen)

17FC C9 ret

LR SSSR SSL LS LL LL LL LL is si iii iii iiiliiliiiiiis:] GRA SET PAPER

17FD CD860C call 0C86 SCR INK ENCODE

1800 3239B3 id (B339),a (GRA Paper)

1803 C9 ret

LLLLLLILLLL LIL LL LLLILLELLLIL LL LLLLILLL LL LL LLlLLLL) GRA GT PEN

1804 3A38B3 ld a,(B338) (GRA Pen)

1807 C3AO0OC Jp OCAO SCR INK DECODE

LILI LSLLLL LL LS LS I LILI LL LLILLLLILLLLLLLLILLL LL LLL LL) GRA GT PAPER
180A 3A39B3 id a, (B339) (GRA Paper)

180D C3A00C jp OCAO SCR INK DECODE

LILI SLILSILLLLL LL SIL ILS SL SSL ILILSLSLLS LL ISLE EE) GRA PLOT RELATIVE
1810 CD5716 cell 1657 Add coord. act. + coord. rel.
A A DE ADF D DE AE DE DD D DE DE DE D D DE AE ARE DEEE GRA PLOT ABSOLUTE
1813 C3DCBD Jp BDDC GRA PLOT

LLLLLLLLLLLLLLSLELLLS IL LSLLLL LL LL LL ELLL ELLE LL LEZ) GRA PLOT

1816 CDFC16 call 16FC

1819 DO ret nc

181A CDA9OB call OBA9 SCR DOT POSITION

181D 3A38B3 id a,(B338) (GRA Pen)

1820 47 ld b,a

1821 C3E8BD Jp BDE8 SCR WRITE

LÉLLS SL SL LS SSL LL Li Lis LL iii il iii lili: lil:; GRA TEST RELATIVE

1824 CD5716 call 1657 Add coord. act. + coord. rel,

LÉLL SSL SL LL LL LL ill ii iii) lilili::)))il)i;::i;ii:) GRA TEST ABSOLUTE

1827 C3DFBD Jp BDDF GRA TEST

—222-

GRAPHICS SCREEN

Lis LS La LL SL SL LL SELS LL SL ELLE LL Li LLis iii LLLlL] GRA TEST

182A CDFC16 call 16FC

182D D20A18 JP nc,180A  GRA GET PAPER
1830 CDA9OB cali OBA9 SCR DOT POSITION
1833 C3ESBD Jp BDES SCR READ

LÉLLS SL LS LL LL LS LL SELLE LS SSL LLS SSII SSL SELS LS SZ GRA LINE RELATIVE

1836 CD5716 call 1657 Add coord. act, + coord, rel,.

LL EL SLI SSL SSL LL SL SSL LL EL LIL LS LL SSL LL SSL LL LL LS] GRA LINE ABSOLUTE

1839 C3E2BD jp BDE2 GRA LINE

LELLELL LL SLI LLEL LIL IE LIL LLILLLLLLILILLILLLLILILLLLZLS] GRA LINE

183C ES push hl

183D DS push de

183E CD1A16 call 161À aller chercher pos départ phys
1841 ED5342B3 ld (B342),de (Buffer de calcul Coord. X)
1845 2244B3 id (B344),h1 (Buffer de calcul Coord. Y)
1848 D1 pop de

1849 E1 pop hi

184A CD1D16 call 161D aller chercher pos obj phys
184D ES push hl et fixer Cur

184E 2A42B3 id h1,(B342) (Buffer de calcul Coord. X)
1851 B7 or a a

1852 ED52 sbc hl,de

1854 ul id b,h

1855 4D ld c, 1

1856 FA6918 Jp m, 1869

1859 2A42B3 ld h1,(B342) (Buffer de calcul Coord. X)
185C EB ex de,hl

185D 2242B3 ld (B342),h1 (Buffer de calcul Coord. X)
1860 2A44B3 ld h1,(B344) (Buffer de calcul Coord, Y)
1863 E3 ex (sp),h1

1864 2244B3 ld (B344),h1 (Buffer de calcul Coord. Y)
1867 1808 Jr 1871

1869 210000 id h1,0000

186C B7 or a

186D ED42 sbc hl,bc

186F 44 id b,h

1870 4D ld c, 1

-223-

1871
1872
1875
1876
1878
1879
187C
187F
1880
1882
1883
1884
1885
1887
188A
188C
188€
188F
1890
1891
1893
1896
1898
189B
189C
189D
189F
18A1
18A2
18A5
18A6
18A9
18AC
18AD
18AF
18B2
18B3
18B6
18B7
18B8

D1
2A44B3
B7
ED52
EB
F28E18
210000
B7
ED52
54

5D

B7
ED42
210100
3027
1804
62

6B

B7
ED42
21FFFF
3009
223AB3
60

69
3EFF
1819
E5
2A42B3
09
2242B3
2A44B3
B7
ED52
2244B3
E1
223AB3
60

69

EB

pop
ld
or
sbc
ex
Jp
ld
or
sbc
1a
1d
or
sbc
ld
Jr
Jr
ld

or
sbc
ld
Jr
id
ld
id
ld
Jr
push
ld
add
ld
ld
or
sbc
ld
pop
id
ld
ld
ex

GRAPHICS SCREEN

de

h1,(B344) (Buffer de calcul Coord. Y)

a

hl,de
de,hl
p, 188€
h1,0000
à

hl,de
d,h

e,l

a

hl,bc
h1,0001
nc, 18B3
1898
h,d

Le

a

h1,bc
h1,FFFF
nc, 18A1

(B33A),h1

h,b
1,c
a,FF
18BA
hl

h1,(B342) (Buffer de calcul Coord,

hl,bc

(B342),h1 (Buffer de calcul Coord.
h1,(B344) (Buffer de calcul Coord.

a
hl,de

(B344),h1 (Buffer de calcul Coord.

hl

(B33A),h1

h,b
1,c
de,hl

-224-

X)

X)
Y)

Y)

18B9
18BA
18BD
18BE
18C2
18C3
18C6
18C9
18CD
18D1
18D2
18D3
18D5
18D7
18D8
18DC
18DF
18E0
18E1
18E4
18E5
18E7
18E9
18EA
18EB
18EC
18EE
18EF
18F0
18F1
18F4
18F5
18F7
18FA
18FB
18FC
18FD
1900
1901
1902

AF
324683
13
ED5340B3
23
CD8C37
223CB3
ED533EB3
ED4B4OB3
50

59

CB3A
CB1B

C5
ED4B3CB3
2A3EB3
19

EB
2A4OB3
B7

ED52
3007

19

EB

B7

ED52

EB

03

D5
3A46B3
B7

2823
2A42B3
54

5D

09
2242B3
44

4D

oB

xor
ld
inc
ld
inc
call
ld
ld
ld
ld
ld
srl
rr
push
ld
ld
add
ex
ld
or
sbc
jr
add
ex
or
sbc
ex
inc
push
ld
or
Jr
ld
ld
ld
add
ld
id
id
dec

GRAPHICS SCREEN

a
(B346),a

de

(B340),de

hl

378C hl/de => hl, Rest => de
(B33C),h1

(B33E),de

bc, (B340)

d,b

e,c

d

e

bc

bc, (B33C)

h1, (B33E)

hl,de

de,hl

h1, (B340)

a

hl,de

nc, 18F0

hl,de

de,hl

a

h1,de

de,h1

bc

de

a, (B346)

a

Z:191A

h1,(B342) (Buffer de calcul Coord. X)
d,h

e, 1

h1,bc

(B342),h1 (Buffer de calcul Coord. X)
b,h

c, 1

bc

—225-

1903
1906
1907
1904
190D
1910
1911
1914
1915
1918
191A
191D
19€
191F
1920
1923
1924
1925
1926
1927
192B
192C
192F
1932
1935
1936
1939
193A
193D
193€
193F
1940
1941
1942
1944

2AUUB3
E5
CDB016
3A38B3
DCC4OF
D1
2A3AB3
19
2244B3
1823
2A4UB3
54

5D

09
224uB3
uu

UD

0B

EB
ED5B42B3
D5
CD6416
313883
DC2F10
D1
2A3AB3
19
2242B3
Di

ci

0B

78

BI
2093
c9

id
push
call
ld
call
pop
ld
add
ld
Jr
1d
ld
ld
add
ld
ld
id
dec
ex
ld
push
call
id
call
pop
id
add
Id
pop
pop
dec
ld
or
Jr
ret

GRAPHICS SCREEN

hl1, (B344)
hl

16B0

a, (B338)
c,OFC4

de

h1, (B334)
hl,de
(B344),h1
193)

h1, (B344)
d,h

e,l

h1,bc
(B344),h1
b,h

c,l

bc

de,hl

de, (B342)
de

1664

a, (B338)
c,102F

de

h1, (B334A)
hl,de
(B342),h1
de

bc

bc

a,b

c

nz, 18D7

(Buffer de calcul Coord,

(GRA Pen)

SCR HORIZONTAL

(Buffer de calcul Coord.

(Buffer de calcul Coord.

(Buffer de calcul Coord,

(Buffer de calcul Coord.

(GRA Pen) -

SCR VERTICAL

(Buffer de calcul Coord,

CELLLLLLLLLLLLLL I LILI LL LLLLELLLLLLL LL LELLLEL EE] GRA WR CHAR

1945
1947
194A

DDES
CDD312
113AB3

push
call
ld

ix
12D3
de,B33A

TXT GET MATRIX

-226-

Y)

Y)

Y)

Y)

X)

X)

194D
194E
1950
1953
1955
1958
195B
195D
195E
195F
1962
1963
1964
1965
1966
1968
196B
196C
196D
196F
1972
1974
1975
1977
197A
197C
197F
1986
1987
198A
198C
198D
198F
1991
1994
1995
1998
199B
199D
199F

D5
DDE1
010800
EDBO
CD1A16
CDFF16
304C
ES

D5
010700
EB

09

EB

B7
ED42
CDFF16
Di

ET
303A
CDA9OB
1608
ES
1E08
CDCF19
CBO9
DCF90B
DDCB0006
ET
CD130C
DD23
15
205
DDE1
CDFC15
EB
CDECOA
010800
FEO1
2804
3003

push
pop
ld
idir
call
call
Jr
push
push
ld
ex
add
ex
or
sbc
call
pop
POP
Jr
call
ld
push
ld
call
rrc
call
ric
POP
call
inc
dec
jr
POP
call
ex
call
id
CP
Jr
jr

GRAPHICS SCREEN

de
ix
bc,0008

161A
16FF
nc, 19A9
hl

de

bc, 0007
de,hl
h1,bc
de,hl

a

h1,bc
16FF

de

hl

nc, 19A9
OBA9
d,08

hl
e,08,
19CF

C
c,O0BF9
(ix+00)
hl

0C13

Ix

d

nz, 1974
ix

15FC
de,hl
OAEC
bc, 0008
01
Z,1943
nc, 1944

aller chercher pos départ phys

SCR DOT POSITION

SCR NEXT BYTE

SCR NEXT LINE

GRA ASK CURSOR

SCR GET MODE

-227-

GRAPHICS SCREEN

19A1 09 add hl,bc

1942 09 add hl,bc

19A3 09 add hl,bc

19A4 09 add hl,bc

19A5 EB ex de,hl

1946 C3F415 Jp 15F4 GRA MOVE ABSOLUTE
19A9 0EO8 ld c,08

19AB D5 push de

19AC 0608 ld b,08

19AE CDFF16 call 16FF

1981 300C Jr nc, 19BF

1983 ES push hl

1984 D5 push de

1985 C5 push bc

19B6 CDA9OB call OBA9 SCR DOT POSITION
19B9 CDCF19 call 19CF

19BC Ci pop bc

198D D1 pop de

19BE E1 pop hl

19BF DDCB0006 rlc (ix+00)

19C6 D1 pop de

19C7 2B dec hl

19C8 DD23 inc ix

19CA OD dec c

19CB 20DE Jr nz,19AB

19CD 18C0 Jr 198F

19CF  DDCBOO07E bit 7,(1x+00)

198 3A39B3 ld a, (B339) (GRA Paper)
19DB 47 ld b,a

19DC C3E8BD jp BDE8 SCR WRITE
19DF C7 rst O0

-228-

KEYBOARD MANAGER

2.5.7 ‘KEYBOARD MANAGER (KM)

Ce pack a pour fonction la surveillance du clavier et la conversion en
codes de caractères utilisables.

Pour l'interrogation cyclique des touches, il utilise le mécanisme
d'EVENT,

Voici les routines que nous avons sélectionnées:

KM WAIT CHAR va chercher un caractère dans le buffer clavier, dans la
chaîne d'extension ou dans le buffer Put Back. Si aucun caractère n'est
disponible, la routine ne revient pas. Elle attend obligatoirement.

a contient s’il y a lieu le caractère qui a été entré au clavier.

KM READ CHAR transmet également un caractère dans a, s’il y en avait un,
mais cette routine n'attend pas qu'il y ait un résultat positif.

Si au retour de la routine, le carry est mis, c'est qu'il n’y avait pas
de caractère à aller chercher.

Les routines KM WAIT KEY et KM READ KEY travaillent de façon similaire,
mais seul le buffer clavier est interrogé.
La chaîne d'extension et le buffer Put Back ne sont pas pris en compte.

KM SET REPEAT vous permet de déterminer quelles touches doivent être
dotées de la fonction de répétition.

11 faut placer en a le numéro de touche. b doit contenir 8FF si la touche
doit avoir une fonction de répétition et 0 s'il s'agit d'annuler la
fonction de répétition de cette touche.

-229-

KEYBOARD MANAGER

DM DEN UE DEAD DE DE DE DE DE DE DU DE DD DE DE DD OU DEEE D OU DEEE DE KM INITIALISE

19E0
193
196
19E7
1SEA
19EB
19EC
19EF
19F2
19F5
19F8
19F9
19FC
19FD
1A00
1A01
1A04
1405
1A08
1A0B
1A0D
TAOF
1A12
1414
1A15
1A17
1A19
1A1B
TAIC

h1,1E02
1C6D

a
(B50B),a
ha

La
(B4E7),hl
h1,B43C
de,FFBO
(B547),h1
hl,de
(B545),h1
hl,de
(B543),h1
hl,de
(B541),h1
de,h]
h1,1D69
bc,00FA

b,0A
h1,B4EB
(h1),00
hl

1412

b, OA
(h1),FF
hl

1A19

KM SET DELAY

(Shift Lock State)

(Adr. de table Repeat)
CAdr, Key CTRL Table)
(Adr. Key SHIFT Table)
(Adr, Key Translation Table)

Key Translation Table

Key State Map

LL SSL LL LL LL LL LL LL SL SL LL SSL LL LL LL LL LL LE LLLL LES SE) KM RESET

TAIE
1421
1A24
1427
1A2A
1A2D
1A30
1433

21021E ld
CD6D1C call
AF xor
320BB5 ld
67 id
6F ld
22E7B4 ld
213CB4 ld
11B0FF id
2247B5 ld
19 add
2245B5 ld
19 add
2243B5 ld
19 add
2241B5 ld
EB ex
21691D ld
01FA00 id
EDBO ldir
060A ld
21EBB4 ld
3600 ld
23 inc
10FB djnz
O6OA 1d
36FF ld
23 inc
10FB djinz
CDEDIC call
CD751A call
1146B4 id
219800 ld
CD811A call
21361A id
CD8A0A call
C3821C jp

1CED
1A75
de,B446
h1,0098
1A81
h1,1436
OA8A
1C82

KM EXP BUFFER CONT'D

Restore KM Indirection

Move (h1+3)=>((h1+1)),cnt=(h1)
KM DISARM BREAK

-230-

KEYBOARD MANAGER

1436 03 db 03 3 Octets
1437 EEBD dn BDEE Adresse objet
1A39 C32F1C Jp 1C2F KM TEST BREAK
LÉLL LL LLLILSLEESLLLILILILLLLIILLILLLIIT ESS SITES TITI.) KM WAIT CHAR
1A3C CD421A call 1A42 KM READ CHAR
1A3F
30FB ir nc; 1A3C KM WAIT CHAR
1A41 C9 ret

LL LIL LE LL ELLE LL STE TILL LS S SL LLLSLLISILI LI LS LL] KM READ CHAR

1A42 ES push hl

1443 21E0B4 ld h1,B4EO Put Back Buffer

1A46 7E ld a,(h1) aller chercher caractère
1A47 36FF ld (h1),FF vider buffer

1A49 BE cp (h1) y avait-il un caractère ?
1AUA 3827 Jr c,1A73 oui =>

TAUC 2ADEB4 ld h1, (B4DE) (Exp. String Pointer)
1AUF 7C ld a,h

1A50 B7 or a Exp, String ?

1451 2011 ir nz,1A64 oui =>

1453 CD5C1B call 1B5C KM READ KEY

1456 301B jr nc,1473 aucun caractère =>
1A58 FE80 cp 80 Caractère < 128 ?
TASA 3817 ir c,1A73 oui =>

1TASC FEAO cp AO

TASE 3F ccf

1A5F 3812 jr c,1A73

1461 67 Id h,a

1462 2E00 id 1,00

1464 DS push de

1465 CD2E1B call 1B2E KM GET EXPAND

1468 3802 ir c,1A6C

1A6A 2600 ld h,00

1A6C 2C inc 1

1A6D 22DEB4 ld (B4DE),h1 (Exp. String Pointer)
1A70 D1 pop de

1A71 300 jr nc, 1A53

1473 El pop  hl

1A74 C9 ret

-?31-

KEYBOARD MANAGER

1475 3EFF ld a, FF

1477 32E0B4 ld (B4EO),a (Put Back Buffer)

TA7A C9 ret

147B CD811A call 1481 KM EXP BUFFER CONT'D

1A7E 3F ccf

1A7F FB ei

1480 C9 ret

LILLLLLÉILLSLLSISLSLLLSSSSSNSIS SSSR LS LILI LLS SL SL LL LS SE) KM EXP BUFFER CONT'D
1481 F3 di

1482 7D ld a, l

1483 D631 sub 31

1485 7C ld a,h

1486 DEO0 sbc  a,00

1A88 D8 ret c

1A89 19 add hl,de

1A8A 22E3B4 ld (B4E3),h1 (Pointeur fin Exp Buffer)
1A8D EB ex de,h1

1A8E 22E1B4 ld (B4E1),h1 (Pointeur début Exp Buffer)
1491 013004 ld bc,0A30 ASCII

1A94 3601 id (h1),01 0

1496 23 inc hl à

1A97 71 ld (h1),c 9

1A98 23 inc hl dans

1499 OC inc c Expansion

1A9A 10F8 djnz 1A94 Buffer

1A9C EB ex de,hl Restore

1A9D 21B31A ld h1,1AB3 Default Exp String

1TAAO OEOA ld c,OA

1AA2 EDBO idir

1AAU EB ex de,hl

TAA5S 0613 id b,13

TAA7 AF xor a

1AA8 77 id (hl),a

1AA9 23 inc hl

TAAA 10FC dinz 1AA8

TAAC 22E5B4 id (BUES),h1 (Pointeur Exp Buffer libre)
1AAF 32DFB4 ld (B4DF),a

1AB2 C9 ret

—232-

KEYBOARD MANAGER

DD DD DE DE DEEE DE DE DD DD DE DD DIE DH DEDEHEDE DE DE DE DE HD DÉE DE DE D D DE DE DE Default Exp String

1AB3 O1 2E O1 OD 05 52 55 HE  ..... RUN
1ABB 22 OD

LL LL EL SSL SLLLLE SEL LLLL LILI LE LL ESS LLEL LILI LL S LE) KM SET EXPAND

1ABD F3 di

TABE 78 id a,b

TABF CD3E1B call 1B3E Adr. Exp String => de
1AC2 301F Jr nc,.1AE3  Token non valable =>
TAC C5 push bc

TAC5S D5 push de

1TAC6 E5 push hl

1TAC7 CDESIA call 1AES5 nettoyer Exp Buffer
TACA 3F ccf

1TACB E1 pop hl

TACC D1 pop de

TACD C1 pop bc

TACE 3013 Jr nc, 1AE3

1ADO 1B dec de

1AD1 79 ld a,c

TAD2 OC inc c

1AD3 12 ld (de),a

TAD4 13 inc de

1AD5S E7 rst 4 1d6a, (h1)

1AD6 23 inc hl

1AD7 OD dec c

1AD8 20F9 Jr nz, 1AD3

TADA 21DFB4 ld h1,B4DF

1ADD 78 ld a,b

1ADE AE xor  (hl)

TADF 2001 Jr nz,1AE2

1AET 77 ld (h1),a

1AE2 37 scf

1AE3 FB ei

1AE4 C9 ret

DÉUDEDEDEDE DE HEHE DD DD DD DE DE DE DE DE DE OU DE DUO DE DEEE DE DE D DE DEEE nettoyér Exp Buffer

1AE5 0600 ld b,00
1AE7 60 ld h,b
1AE8 6F ld 1,a

—233-

1AE9
TAEA
1AEB
TAEC
TAËE
TAEF
1AFO
1AF1
TAF2
TAF3
1AF4
1AF7
1AF9
1AFB
1AFD
1AFE
1AFF
1B00
1B03
1B04
1B05
1B08
1B09
1BOA
1BOB
1BOC
1BOD
1B0E
1B11
1B14
1B16
1B17
1B18
1B19
1B1B
1B1C
1B20
1B21

79
95

c8
300F
7D

69

4F

19

EB

09
CD221B
2823jr
EDBO
181F
UF

19

E5
2AE5B4
09

EB
2AE3B4
7D

93

7C

gA

El

D8
CD221B
2AE5B4
2806
D5

1B

28
EDB8
Di
ED53E5B4
B7

cg

ld
sub
ret
jr
ld
ld
ld
add
ex
add
call

ldir
jr
1d
add
push
1d
add
ex
ld
ld
sub
ld
sbc
pop
ret
call
ld
Jr
push
dec
dec
lddr
pop
ld
or
ret

KEYBOARD MANAGER

a,C

il

Z

nc, 1AFD
a, 1
1,c
c,a
hl,de
de,hl
hl,bc
1B22
z,1B1C

1B1C

c,a

hl,de

hl

h1, (BUES)
hl,bc
de,hl

h1, (B4E3)
a, 1]

e

a,h

a,d

hl

C

1B22

h1, (B4ES5)
z,1B1C
de

de

hl

de
(B4E5), de
a

Place pour nouvelle Exp String?
non =>

(Pointeur Exp Buffer libre)

(Pointeur fin Exp Buffer)

Place pour nouvelle Exp String?
(Pointeur Exp Buffer libre)
Non =>

(Pointer Exp Buffer libre)

EDEN DD DD DE DE DE DEEE HE DE DD DE DE HEIN DE DE DD DE place pour nouvelle Exp String?

-234-

KEYBOARD MANAGER

1B22 3AESB4 ld a, (B4ES) (Pointer Exp Buffer libre)
1B25 95 sub ]!

1B26 4F 1d c,a

1B27 3AE6B4 1d a, (B4E6)

1B2A 9C sbc  a,h

1B2B 47 ld b,a

1B2C B1 or c

1B2D C9 ret

LILSLSLS LL IS SIL SLLLSLSLLLLLLLILILILLLLLILLILLILLILS.) KM GET EXPAND
1B2E CD3E1B call 1B3E Adr, Exp String dans de
1B31 DO ret nc

1B32 BD cp 1

1B33 C8 ret z

1B34 3F ccf

1B35 DO ret nc

1B36 ES push hl

1B37 2600 ld h, 00

1B39 19 add hl,de

1B3A 7E ld a, (h1)

1B3B EI pop hl

1B3C 37 scf

1B3D C9 ret

LÉLL SLI SL ELLE LLS LL SL LE ELLES LELLL LS LLLLSLLILL LL TTL) Adr. Exp String dans de

1B3E E67F and 7F Token dans zone valable?
1B40 FE20 cp 20

1B42 DO ret nc non =>

1B43 ES push hl

1B44 2AE1B4 ld h1,(B4E1) (Pointer Start Exp Buffer)
1B47 110000 ld de, 0000

1B4A 3C inc a

1B4B 19 add hl,de ajouter à hl la longueur
1B4C SE ld e,(hl) de l'Expansion String
1B4D 23 inc  hl

1B4E 3D dec a

1B4F  20FA jr nz,1B4B

1B51 7B ld a,e

1B52 EB ex de,hl

1B53 El pop hi

-235-

KEYBOARD MANAGER

1B54 37 scf
1B55 C9 ret

DD NE DE DE DIE IDE DD HE DE DH DE DD DE DE DE DE DD DE DE NE DE DE DE DE DE DE DE DE DE DE DE DE DIE DE DE DEN KM WAIT KEY

1B56 CD5C1B call 1B5C KM READ KEY
1B59 30FB Jr nc,1B56 KM WAIT KEY
1B5B C9 ret

LES SEE SL LEZ LI ESS L I LLLLLIILLLILIILLLSLLILLSLLS.S. KM READ KEY
1B5C E5 push hl

1B5D C5 push bc

1B5SE CD151D call 1D15

1B61 303A Jr nc, 1B9D

1B63 79 ld a,C

1B64 FEEF cp EF

1B66 2834 Jr Z,1B9C

1B68 E60F and OF

1B6A 87 add  a,a

1B6B 87 add  a,a

1B6C 87 add  à,a

1B6D 3D dec a

1B6E 3C inc a

1B6F CBO8 Frc D

1B71 30FB jr nc, 1B6E

1B73 CDA01B call 1BA0

1B76 21E8B4 1d hl,BUYE8 Caps Lock State
1B79 CB7E bit 7,(hl)

1B7B 280A Jr Z,1B87

1B7D FE61 cp 61

1B7F 3806 Jr c,1B87

1B81 FE7B cp 7B

1B83 3002 jr nc, 1B87

1B85 C6EO add  a,E0

1B87 FEFF cp FF

1B89 28D3 jr Z,1B5E

1B8B FEFE cp FE

1B8D 21E7B4 ld hl,B4E7 Shift Lock State
1B90 2805 Jr Z,1B97

1B92 FEFD cp FD caps lock ?
1B94 23 inc hl

-236-

KEYBOARD MANAGER

1B95 2005 jr nz,1B9C non =>

1B97 7E ld a, (hl1)

1B98 2F cpl toggle caps lock

1B99 77 ld (h1),a

1B9A 18C2 Jr 1B5E

1B9C 37 scf

1B9D C1 pop bc

1B9Œ E1 pop hl

1B9F C9 ret

1BAO CB11 rl C

1BA2 DA481D Jp c,1D48 KM GET CONTROL

1BAS 47 ld b,a

1BA6 3AE7B4 ld a, (B4E7) (Shift Lock State)
1BA9 B1 or C

1BAA E640 and 40

1BAC 78 ld a,b

1BAD C2431D Jp nz,1D43 KM GET SHIFT

1BBO C33E1D Jp 1D3E KM GET TRANSLATE

LL EL LELL SET SS SSII SSL LILI LLLILLIILLILLILLILILLSS. TI) KM GET STATE
1BB3 2AE7B4 ld h1,(B4E7) (Shift Lock State)
1BB6 C9 ret

DD NH HE DH DD DE DE DEEE DEEE DD DD DÉHE DE DEEE DE ED DEMO DEDE DEEE Update Key State Map
1BB7 11FFB4 ld de,B4FF  Multihit contr. à B4F5
1BBA 21F5B4 ld h1,B4F5S Scan touches enfoncées
1BBD CD4608 call 0846 Scan Keyboard

1BCO 3A01B5 ld a, (B501)

1BC3 E6AO and AO isoler SHIFT/CTRL
1BCS 4F ld c,a

1BC6 21EDB4 ld h1,BU4ED Key 16.,,23

1BC9 B6 or (h1)

1BCA 77 ld (hl),a

1BCB 21FFB4 ld h1,B4FF  Multihit contr. à B4F5
1BCE 11EBB4 ld de,B4EB Key State Map

1BD1 0600 ld b,00

1BD3 1A ld a, (de)

1BD4 AE xor  (hl)

1BD5 A6 and  (hl)

—237-

KEYBOARD MANAGER

1BD6 C4481C call nz,1C48
1BD9 7E ld a, (hl)
1BDA 12 ld (de),a
1BDB 23 inc  hl

1BDC 13 inc de

1BDD OC inc  c

1BDE 79 ld a,c
1BDF E60F and OF

1BE1 FEOA cp OA

1BE3 20ŒE Jr nz, 1BD3
1BES 79 id a,C
1BE6 E6AO and AO

1BE8 CB71 bit  6,c
1BEA 4F ld c,a
1BEB C4EEBD call nz,BDEE KM TEST BREAK
1BEE 78 ld a,b
1BEF B7 or a

1BFO CO ret nz

1BF1 2109B5 ld h1,B509
1BF4 35 dec (hl)
1BF5 CO ret nz

1BF6 2A0AB5 id h1, (B504A)
1BF9 EB ex de,hl
1BFA 42 ld b,d
1BFB 1600 ld d, 00
1BFD 21EBB4 ld h1,B4EB Key State Map
1C00 19 add hl,de
1C01 7E ld a, (hl)
1C02 2A47B5 ld h1,(B547) (Adr. de table Repeat)
1C05 19 add hl,de
1C06 A6 and  (hl)
1C07 A0 and b

1C08 c8 ret z

1C09 2109B5 ld hl,B509
1C0C 34 inc  (hl)
1COD 3A40B5 id a, (B540)
1C10 B7 or a

1C17 CO ret nz

1C12 79 id a,C
1013 B3 or e

—238-

KEYBOARD MANAGER

1C14 4F id c,a

1C15 3AE9B4 ld a, (B4E9) (KM Delay)
1C18 3209B5 ld (B509),a
1C1B CDFE1C call 1CFE

1CIE 79 ld a,C

1C1F E60F and OF

1C21 6F ld 1,a

1C22 60 ld h,b

1C23 220AB5 ld (B50A),h1
1C26 FEO8 cp 08

1C28 CO ret nz

1C29 CB60 bit  4,b

1C2B CO ret nz

1C2C CBF1 set  6,c

1C2E C9 ret

MED DE DD DEEE DEEE DD DEEE DE DEEE DEEE DEEE DEEE DEEE DEEE KM TEST BREAK

1C2F 21F3B4 ld h1,B4F3

1C32 CB56 bit  2,(hl)
1C34 C8 ret Zz
1C35 79 ld a,c
1C36 EEAO xor A0
1C38 2056 jr nz,1C90 KM BREAK EVENT
1C3A C5 push bc
1C3B 23 inc hl
1C3C 0604 ld b,0A
1C3E 8E adc a,thl)
1C3F 2B dec hl
1C40 10FC djnz 1C3E
1C42 C1 pop bc
1C43 FEA4 cp A4
1C45 2049 jr nz,1C90 KM BREAK EVENT
1Cu7 C7 rst O0
1C48 ES push hl
1C49 DS push de
1C4A SF ld e,a
1C4B 2F cpl

1C4C 3C inc a
1C4D A3 and e
1C4E 47 ld b,a

-239-

KEYBOARD MANAGER

1C4F 3AEAB4 ld a, (B4EA)

1C52 CD181C call 1C18

1C55 78 ld a,b

1C56 AB xor e

1C57 20F1 Jr nz, 1C4A

1C59 D1 pop de

1CSA E1 pop hl

1C5B C9 ret

LILS LL LLLLLLLLLL SSL SLILLSSSLLLLLLLLLSL IL LS SSL SL LL: KM GET JOYSTICK
1C5C 3AF1B4 ld a,(B4F1) (Joystick 1)

1C5F E67F and 7F

1C61 6F id 1,a

1C62 3AF4B4 ld a, (B4F4) (Joystick 0)

1C65 E67F and 7F

1C67 67 ld ha

1C68 C9 ret

LISLE LL LL LL LI LL LL LL SLI LIL LILI LILI LIL LL LILI LLLLLLSSS KM GET DELAY
1C69 2AE9B4 ld h1,(B4E9) (KM Delay)

1C6C C9 ret

RME MEME DEDE DE DD DE DE DEEE DE DEEE DE DD DEEE ED DE DEEE DEEE DE DD DE DE DEEE JE KM SET DELAY

1C6D 22E9B4 ld (B4E9),h1 (KM Delay)
1C70 C9 ret

LÉLLELLSL SL LL SL LL LS Lil Li) iii li Lili Li LL LL LL, , LE.) KM ARM BREAK

1C71 CD821C call 1C82 KM DISARM BREAK
1C74 210DB5 1d hl1,B50D Break Event Block
1C77 0640 ld b,40

1C79 CDD201 call 01D2 KL INIT EVENT
1C7C 3EFF ld a, FF

1C7E 320CB5 ld (B50C), a

1Cc81 C9 ret

LELL LS SL LS Li LL LL LL LL LL LL LS, 11:22: LLL LL SE, ES) KM DISARM BREAK

1C82 C5 push bc
1C83 D5 push de
1C84 210CB5 ld h1,B50C

-240-

1C87
1C89
1C8A
1C8D
1C8E
1C8F

3600
23
CD8502
D1

C1

C9

ld
inc
call
pop
pop
ret

KEYBOARD MANAGER

(h1),00

hl

0285 KL DEL SYNCHRONOUS
de

bc

DM IE DE DE DE MED DIE DE DE DE DE DE DE DE DE DE DE DE DE DE DE DE DE DE ED DD DE DE DE DE DE DE DE DE ED DE DE DE DE DE KM BREAK EVENT

1C90
1C93
1C94
1C96
1097
1C98
1C99
1C9A
1C9B
10
1CA0
1CA3
1CA4
1CA5

1CA6
1CA9
1CAB
1CAD
1CAE
1CB1
1CB4
1CB5
1CB6
1CB7
1CB8
1CB9
1CBA
1CBB
1CBC

210CB5
7E
3600
BE

C8

C5

D5

23
CDE201
OEEF
CDFE1C
D1

C1

C9

2A47B5
181D
FE50
DO
2A47B5
CDCD1C
UF

2F

A6

77

79

A0

B6

77

C9

ld
ld
ld
CP
ret
push
push
inc
call
ld
call
pop
pop
ret

ld
Jr
CP
ret
id
call
ld
cpl
and
ld
ld
and
or
id
ret

h1,B50C
a,(hl)
(h1),00
(hl)

Z

bc

de

hl

01E2 KL EVENT
c.EF
1CFE

de

bc

h1,(B547) (Adr. de table Repeat)

1CC8 fixer Z suivant bit touche
50 Key > 80 ?

nc oui => non valable
h1,(B547) (Adr. de table Repeat)
1CCD aller chercher bit corresp, touche #
c,a

(h1)

(hl),a

a,c

b (b=$ff/00)

(h1l)

{hl}),a

DEN DEEE DE DE MED DE DE DD DE DE DE DEDE DE DIE DE DD DE DE DEN DE DE DD DE DE DE DE DE DE DIE DE DE DIE NE KM TEST KEY

-241-

1CBD
1CBE
1CC1
1CC3
1CC4
1CC5
1CC8
1CCB
1CCC

F5
3AEDB4
E6AO
UF

F1
21EBB4
CDCDIC
A6

C9

push
ld
and
id
pop
ld
call
and
ret

KEYBOARD MANAGER

af

a, (B4ED)
AO

c,à

af
h1,B4EB
1CCD
(h1)

(Key 16...23)
isoler SHIFT/CTRL

Key State Map
aller chercher bit corresp. touche #
masquer bit touche

DDR ND DEDE DU DU DEN D DEN OU DEEE DE DE DEN DE DU DE DE DEEE DE aller chercher bit corresp. touche #

1CCD
1CCE
1CCF
1CD1
1CD2
1CD3
1CD4
1CD5
1CD7
1CD8
1CD9
1CDA
1CDD
1CDF
1CE0
1CE1
1CE2
1CE3
1CE4

D5
F5
E6F8
OF
OF
OF
5F
1600
19
F1
ES
21E51C
E607
5F
19
7E
EI
D1
C9

push
push
and
rrca
rrca
rrca
ld
ld
add
pop
push
ld
and
ld
add
ld
pop
pop
ret

de
af
F8

e,a
d,00
hl,de
af

hl
h1,1CE5
07

e,a
hl,de
a, (h1)
hl

de

Key#
/8

adresser Key Map

Charger carte bits
correspondant
à la touche

DEDEDEDE DENON DE UE DEAD DE DE DU DU DE OU DE DEEE DÉ DE DU OU DUO DE DE DE DEN DEDE Cartes bits

01 02 04 08 10 20 40 80

1CE5

1CED
1CEE
1CF1
1CF3
1CF4
1CF5

F3
213CB5
3615
23

AF

77

di
1d
ld
inc
xor
ld

h1,B53C
(h1),15
hl

a
(hl1),a

-242-

KEYBOARD MANAGER

1CF6 23 inc  hl

1CF7 3601 1d (h1),01
1CF9 23 inc hl

1CFA 77 id (h1),a
1CFB 23 inc hl

1CFC 77 ld (h1),a
1CFD C9 ret

1CFE 213CB5 id h1,B53C
1D01 B7 or a

1D02 35 dec (h1)
1D03 280€ Jr Z,1D13
1D05 CD2C1D call 1D2C
1D08 71 td (h1),c
1D09 23 inc hi]

1D0A 70 ld (h1),b
1D0B 2140B5 ld h1,B540
1D0E 34 inc  (hl)
1D0F 213EB5 ld h1,B53E
1D12 37 scf

1D13 34 inc  (hl)
1D14 C9 ret

1D15 213EB5 ld h1,B53E
1D18 B7 or a

1D19 35 dec (h1)
1D1A 280€ jr Z,1D2A
1D1C CD2C1D call 1D2C
1D1F 4E ld c,(h1)
1D20 23 inc hl

1D21 46 ld b, (h1)
1D22 2140B5 ld h1,B540
1D25 35 dec (hl)
1D26 213CB5 ld h1,B53C
1D29 37 scf

1D2A 34 inc (h1)
1D2B C9 ret

1D2C 23 inc hl

1D2D 34 inc  (hl)

-243-

KEYBOARD MANAGER

1D2E 7E ld a, (hl)
1D2F FE14 Cp 14
1D31 2002 jr nz,1D35
1D33 AF xor a

1D34 77 ld (h1),a
1D35 87 add  a,a
1D36 CE14 adc a,14
1D38 6F Id 1,a
1D39 CEBS adc a,B5
1D3B 95 sub 1

1D3C 67 ld h,a
1D3D C9 ret

HEHEHE HE HD HE HE HENE HE DEMO DEDE DE DE DE DH DD ÉD DE DEN DH DE DEEE DE DEEE KM GET TRANSLATE

1D3E 2A41B5 ld h1,(B541) (Adr. Key Transl. Table)
1D41 1808 jr 1D4B Get Key Table

LEE SELS SSSR DS LIL LSLLLILLSSLIIILLLLIILILII SIL SE. KM GET SHIFT
1D43 2A43B5 ld h1,(B543) (Adr. Key SHIFT Table)
1D46 1803 jr 1D4B Get Key Table

ç

HEHHEDEHEDE EDMOND DD DE DEEE DE DEEE DE DE EN HE KM GET CONTROL
1D48 2AU5BS5 ld h1,(B545) (Adr. Key CTRL Table)

DH DH DEH DE HE HD DH CH HE DE DÉHE NH ED DEDÉOHEDEDEDÉDÉ DE DE DEEE EEE DE DEEE Get Key Table

1D4B 85 add a,1

1D4C 6F ld l,a
1D4D 8C adc ah
1D4E 95 sub 1]

1D4F 67 ld h,a
1D50 7E 1d a,(h1)
1D51 C9 ret

EDR DD DD DD DD DEEE DD DEEE DEEE DE DEEE KM SET TRANSLATE

1D52 2441B5 ld h1,(B541) (Adr. Key Transl. Table)
1D55 1808 jr 1D5F Set Key Table

DEN DE DENEHDEDENE MED HD DE DE DE DE DEEE DE DE DE DE HE DE DE DE DE DE DE DE DE DE DE DE JE DE DE DEN KM SET SHIFT
1D57 2A43B5 ld h1,(B543) (Adr,. Key SHIFT Table)
1D5A 1803 Jr 1D5F Set Key Table

-244-

KEYBOARD MANAGER

LÉ S ESS SELLES LL ELLLL LL SL LLL LL LLL EL ELL LIL LL LL SLT) KM SET CONTROL

1D5Ç 2A45B5 ld h1,(B545) (Adr. Key CTRL Table)
LILSLLLLLLLLSL LS S SLR LS LSLLSSSLSSSLSLLSLSLILLILLLSSSS. Set Key Table
1D5F FE50 cp 50

1D61 DO ret nc

1D62 85 add a,l

1D63 6F ld 1,a

1D64 8C adc a,h

1D65 95 sub ]

1D66 67 ld h,a

1D67 70 ld (h1),b

1D68 C9 ret

MEHEDE HD HEDE DE DENE HE DEHEHEDÉHE DD HD DE DEEE NENE Key Translation Table

1D69 FO F3 F1 89 86 83 8B 8A
1D71 F2 E0 87 88 85 81 82 80
1D79 10 5B OD SD 84 FF 5C FF
1D81 SE 2D 40 70 3B 3A 2F 2E
1D89 30 39 6F 69 6C 6B 6D 2C
1D91 38 37 75 79 68 6A 6E 20
1D99 36 35 72 74 67 66 62 76
1DA1 34 33 65 77 73 64 63 78
1DA9 31 32 FC 71 09 61 FD 7A
1DB1 OB OA 08 09 58 SA FF 7F

LES ELLE LES LL LS ESS LL IS SLL LILI LLLESLELLS LIL LIL LS LS. Key SHIFT Table

1DB9 F4 F7 F5 89 86 83 8B 8A
1DC1 F6 EO 87 88 85 81 82 80
1DC9 10 7B OD 7D 84 FF 60 FF
1DD1 A3 3D 7C 50 2B 2A 3F 3E
1DD9 SF 29 4F 49 4C LB UD 3C
1DE1 28 27 55 59 48 LA UE 20
1DE9 26 25 52 54 47 46 42 56
1DF1 24 23 45 57 53 44 43 58
1DF9 21 22 FC 51 09 41 FD 5A
1E01 OB OA 08 09 58 SA FF 7F

ED HD DE HD DD DD DE HE DE DE HEE HD DÉ DE HE D DE DE DEDÉDÉDE DE DEEE DE DD DE DE DEEE Key CTRL Table

1E09 F8 FB F9 89 86 83 8C 8A

-245-

1E11
1E19
1E21
1E29
1E31
1E39
1E41
1E49
1E51
1E59
1E61

1E63
1E64
1E65
1E66
1E67

FA EO
10 1B

87
0
00
OF
15
12
05
FC
FF
4B

88 85 81
1D 84 FF
10 FF FF
09 OC 0B
19 08 OA
14 07 O6
17 13 O4
11 E1 01
FF FF FF
FF FF FF

rst
rst
rst
rst
rst

82 80
1C FF
FF FF
OD FF
OE FF
02 16
03 18
FE 1A
FF 7F
FF FF

OO ©O0OOO

KEYBOARD MANAGER

-246-

SOUND MANAGER

2.5.8 SOUND MANAGER (SOUND)

Il n'y a pas grand chose à dire sur ce pack. La production du son
proprement dite y prend en fait peu de place. La plus grande partie est
occupée par la gestion des diverses files d'attente au rang desquelles
figure également la réalisation de la TONE ENVELOPPE, que le PSG ne
maîtrise pas de lui-même.

L'amateur de musique préfèrera sans doute programmer directement le PSG
car les routines du SOUND sont trop taillées sur mesure pour les
instructions Basic correspondantes. Pour Jouer des mélodies, même à trois
voix et même avec un tempo rapide, le Basic est très suffisant.

Pour le programmeur en langage-machine il serait tout au plus intéressant
de réaliser une bonne percussion (c'est-à-dire avec des changements de
son importants), ce qui n'est qu’imparfaitement possible en Basic avec
des sons brefs mais complexes.

-11 1-

SOUND MANAGER

LELLELLLLLL SELS SSL ES LL LL LS LL LL LL ESS LES LL SL L LL) SOUND RESET

1E68
1E69
1E6A
1E6D
1E70
1E73
1E76
1E78
1E7B
1E7D
1E80
1E83
1E86
1E89
1E8A
1E8B
1E8C
1E8D
1E8E
1E8F
1E90
1E91
1E92
1E93
1E94
1E96
1E98
1E9A
1E9C
1E9D
1EAO
1EA1
1EA4
1EAS
1EA7
1EA9
1EAA
1EAB
1EAD

AF

F3
3252B5
3251B5
2155B5
11031F
0681
CDD201
3E3F
3219B6
215CB5
013D00
110801
AF

77

23

72

23

73

09

3C

EB

29

EB
FEO3
20F2
0E07
DDES
ES
211DB5
u1
113F00
19
CB38
30F8
C5

E5
DDE1
EB

xor
di
ld
ld
ld
ld
ld
call
ld
ld
ld
ld
ld
xor
ld
inc
ld
inc
ld
add
inc
ex
add
ex
Cp
jr
ld
push
push
1d
ld
ld
add
srl
jr
push
push
pop
ex

a

(B552),a (activité SOUND act.)
(B551),a (ancienne act. SOUND (d’après HOLD))
h1,B555 Sound Event Block
de,1F03 Sound Event

b,81

01D2 KL INIT EVENT

a, 3F

(B619),a

h1,B55C  SOUND Params Canal A
bc,003D

de,0108

a

(h1),a

hl

(hl),d

hl

(hl),e

hl,bc

a

de,hl

fhlhl

de,hl

03

nz, 1E8A

c,07

ix

hl

h1,B51D

b,c

de,003F

hl,de

D

nc, 1EA1

bc

fl

ix

de,hl

-I1 2-

SOUND MANAGER

1EAE CD/F22 call 227F
1EB1 13 inc de

1EB2 13 inc de

1EB3 13 inc de

1EB4 6B ld l,e
1EB5 62 ld hd
1EB6 13 inc de

1EB7 013B00 ld bc,003B
1EBA 3600 ld (h1),00
1EBC EDBO ldir

1EBE DD361C04 ld Cix+1C),04
1EC2 C1 pop bc

1EC3 EB ex de,hl
1EC4 Où inc b

1ECS 10DE djnz 1EA5
1EC7 E1 pop hl

1EC8 DDE1 pop ix

1ECA C9 ret

DH DH DEHEDEHE HE EH HE DEEE DEEE DEEE DEEE DD DEDE DE DE DEEE SOUND HOLD

1ECB 2152B5 ld h1,B552 Activité SOUND act.
1ECE F3 di

1ECF 7E ld a, (hl)

1ED0 3600 ld (h1),00

1ED2 FB ei

1ED3 B7 or a Canaux actifs ?
1ED4 C8 ret Zz Non =>

1ED5 2B dec hl

1ED6 77 ld (hl),a

1ED7 2E03 ld 1,03 volume

1ED9 0E00 ld c,00 de tous les canaux
1EDB 3E07 ld a,07 sur 0

1EDD 85 add  aà,1l

1EDE CD2608 call 0826 MC SOUND REGISTER
1EE1 2D dec 1]

1EE2 20F7 ir nz,1EDB

1EE4 37 scf

1ÉE5 C9 ret

SHEDEDE DIE DE DE D DE DE DEEE OH DE HE DE DE DE DE DE DE DE DE DE DE DE DE DE DE HE DÉ HE DE DE DE DE DE DE DEIE DE SOUND CONTINUE

-11 3-

1EE6
1EE9
1EEA
1EEB
1EEF
1EF2
1EF4
1EF6
1EF7
1EFA
1EFD
1EFE
1F00

1F03
1F05
1F08
1F09
1FOA
1F0B
1F0C
1FOD
1F0E
1F0F
1F10
1F12
1F16
1F19
1F1B
1F1D
1F1F
1F20
1F23
1F24
1F27
1F2A
1F2B
1F2E

SO

UND MANAGER

3A51B5 ld a,(B551) (ancienne act. SOUND (d'après HOLD))
B7 or a Canal actif ?
C8 ret 2z non =>
DD211DB5 ld 1x,B51D
113F00 ld de, 003F
DD19 add ix,de
CB3F srl a fixer ancien
F5 push af volume
DD/EOF ld a, (1x+0F) pour tous les canaux
DC7622 call c,2276
F1 pop af
20F2 Jr nz, 1EF2
C31E20 Jp 201E
LELLLL ELLE LLLLLLLL LS LSSELELL LL LLLLLLLLLLLLLLLLL EL) Sound Event
DDES push ix
2150B5 ld h1,B550
ES push hl
AF xor a
77 ld (h1),a
23 inc hl
46 ld b,(h1)
C5 push bc
23 inc  hl y a-t-il un
B6 or (h1) canal actif ?
2822 ir Z,1F34 non =>
DD211DB5 ld 1x, B51D
013F00 ld bc, 003F
DDO9 add ix,bc
CB3F srl a Canal actif ?
30FA Jr nc;1F19 non => suivant
F5 push af
DD7E04 ld a, (1x+04)
1F rra
DCC222 call c,22C2
DD7E07 ld a, (1x+07)
1F rra
DCB621 call c,21B6
DCA820 call c,2048
F1 pop af

1F31

-I1 4

SOUND MANAGER

1F32 20E2 Jr nz,1F16
1F34 C1 pop bc

1F35 El pop hl

1F36 7E id a,(hl)
1F37 B7 or a

1F38 2820 ir Z,1F5A
1F3A 4F 1d c,a
1F3B 23 inc  hl

1F3C 7E ld a,(hl)
1F3D 70 ld (h1),b
1F3E A8 xor  b

1F3F 47 id b,a
1F40 23 inc _ hl

1F41 B6 or (h1)
1F42 77 ld (hl),a
1F43 78 ld a,b
1F44 2F cpl

1F45 A1 and c

1F46 2812 Jr Z,1F5A
1F48 DD211DB5 ld 1x, B51D
1F4C 113F00 ld de, 003F
1F4F DD19 add 1ix,de
1F51 CB3F srl a

1F53 F5 push af

1F54 DC7F22 call c,227F
1F57 F1 pop af

1F58 20F5 Jr nz,1F4F
1F5A AF xor a

1F5B 3254B5 id (B554),a
1FSE DDE1 pop ix
1F60 C9 ret

LRSLLLL LES LL LL LL LT II LL LL LILI LL LIL LLLLL LIL LL LL LL SL) Scan Sound Queues
1F61 2152B5 ld h1,B552 activité SOUND act.
1F64 7E ld a,(hl)
1F65 B7 or a

1F66 C8 ret 2z

1F67 23 inc hl

1F68 35 dec  (hl)
1F69 CO ret nz

-I1 5-

SOUND MANAGER

1F6A 34 inc  (hl)
1F6B 23 inc hl

1F6C 7E ld a, (hl)
1F6D B7 or a

1F6E CO ret nz

1F6F  2B dec hl

1F70 3603 ld (h1),03
1F72 2B dec hl

1F73 46 ld b,(hl1)
1F74 2122B5 ld h1,B522
1F77 113F00 ld de,003F
1F7A AF xor a

1F7B 19 add hl,de
1F7C CB38 srl b

1F7E 30FB Jr nc, 1F7B
1F80 35 dec (hl)
1F81 2005 jr nz,1F88
1F83 2B dec hl

1F84 CB06 rlc (hl)
1F86 8A adc a,d
1F87 23 inc hl

1F88 23 inc hl

1F89 35 dec  (hl)
1F8A 2005 jr nz, 1F91
1F8C 23 inc  hl

1F8D CB06 rlc (hl)
1F8F 8A adc a,d
1F90 2B dec hl

1F91 2B dec hl

1F92 Où inc b

1F93 10€6 djnz 1F7B
1F95 B7 or a

1F96 C8 ret 7z

1F97 2154B5 ld h1,B554
1F9A 77 ld (h1),a
1F9B 23 inc hl

1F9C C3E201 Jp 01E2 KL EVENT

DEN ND NUE DE DE DE D DE DU DD DD DEDEDE HE DEN DE DE HD DE De D DE DEEE DE HE DE DEN DE DE DE SOUND QUEUE

1F9F CDE61E call 1EE6 SOUND CONTINUE

-11 6-

1FA2
1FA3
1FA5
1FA6
1FA7
1FA8
1FA9
1FAC
1FAD
1FB1
1FB4
1FB5
1FB7
1FB9
1FBB
1FBE
1FC1
1FC2
1FC3
1FC4
1FC6
1FC7
1FC8
1FC9
1FCA
1FCB
1FCC
1FCD
1FCE
1FDO
1FD1
1FD2
1FD6
1FD9
1FDB
1FDD
1FDF
1FE0
1FE1
1FE4

7E
E607
37

c8

#F

B6
FC9AIE
41
DD211DB5
113F00
AF
DD19
CB38
30FA
DD721E
DDBE1C
3F

9F

04
10EF
B7

co

u1

7E

1F

1F

1F

BO
E60F
UF

23
DD211DB5
113F00
DD19
CB38
30FA
ES

C5
DD/E1B
DD341B

id
and
scf
ret
ld
or
call
ld
ld
ld
xor
add
srl
Jr
ld
cp
ccf
sbc
inc
d}jnz
or
ret
ld
ld
rra
rra
rra
or
and
ld
inc
ld
ld
add
srl
Jr
push
push
ld
inc

SOUND MANAGER

8, (hl)
07

Z
c,a

(h1)

m, 1E9A.
b,c

ix, B51D
de, 003F

a

ix, de

D

nc, 1FB5
(ix+1E),d
Cix+1C)

a,

b

1FB5
a

nz
b,c

a, (hl)

b

OF

c,a

hl

ix, B51D
de, 003F
ix, de

b

nc, 1FD9
hl

bc

a, (ix+1B)
(ix+1B)

-I1 7-

SOUND MANAGER

1FE7 DD351C dec (ix+1C)
1FEA EB ex de,hl
1FEB CD3A20 call 2034
1FEE ES push hl

1FEF EB ex de,hl
1FFO DD7E01 id a, (ix+01)
1FF3 2F cpl

1FF4 A and c

1FF5 12 ld (de),a
1FF6 13 inc de

1FF7 7E ld a,(hl)
1FF8 23 inc  hl

1FF9 87 add a

1FFA 87 add a

1FFB 87 add a

1FFC 87 add a

1FFD 47 ld b,a
1FFE 7E id a,(hl)
1FFF 23 inc hl

2000 E60F and OF

2002 BO or D

2003 12 ld (de),a
2004 13 inc de

2005 010600 id bc, 0006
2008 EDBO ldir

200A El pop hl

200B F3 di

200C DD/E1A id a, (1x+1A)
200F DD341A inc (ix+1A)
2012 DDB603 or (ix+03)
2015 FB ei

2016 CCBD20 call Zz,20BD
2019 C1 pop bc

201A EI pop hl

201B O4 inc b

201C 10B8 dinz 1FD6
201E ES5 push hl

201F 2151B5 ld h1,B551 ancienne act. SOUND (d’après HOLD)
2022 7E ld a, (hl)
2023 B7 or a

-I1 8-

SOUND MANAGER

2024 2811 Jr Z, 2037
2026 3600 ld (h1),00
2028 F3 di

2029 23 inc hi
202A 46 ld b,(h1)
202B BO or b

202C 77 ld (hl),a
202D 78 ld a,b
202E B7 or a

202F 2005 Jr nz, 2036
2031 23 inc hl
2032 3603 ld (h1),03
2034 23 inc hl
2035 77 ld (hl),a
2036 FB ei

2037 El pop h]
2038 37 scf

2039 C9 ret

203A E603 and 03
203C 87 add  a,a
203D 87 add  a,a
203€ 87 add  a,a
203F C61F add  a,1F
2041 DDES push ix
2043 E1 pop hl
2044 85 add  a,1
2045 6F ld 1,a
2046 8C adc a,h
2047 95 sub ]!

2048 67 ld h,a
2049 C9 ret

LLLLILLILLELLL IS SLI LL IL I LL LIL LLILLL LL LL LL LL LIL...) SOUND RELEASE

204A 6F 1d 1,a

204B CDEGIE call 1EE6 SOUND CONTINUE
204E 7D id a,]

204F E607 and 07

2051 C8 ret Zz

2052 DD211DB5 id ix, B51D

-II 9-

SOUND MANAGER

2056 113F00 ld de, 003F
2059 DD19 add ix,de
205B CB3F srl à

205D 30FA Jr nc, 2059
205F F5 push af

2060 DDCBO35E bit  3,(ix+03)
2068 20EC jr nz, 2056
2064 18B2 Jr 201E
LELLLLLLELLILLLLLILLL LI STILL SL LLSSLSLILLLI LL ILL SL] SOUND CHECK
206€ E607 and 07

206E C8 ret z

206F 2120B5 ld h1,B520
2072 113F00 1d de,003F
2075 19 add hl,de
2076 1F rra

2077 30FC jr nc, 2075
2079 F3 di

207A 7E ld a,(hl)
207B 87 add  a,a
207C 87 add  a,a
207D 87 add  a,a
207E 111900 ld de,0019
2081 19 add hl,de
2082 B6 or (h1)
2083 23 inc hl

2084 23 inc hl

2085 3600 ld (h1),00
2087 FB el

2088 C9 ret

LÉSLSS SSSR LL LL LL SLLLSLS LL LLSRSLSLLS SSD LS SSS SSL SSL L LS] SOUND ARM EVENT
2089 E607 and 07

208B C8 ret 2z

208C EB ex de, hi
208D 2139B5 ld h1,B539
2090 013F00 ld bc,003F
2093 09 add hl,bc
2094 1F rra

2095 30FC jr nc, 2093

-I1 10-

2097
2098
2099
209A
209B
209€
209D
209F
20A0
201

20A2
2043
20A4
20A5

2048
20AB
20AC
20AF
20B2
20B5
20B6
20B7
20BA
20BD
20BE
20BF
2001
2003
2005
20C6
20C8
20CB
20CC
20CD
20D1
20D2
20D3
20D5

AF
F3
BE
23
73
23
2003
72
FB
cg

77
FB
EB
C3E201

DD7E1A
B7
CA7F22
DD7E01
2150B5
B6

77
DD7E19
CD3A20
7E

B7
280€
CB5F
2053
ES
3600
CD1F21
E1

DO
DD360310
23

7E
E6FO
F5

xor
di
cp
inc
ld
inc
jr
ld
ei
ret

1d
ei
ex
jp

id
or
jp
ld
1d
or
id
ld
call
ld
or
jr
bit
Jr
push
ld
call
pop
ret
ld
inc
ld
and
push

SOUND MANAGER

a

(h1)
hl

(h1).e

hi

nz, 2042
(h1),d

(hl),a

de,hl
O1E2

KL EVENT

a, (ix+1A)

a
Z, 227F

a, (ix+01)
h1,B550

(h1)

(hi),a
a, (ix+19)

203A
a, (h1l)
a

Z, 20CD
3,a
nz,211
hi
(h1),0
211F
hl

nc
(1x+03
hl
a,(h1)
FO

af

8

0

),10

-I1 11-

SOUND MANAGER

Z,20E9

hl

224B

d, (ix+01)

2175

h1,B551 Ancienne act. SOUND (d’après HOLD)

a, (ix+01)
(h1)
(h1),a
(ix+19)
Cix+1A)
(ix+1C)

a, (1x+1E)
(1x+1E),00

a
Z

h,a

1, (ix+1D)

01E2 KL EVENT

-I1 12-

2118
211À
211E

211F
2121
2122
2125
2129
212B
212D
2131
2133
2135
2139
213A
213)
213€
2140
2141
2144
2146
2148
214C
214E
2150
2154
2157
2158
215A
215B
215E
2160
2164
2165
2168
216A
216B

216C

CBS
DD360308
C9

DDES

47
DD4EO1
DD215CB5
CB47
200€
DD219BB5
CB4F
2004
DD21DABS
F3
DD7E03
A1

282D

78
DDBEO1
281A
DDES
DD21DAB5
CB57
2004
DD219BB5
DD7E03
A1

2812

FB
CDB720
DDE1
DD360300
FB
CDB720
DDE1

37

C9

El

res
1d
ret

push
ld
ld
id
bit
jr

bit
Jr
ld.
di
ld
and
Jr
ld
cp
Jr
push
ld
bit
ir
Id
ld
and
Jr
ei
call
pop
ld
ei
call
pop
scf
ret

pop

SOUND MANAGER

3,(h1)
(ix+03),08

1x

b,a

c,(ix+01)

1x, B55C  SOUND Params Canal A
0,a

nz, 2139

1x, B59B  SOUND Params Canal B
1,a

nz, 2139

1x, B5DA  SOUND Params Canal C

a, (1x+03)

C

Z,216D

a,b

(ix+01)

z,2160

ix

1x, B5DA  SOUND Params Canal C
2,a

nz,2154

1x, B59B  SOUND Params Canal B
a, (1x+03)

C

z,216C

20B7
ix
(ix+03),00

20B7
1x

hl

-I1 13-

216D
216F
2172
2173
2174

2175
2177
217A
217B
217C
217D
217F
2180
2183
2186
2187
2188
218A
218C
218F
2192
2193
2196
2197
2198
219A
219D
219F
21A0
21A1
21A2
21A4
21A7
21AA
21AD
21B0
21B2
21B5
21B6

DDE1
DD7003
FB

B7

C9

CBFB
DD730F
5F

7D

B4
2001
2B
DD7508
DD7409
79

B7
2808
3E06
CD2608
DD7E02
B2
CD8B22
7B

B7
280A
210AB6
1600
19

7E

B7
2003
21B221
DD7S50A
DD74OB
CD6522
180D
010100
C8
DD6EOD

pop
ld
ei
or
ret

set
ld
ld
ld
or
Jr
dec
ld
ld
ld
or
jr
ld
call
ld
or
call
ld
or
jr
ld
ld
add
ld
or
Jr
ld
ld
ld
call
jr
ld
ret
ld

SOUND MANAGER

1x
(1x+03),b

7,e
Cix+0F),e
e,a

a, l

h

nz, 2180
hl
(1x+08),1
(1x+09),h
a, C

a

Z,2192
a,06 charger générateur de bruit
0826 MC SOUND REGISTER
a, (1x+02)
d

228B

a,e

a

Z,21A4
h1,B60A Courbes d'’enveloppe de volume
d,00
hl,de

a, (hl)

a

nz, 2147
h1,21B2
(1x+0A),1
Cix+0B),h
2265

21BF
bc,0001

Z

1, (1x+0D)

-I1 14-

21B9
21BC
21BF
21C0
2102
21C4
2105
21C6
21C7
21C9
21CB
21CC
21CD
21CF
21D2
21D5
218
21DA
21DD
21DE
21E1
21E2
21E3
21E5
21E6
21E7
21EA
21EC
21ED
21F0
21F3
21F4
21F5
21F8
21FB
21FC
21FE
2200
2201
2202

DD660E
DD5E10
7B
FEFF
2876
87

7E

23
38UA
280D
1D

B7
2006
DDB60F
F2DD21
DD860F
E60F
CD7322
LE
DD7E09
47

87
381B
AF

91
DD8608
380C
05
F2F521
DDUE O8
AF

u7
DD7009
DD7708
BO
2002
1EFF
7B

B7
CCH622

SOUND MANAGER

id h, (ix+0E)
id e, (ix+10)

ld a,e

cp FF

Jr Z, 223A
add  a,a

ld a,(hl)
inc hl

Jr c,2213
Jr z,21D8
dec e

or a

Jr nz,21D5
or (1x+0F)
Jp p,21DD
add a,(ix+0F)
and OF

call 2273 fixer volume
ld c;,(hl)

ld a, (ix+09)
ld b,a

add a,a

jr C, 2200
xor a

sub c

add a,(ix+08)
Jr c,21F8
dec Db

JP p,21F5

ld c, (ix+08)
xor a

id b,a

ld (ix+09),b
ld (1x+08),a

or b

Jr nz, 2200
ld e,FF

ld a,e

or a

call z,2246

-I1 15-

SOUND MANAGER

2205 DD7310 ld (ix+10),e

2208 F3 di

2209 DD7106 ld (1x+06),c

220C DD360780 ld (1x+07),80

2210 FB ei

2211 B7 or a

2212 C9 ret

2213 57 ld d,a

2214 4B ld c,e

2215 3EOD ld a,0D Courbe d’enveloppe
2217 CD2608 call 0826 MC SOUND REGISTER
2214 4UA id c,d

221B 3EOB ld a,0B longueur de courbe d'’enveloppe Lo
221D CD2608 call 0826 MC SOUND REGISTER
2220 4E ld c,(h1)

2221 3EOC ld a,0C longuer de courbe d’enveloppe Hi
2223 CD2608 call 0826 MC SOUND REGISTER
2226 3E10 ld a,10

2228 CD/7322 call 2273 fixer volume

222B CD4622 call 2246

222€ 7B id a,e

222F 3C inc a

2230 208D Jr nz,21BF

2232 21B221 ld h1,21B2

2235 CD6522 call 2265

2238 1885 Jr 21BF

223A AF xor a

223B DD7703 ld (1x+03),a

223€ DD7707 ld (1x+07),a

2241 DD7704 ld (1x+04),a

2244 37 scf

2245 C9 ret

2246 DD350C dec (ix+0C)

2249 201E Jr nz, 2269

224B DD7E09 id a, (1X+09)

2Q2UE 87 add a,a

224F 21B221 ld h1,21B2

2252 3011 jr nc, 2265

-11 16-

SOUND MANAGER

2254 DD3408 inc  (ix+08)
2257 2006 Jr nz, 225F
2259 DD3409 inc (ix+09)
225C 1EFF ld e,FF

225€ C8 ret z

225F  DD6EOA ld 1, (1x+04A)
2262 DD660B ld h, (ix+0B)
2265 7E ld a, (hl)
2266 DD770C 1d (1x+0C),a
2269 23 inc  hl

2264 SE ld e,(hl)
226B 23 inc hl

226C DD750D ld (1x+0D),1
226F DD740E ld (1x+0E),h
2272 C9 ret

LÉLLELELSELELLLL LISE LELLIL ELLES ELLES LLL LILI LLLL TL): ) fixer volume

2273 DD770F ld (ix+0F),a

2276 4F ld c,a

2277 DD/E00 ld a, (1x+00)

2274 C608 add  a,08 Volume
227C C32608 Jp 0826 MC SOUND REGISTER
227F DD/E01 ld a, (ix+01)

2282 2F cpl

2283 2152B5 ld h1,B552 activité SOUND act.
2286 F3 di

2287 A6 and  (hl)

2288 77 ld (h1),a

2289 FB ei

228A AF xor a

228B 47 ld b,a

228C DD/E01 ld a, (1x+01)

228F  DDB602 or (1x+02)

2292 2119B6 id h1,B619

2295 F3 di

2296 B6 or (h1)

2297 A8 xor bb

2298 BE cp (h1)

2299 77 id Chl),a

QE CP

229A
229B
229D
229E
229F
22A0
22A1
22A4
22A5
22A6
22A8

22AB
22AE
22AF
22B2
22B3
22B4
22B6
22B7
22BA
22BD
2200
2202
2205
22C8
22CB
22CC
22CD
22CE
22D0
22D2
22D4
22D6
22D7
22D8
22D9
22DA
22DB
22DE

3E07
C32608

CD2423

call

inc

sub
jr
ld
jr
dec

add
sbc
ld
ld
add

SOUND MANAGER

nz, 22A0
a,b

a

nz

a

2276

c,(h1l)
a,07 Registre de commande - canal
0826 MC SOUND REGISTER

2324

a,e

234E SOUND T ADDRESS
nc

a,(h1)

7F

Z
(ix+11),1
(1x+12),h
2313
22CB
1,(1x+14)
h, (1x+15)
e, (1x+18)
c,(hl)

hl

a,e

FO
c22D6
e,00
22E4

e

a, C

a,a

a,a

d,a

a, (1x+16)
a,C

-11 18-

22DF
22E0
22E3
22E4
22E5
22E8
22E9
22EA
22EB
22ED
22F0
22F1
22F3
22F6
22F9
22FA
22FC
22FE

2302

2303
2306
2309
230A
230D
2311
2312

2313
2316
2317
2318
2319
231C
231F
2320
2321
2322
2323

2324

4e
DD7E 17
8A

57
CD2423
UE

7B

B7
2019
DD7E13
3D
2010
DD6E11
DD6612
7E
C680
3805
DD360400
cg
CD1323
DD7318
F3
DD7105
DD360480
FB

cg

DD7713
23
5€
23
DD/514
DD7415
7B
B7
CO
1C
C9

DD7E00

id
ld
adc
ld
call
ld
ld
or
jr
ld
dec
Jr
ld
ld
ld
add
jr
ld
ret
call
id
di
ld
ld
ei
ret

ld
inc
ld
inc
ld
ld
ld
or
ret
inc
ret

id

SOUND MANAGER

c,a

a, (ix+17)
a,d

d,a

2324
c,(h1)
a,e

a

nz, 2306
a, (1x+13)
a

nz, 2303
1,(1x+11)
h, (ix+12)
a, (hl)
a,80

c, 2303
(1x+04),00

2313
(1x+18),e

Cix+05),c
(ix+04),80

(ix+13),a
hl

e,(hl)

hl
Cix+14),1
(1x+15)h
a,e

a

nz

e

a, (1x+00)

-11 19-

SOUND MANAGER

2327 87 add  a,a

2328 F5 push af

2329 DD7116 ld (1x+16),c Hauteur de ton Lo

232C CD2608 call 0826 MC SOUND REGISTER

232F F1 pop af

2330 3C inc a

2331 LA ld c,d

2332 DD7117 ld (x+17),c Hauteur de ton Hi

2335 C32608 Jp 0826 MC SOUND REGISTER

LIL LILI LL LIL LLEL LIL LI LILLELLLLLLLLLLLLLLLL LL. EX] SOUND AMPL ENVELOPE
2338 110AB6 ld de,B60A Courbe d’enveloppe de volume
233B 1803 Jr 2340 copier courbe d’enveloppe

LS SELS LS LL EL EL LL LE LL LL LL LL LL LL LIL LL: ):::)] SOUND TONE ENVELOPE

233D 11FAB6 ld de,B6FA Courbes d’enveloppe de ton

LELEL ELLES LES LL LI LL LL LLLILI LILI I LLIL LIL LL 12): copier courbe d'enveloppe

2340 EB ex de,hl

2341 CD5123 call 2351 aller chercher adr. courbe d’envel.
2344 EB ex de,hl

2345 DO ret nc

2346 EDBO ldir

2348 C9 ret

CLLLLELLLILLLLLLELLILLIILLIIILLLLILLLLILL LL LILI LL) SOUND A ADDRESS

2349 210AB6 id h1,B60A Courbes d’enveloppe de volume

234C 1803 jr 2351 aller chercher adr. courbe d'envel.

LL L LES SELLE ELLE LS LL LL ELLE I TTL LL LIST 22111121] SOUND T ADDRESS

234E 21FAB6 ld h1.B6FA Courbes d'enveloppe de ton

LELLLL LL SELLE LLLL LL LILI SL LL LIL LIL LS LL LIL] aller chercher acr. courbe d'envel.
2351 B7 or a

2352 C8 ret 2z

2353 FE10 cp 10

2355 DO ret nc

2356 011000 ld bc, 0010
2359 87 add a,a
235A 87 add a,a

-11 20-

235B
235C
235D
235E
235F
2360
2361
2362
2363

2364
2365
2366
2367
2368
2369
236A
236B
2360
236D
236€
236F

add
add
add
ld

adc
sub
ld

scf
ret

rst
rst
rst
rt
rst
rst
rst
rst
rst
rst
rst
rst

a,a
a,a
a, 1
1,a
a,h

ha

O0O0O0O0OCOO0O0OCOoOCoO

SOUND MANAGER

-I1 21-

CASSETTE MANAGER

2.5.9 CASSETTE MANAGER (CAS)

Le rôle de ce pack va de soi. L'utilisation des différentes routines ne
présente pas de réel intérêt pour le programmeur en langage-machine car
les programmes professionnels ne font pas en général bon ménage avec le
lecteur de cassette. Le lecteur de disquette est en effet beaucoup plus
satisfaisant dans ce cas.

Voici cependant quelques routines de base qui sont utilisables:

CAS IN OPEN ouvre un fichier d'entrée, Il faut pour cela placer en b la
longueur du nom de fichier, en hl l'adresse de début du nom de fichier et
en de l'adresse de début d’une zone de la Ram de 2 K qui sera utilisée
comme buffer d'entrée.

Au retour de la routine, hl contient l'adresse -de début de la tête de
fichier (header).

a, bc et de contiennent d’autres valeurs tirées du header que vous pouvez
cependant retirer vous-même directement du header, puisque vous disposez
de l'adresse à laquelle il se trouve.

Les flags carry et zéro vous informent sur le succès de l'opération:
Carry=1 et zéro=0 signifient que tout a bien marché.

Carry=0 et zéro-0 signifient qu’il y a déjà un autre fichier d'ouvert.

Si la touche ESC a été enfoncée, carry=0 et zéro=1.

CAS OUT OPEN ouvre un fichier en sortie. Les paramètres à transmettre et
la signification des flags sont les mêmes que ci-dessus. Naturellement,
de doit ici contenire l'adresse du buffer de sortie.

CAS IN CHAR va chercher un caractère dans le buffer d'entrée et le
transmet à travers a. Si c'était le dernier caractère du buffer, un
nouveau bloc est automatiquement lu sur la cassette.

Si carry=0 et zéro=0, c'est que la fin du fichier (EOF) a été atteinte ou
que le fichier n'était pas ouvert. Les autres combinaisons ont le même
sens que ci-dessus,

CAS OUT CHAR écrit le caractère qui se trouve dans a dans le buffer de
sortie, Si celui-ci est plein, il est automatiquement copié sur la
cassette.

La signification des flags est la même que ci-dessus.

-11 22-

CASSETTE MANAGER

ES SL LS LS LT OUNS INITIALISE

2370 CDO124 call 2401 CAS IN ABANDON

2373 CD2E24 call 242E CAS OUT ABANDON
2376 AF xor a

2377 CD8E23 call 238E CAS NOISY

237A 214D01 ld h1,014D

237D 3E19 ld a,19

AS SSL TON NS SET SPEED
237F 29 add hl1,hl

2380 29 add hlhl

2381 29 add hl,hl

2382 29 add hl,hl

2383 29 add hLhl

2384 29 add hl,hl

2385 OF rrca

2386 OF rrca

2387 E63F and 3F

2389 6F ld 1,a

238A 22D1B8 ld (B8D1),h1l (Cass. Speed)

238D C9 ret

AE ADDED ADDED DDRM DD EE EEE EEE DEEE EC AG NOISY
238E 3200B8 ld (B800),a (Cass. Message Flag)
2391 C9 ret

HERMIONE DÉDUIT CAC IN OPEN

2392 DD2102B8 id ix, B802 Input Buffer Status
2396 CDAF23 call 23AF CAS Open

2399 DO ret nc

2394 ES | push hl

239B CD3F25 call 253F lire header fichier
239, ED5B1CB8 ld de, (B81C)

23A2 ED4B1FB8 ld bc, (B81F)

23A6 3A19B8 id a, (B819)

23A9 El pop hl

23AA C9 ret

ES LL TOUS OUT OPEN

23AB DD2147B8 ld 1x, B847 Output Buffer Status

-11 23-

CASSETTE MANAGER

SSL ON CS Open

23AF DD7E00 ld a, (1x+00)
23B2 B7 or a

23B3 CO ret nz
23B4 DDES push 1x
23B6 E3 ex (sp),h1
23B7 3601 ld (h1),01
23B9 23 inc  hl

23BA 73 ld (hl),e
23BB 23 inc  hl
23BC 72 ld (h1),d
23BD 23 inc  hl

23BE 73 ld (hl),e
23BF 23 inc hl
23C0 72 id (hl),d
2301 23 inc hl

23C2 EB ex de,hl
23C3 E1 pop hl

23C4 DS push de
23C5 OE40 ld c,40
23C7 12 id (de),a
23C8 13 inc de
23C9 OD dec c

23CA 20FB Jr nz, 23C7
23CC D1 pop de
23CD DS push de
23CE 78 ld a,b
23CF FE10 cp 10

23D1 3802 jr c,23D5
23D3 0610 ld b,10
23D5 O4 inc b

23D6 48 id c,b
23D7 1807 jr 23E0
23D9 E7 rst 4

23DA 23 inc hl

23DB CDB627 call 27B6
23DE 12 id (de),a
23DF 13 inc de
23E0 10F7 djnz 23D9
23E2 OD dec c

-11 24-

23E3
23E5
23E6
23E7
23E9
23EB
23EC
23EE
23EF
23F3
23F7
23FA
23FB

DD361501
DD361716
DD351C
37

C9

ret

CASSETTE MANAGER

Z,23ÈE

(1x+15),01
Cix+17),16
Cix+10)

NN UE DE AUDE EU DD DD DR UD UE CAS IN CLOSE

23FC
23FF
2400

3A02B8
B7
c8

ld
or
ret

a, (B802)
a
Z

(Input Buffer Status)

MARDI AUDE DEEE DDR EU UE DE QUEUE DE CAC IN ABANDON

2401
2404
2406
2408
2409
240A
240B
240C
240F
2410
2411
2412
2413
2414

2102B8
3E01

ld
ld
ld
inc
ld
inc
id
ld
xor
scf
ret
ld
sbc
ret

h1,B802
à, 01
(h1),00

Input Buffer Status

nz
(h1),a
a, à

DU DR AUDE DU DE UD DE EDEN AUDE DH DE DUREE DEEE CAS OUT CLOSE

2415
2418
2414
241C

3A47B8
FEO4
2812
C6FF

ld
cp
jr
add

a, (B847) (Output Buffer Status)
04

Z,242E CAS OUT ABANDON

a,FF

-I1 25-

CASSETTE MANAGER

241E DO ret nc

241F 215DB8 id h1,B85D

2422 36FF id (h1),FF

2424 23 inc  hl

2425 23 inc hl

2426 7E ld a,(hl)

2427 23 inc hl

2428 B6 or (h1)

2429 37 scf

242A C41426 call nz,2614

242D DO ret nc

SPEED MERDE DD DE DE DE DE DE GED DD DD DEEE EEE DE DEEE DE DE DE DE DE SEC AS OUT ABANDON
242E 2147B8 id h1,B847 Output Buffer Status
2431 3E02 ld a,02

2433 18D1 jr 2406

PDU UNE UE UE DE DR AE AE DE DEAD AD ASE AE EE DE AE QE AE DE DE AE EDR EEE CAS IN CHAR
2435 ES push hl

2436 D5 push de

2437 CS push bc

2438 0602 id b,02

243A CD8B24 call 248B Check Input Buffer Status
243D 201A jr nz, 2459

243F 2A1AB8 Id h1, (B81A)

2442 7C ld a,h

2443 B5 or l

2444 37 scf

2445 CC3F25 call z,253F lire header fichier
2448 300F jr nc, 2459

2UUA 2A1AB8 id h1, (B81A)

244D 2B dec hl

2UU4E 221AB8 ld (B814),h1

2451 2A05B8 ld h1,(B805) (Pointer Input Buffer)
2454 E7 rst 4 ld a, (hl)

2455 23 inc hl

2456 2205B8 id (B805),h1 (Pointer Input Buffer)
2459 182C Jr 2487

PUREDEUE DEMEURE DE DEAR D ED DD DE DD DE DEEE C AG OUT CHAR

-11 26-

245B
245C
245D
245E
245F
2462
2464
2467
2469
246€
246F
2471
2472
2475
2476
2478
247B
247C
247F
2482
2483
2484
2487
2488
2489
248A

ES
D5

cs

UF
2147B8
0602
CD8E 24
201E
2A5SFB8
110008
ED52
cs
D41426
c1
300F
2A5FB8
23
225FB8
2AUABS
71

23
224AB8
ci

Di

El

cg

push
push
push
id
ld
ld
call
Jr
ld
ld
sbc
push
call
pop
Jr
id
inc
id
ld
1d
inc
id
pop
pop
pop
ret

CASSETTE MANAGER

h1

de

bc

c,a

h1,B847 Output Buffer Status
b,02

248E Check Buffer Status
nz, 2487

h1, (B85F)

de, 0800

hl1,de

bc

nc, 2614

bc

nc, 2487

h1, (B85F)

hl

(B85F),h1

h1,(B84A) (Pointer Output Buffer)
(h1),c

hl

(B84A),h1 (Pointer Output Buffer)
bc

de

hl

SPEED UE DDR DU DE DDR AURA D DE EE AU DR DE DEEE DEEE CE CK Input Buffer Status

248B

2102B8

ld

h1,B802 Input Buffer Status

PURE DE HE DEEE DE DR BED DE DEN DE EEE DEAD DEN SEE CN QC K Buffer Status

248E
248F
2490
2491
2493
2494
2495

7E
B8
c8
EE01
CO
70
C9

ld
cp
ret
xor
ret
ld
ret

a, (h1)
b
zZ
01
nz
(h1),b

LES LL LT TEST EOF

-11 27-

CASSETTE MANAGER

2435 CAS IN CHAR
nc

DUREE AE OU DE UE AE AE GARD UN AE DE AE GE GE DE GR CAS RETURN

hl

hl, (B81A)

hl

(B81A),h1

h1,(B805) (Pointer Input Buffer)
hl

(B805),h1 (Pointer Input Buffer)
hl

AL TOUS IN DIRECT

2496 CD3524 call
2499 DO ret
249A ES push
249B 2A1AB8 id
24 23 inc
249F 221AB8 ld
2UA2 2A05B8 id
24A5 2B dec
24A6 2205B8 ld
24A9 E1 pop
24AA C9 ret
24AB EB ex
24AC 0603 ld
2UAE CD8B24 call
24B1 CO ret
2uB2 ED531CB8 id
24B6 CDCF24 call
24B9 2A1CB8 id
2uBC ED5B1AB8 id
24C0 19 add
24C1 221CB8 id
24C4 CD3F25 call
24C7 38F0 Jr
24C9 C8 ret
24CA 2AA6B8 ld
24CD 37 scf
2UCE C9 ret
24CF 2A03B8 id
24D2 ED5B1CB8 ld
24D6 ED4AB1AB8 ld
24DA 7B ld
24DB 95 sub
24DC 7A ld
24DD 9C sbc
24DE DAAGBA Jp
24E1 09 add

de,hl

b,03

248B Check Input Buffer Status
nz

(B81C),de

24CF

h1, (B81C)

de, (B81A)

hl,de

(B81C),h1

253F Lire header fichier
c,24B9

Z
h1, (B8A6)

h1,(B803)  (Adr, Start Input Buffer)
de, (B81C)

bc, (B81A)

a,e

1

a,d

a,h

C,BAA6 (0537)KL LDIR CONT’D

hl,bc

-11 28-

24E2
24E3
24E4
24E5
24E6
24E7

C3ACBA

dec
ex
add
dec
ex
Jp

CASSETTE MANAGER

hl

de,hl

hl,bc

hl

de,hl

BAAC (O53D) KL LDDR CONT'D

PUDEUR UE UE DEAD DE DEEE DU DDR DEEE EC AS OUT DIRECT

2UEA
24EB
24EC
24ED
24F0
24F2
24F5
24F6
24F7
2uF8
24F9
2UFC
2500
2504
2507
250B
250E
250F
2510
2511
2514
2517
2518
2514
251B
251E
251F
2520
2523
2524
2525

2526 ”

E5

325EB8
ED5364B8
ED4366B8
2248B8
ED535FB8
21FFF7

push
push
ld

hl

h1,B847 Output Buffer Status

248E Check Buffer Status

nz

(B85E),a

(B864), de

(B866),bc

(B848),h1  (Adr. Start Output Buffer)
(B85F),de

h1,F7FF

hl,de

C
h1,0800
(B85F),h1
de,hl
hl,de

hl
h1,(B848)  (Adr. Start Output Buffer)
hl,de

hl

2614

hl

de

nc

2504

-11 29-

CASSETTE MANAGER

RSS LOU CATALOG

2528 2102B8 ld h1,B802 Input Buffer Status
252B 7E ld a,(hl)

252C B7 or a

252D CO ret nz

252E 3605 ld (h1),05

2530 ED5303B8 ld (B803),de (Adr, Start Input Buffer)
2534 CD8E23 call 238 CAS NOISY

2537 CD4425 call 2544

253A 38FB Jr c,2537

253C C30124 Jp 2401 CAS IN ABANDON

PAR UE DDR DE DD DE DE DEEE DEEE DE DEEE DEN À TE header fichier
253F 3A18B8 ld a, (B818)

2542 B7 or a

2543 CO ret nz

2544 010183 id bc,8301

2547 CD7326 call 2673

254A 305C Jr nc, 25A8

254C 218CB8 id h1,B88C

254F 114000 ld de, 0040

2552 3E2C id a, 2C

2554 CD3628 call 2836 CAS READ

2557 304F Jr nc, 2548

2559 CDC525 call 25C5

255C 2057 Jr nz, 25B5

255E  068B ld b,8B

2560 3802 Jr c,2564

2562 0689 id b,89

2564 CD9226 call 2692

2567 ED5SB9FB8 ld de, (B89F)

256B 2A1CB8 ld h1, (B81C)

256E  3A02B8 ld a, (B802) (Input Buffer Status)
2571 FEO3 cp 03

2573 280€ Jr Z, 2583

2575 21FFF7 ld h1,F7FF

2578 19 add hl,de

2579 3E04 id a,04

257B 382B Jr c,25A8

257D 2A03B8 ld h1,(B803)  (Adr. Start Input Buffer)

-I1 30-

2205B8
3E16
CD3628
301E
2117B8

CA

(B805)

a
(B81E)
h1, (B8
(B81A)
27BF
a,8C
Zz,270C

260D

a
h1,B80
Z, 260B
b, 85
2713
254C
af
b,88
2692
af

nc, 254
b,87
2711
254C
27BF

zZ
a, (B81
a

Z, 25EB
a, (B8A

SSETTE MANAGER

sh1 (Pointer Input Buffer)

CAS READ

sa
9F)
hl

2 Input Buffer Status

C

E)

-I1 31-

25D4
25D5
25D6
25D9
25DA
25DD
25DE
25E1
25E4
25E7
25E9
25EA

25EB
25FE
25EF
25F0
25F1
25F2

25F3
25F6
25F9
25FB
25FC
25FF
2600
2601
2604
2605
2606
2607
2608
260À

260B
260D
260E
260F
2612

2107B8
118CB8
0610

call
ret
ex
ld
Cp
ret

CASSETTE MANAGER

a
nz

a, (B807) (File Header Input)
a

nz, 25F3

nz

h1,B88C

de, B807 File Header Input
bc, 0040

a

25F3
nz
de,hl
a, (de)
(h1)

h1,B807 File Header Input
de, B88C

b,10

a, (de)

27B6

c,a

a, (hl)

27B6

2AUF CAS STOP MOTOR

-11 32-

2613

2614
2617

cg

010284
CD7326
3O4A
068A
114CB8
CD9526
2163B8
CD8826
303A
2A48B8
224AB8
2261B8
ES
214CB8
114000
3E2C
CD3F28
ET
3022
ED5B5FB8
3E16
CD3F28
215DB8
DC8826
3011
210000
225FB8
215CB8

CD1327

inc
xor

scf
jr
or

Jr
ld
call

CASSETTE MANAGER

bc, 8402

2673

nc, 2666

D,8A

de, B84C File Header Output
2695

h1,B863

2688

nc, 2666

h1,(B848)  (Adr. Start Output Buffer)
(B84A),hl (Pointer Output Buffer)
(B861),h1

hl

h1,B84C File Header Output
de,0040

a, 2C

283F CAS WRITE

hl

nc, 2666

de, (B85F)

a,16

283F CAS HRITE

h1,B85D

c, 2688

nc, 2666

h1,0000

(B85F),h1

h1,B85C

(h1)

a

(B863),a

260D

a

h1,B847 Output Buffer Status
Z, 260B

b,86

2713

-11 33-

012001
C3722A

118CB8
3A0OB8

Jr
ld
ld
cp
ld
scf
push
push
call
pop
pop
sbc
ret
ld
Jp

ld
or
scf
ret
ld
JP

ld
1d
or
ret

call
call
ld
or

ld
call
ld
jr
call

Jr

CA

262€
h1,B8C
a,C
(h1)
(h1),0

hi

bc

nz, 276
bc

hl

a, a

nc
(h1),c
2AUB

a,(hl)
a

Z
bc,012
2A72

de, B88
a, (B80
a

nz
(B801)
2783
2726
a, (de)
a
nz,26B
a,8E
2727
bc, 001
26DF
278F
bc, 100
Z, 26C6

SSETTE MANAGER

C

0

0

CAS START MOTOR

C

C
0) (Cass. Message F1ag)

sa

1

0

0

-I1 34-

13
CDBF27
200B
13

1A

CASSETTE MANAGER

Le
hd

a, (hl)
a
Z,26C3
C

hl
26BB
a,b
b,c
c,a
278D
a, (de)
27B6

a

nz, 26D2
a,20
bc

de
1334 TXT WR CHAR
de

bc

de
26C9
275C
de,hl
hl,bc
de,h1
a, 8D
2727
b,02
278D
a, (de)
27A4
275C
de
27BF
nz, 2704
de

a, (de)

-[T 35-

CASSETTE MANAGER

26FB E60F and OF

26FD C624 add  a,24

26FF  CD8027 call 2780 sortir message CAS (1 caract.)
2702 1858 jr 275C

2704 A ld a, (de)

2705 2101B8 ld h1,B801

2708 B6 or (h1)

2709 C8 ret 2Z

270A 186F Jr 277B

270C CD2727 call 2727

270F 186A Jr 277B

2711 3EFF id a, FF

2713 F5 push af

2714 CD1F27 call 271F sortir message CAS (# dans b)
2717 F1 pop af

2718 C660 add  a,60

271A D48027 call nc,2780 sortir message CAS (1 caractère)
271D 185C Jr 277B

LL LE LE LE LEE EL ELLES LES SSL ELLE LE ELLES SELS LS SSL SSI TS SES message CAS (# dans b)
271F CD8011 call 1180 TXT GET CURSOR

2722 25 dec h

2723 C47B27 call nz,277B

2726 78 id a,b

2727 ES push hl

2728 E67F and 7F

2724 47 id b,a

272B 21C527 id h1,27C5 messages cassette

272E 2807 Jr Z,2737

2730 7E id a,(h1)

2731 23 inc  hl

2732 B7 or a

2733 20FB Jr nz, 2730

2735 10F9 dJjnz 2730

2737 JE id a,(h1)

2738 B7 or a

2739 2805 Jr z,2740

273B CD4327 call 2743

273€ 18F7 Jr 2737

2740 E1 pop hl

-11 36-

CASSETTE MANAGER

a, (B800)
a

nz
271F
1A42
c,2769
1279
1B56
1281
1B

Z

2783
a, OA

sortir message CAS (1 caractère)

sortir message CAS (1 Caractère)
(Cass. Message Flag)

sortir message CAS (# dans b)
KM READ CHAR

TXT CUR ON
KM WAIT KEY
TXT CUR OFF

RER RER REC TT message CAS (1 caractère)

2741 23 inc
2742 C9 ret
2743 FA2727 Jp
2746 ES push
2747 0600 ld
2749 04 inc
274A 7E ld
274B 23 inc
274C 07 rlca
274D 3OFA Jr
274F CD8D27 call
2752 E1 porn
2753 JE ld
2754 23 inc
2755 E67F and
2757 CD8027 call
275A 10F7 dJnz
275C 3E20 id
275E 1820 Jr
2760 3A00B8 ld
2763 B7 or
2764 37 scf
2765 CO ret
2766 CD1F27 call
2769 CD421A call
276C 38FB Jr
276E CD7912 call
2771 CD561B call
2774 CD8112 call
2777 FEB cp
2779 C8 ret
277A 37 scf
277B CD8327 call
277E 3EOA ld
2780 C30014 Jp
2783 F5 push

1400

af

TXT OUTPUT

-I1 37-

2784
2785
2787
278A
278B
278C

278D
278E
2791
2792
2795
2796
2797
2798
2799
279A
279B
279C
279D
279F
27A2
27A4
27A6
27A7
27A9
27AB
27AD
27AE
27AF
27B0
27B3
27B4
27B6
27B8
27B9
27BB
27BC
27BE

inc
Sub
Jr
add
push
Id
or
call
pop
jr
cp
ret
cp
ret
add
ret

CASSETTE MANAGER

h1

a,01

115€ TXT SET COLUMN
hl

af

1256 TXT GET WINDOW

1180 TXT GET CURSOR

de

a, FF
(B801),a
277B
b,FF

OA

nc, 27A6
a, 3A

af

a,b

a
nz,27A4
af

2780 sortir message CAS (1 caractère)
61

C

7B

nc

a,E0

-11 38-

CASSETTE MANAGER

27BF 3AO02B8 id a, (B802) (Input Buffer Status)
27C2 FEOS cp 05

27C4 C9 ret
LS US fe ES cassette
27C5. 50 72 65 73 F3 00 50 4C Press, PL

27CD 41 D9 74 68 65 EE 61 6E AYthenan

2705 F9 6B 65 79 BA 00 65 72 ykey:.er

27DD 72 6F F2 00 80 81 00 80 ror.,..,

27E5 52 45 C3 61 6E E4 81 00 RECand..

27ED 52 65 61 E4 82 00 57 72 Read. .Wr

27F5 69 74 ES 82 00 52 65 77 ite. ren

27FD 69 6E E4 74 61 70 E5 00 indtape.

2805 46 6F 75 6E 64 20 AO 00 Found66,

280D 4C 6F 61 64 69 6E E7 00 Loading.

2815 53 61 76 69 6E E7 00 00 Saving. .

281D 4F EB 00 62 6C 6F 63 EB OK.block

2825 00 55 6E 6E 61 6D 65 E4 .Unnamed

282D 66 69 6C 65 20 20 20 AO file

2835 00

NME DE UE UE DDR DEEE DDR NEED DDR DR DEAEDE DEEE DER AS READ

2836 CD73p8 call 2873 allumer moteur ouvr. clavier
2839 F5 push af

283A 21B828 ld h1,28B8

283D 1819 Jr 2858

UM DDE DRE DD DNA DE DEA DD DHEA DEEE MED MEEC AS WRITE

283F CD/328 call 2873 allumer moteur ouvr. clavier
2842 F5 push af

2843 CD6429 call 2964

2846 21F728 ld h1,28F7

2849 DC9D28 call c,289D

284C DC7929 call c,2979

284F 180F jr 2860

PUDEDEDE AD DNE DD DD DD DD DEN DEEE DE DEN DEEE DESERT CAS CHECK

2851 CD/328 call 2873 allumer moteur ouvr, clavier
2854 F5 push af

2855 21C728 ld h1,28C7

-11 39-

CASSETTE MANAGER

2858 ES push hl

2859 CD1929 call 2919

285C E1 pop hl

285D DC9D28 call c,289D

2860 D1 pop de

2861 F5 push af

2862 0182F7 id bc,F782 Port A=Out

2865 ED49 out  (c),c

2867 0110F6 ld bc,F610 allumer moteur

286A ED49 out  (c),c

286C FB ei .

286D 7A ld a,d

286E CD512A call 2451 CAS RESTORE MOTOR
2871 F1 pop af

2872 C9 ret

SD 6 DD DE OEM DA EDEN DEEE DEAR DE SE) ] ] Im moteur ouvr. clavier
2873 32CDB8 id (B8CD),a

2876 1B dec de

2877 1C inc e

2878 ES push hl

2879 DS push de

287A CD681E call 1E68 SOUND RESET

287D D1 pop de

287E DDE1 pop {x

2880 CD4B2A call 2AUB CAS START MOTOR

2883 F3 di

2884 010EF4 id bc,F40E Sound 1/0 Port select
2887 ED49 out (c),c

2889 01D0F6 id bc,F6D0 Strobe mis

288C ED49 out  (c),c

288E 0E10 ld c,10 Strobe coupé

2890 ED49 out  (c),c

2892 0192F7 id bc,F792 Port A=In

2895 ED49 out  (c),c

2897 0158F6 ld bc,F658 ouvrir clavier Y9 (ESC)
289A ED49 out (c),c & sound 1/0 sur port A
289C C9 ret

289) 7A ld a,d

-1I 40-

CASSETTE MANAGER

or a
Jr Z, 28AE
push hl

push de

id e,00

call 28ÂAE

pop de

pop hl

ret nc

dec d

Jr nz, 28A1
ld bc,FFFF
ld (B8D3),bc
ld d,01

jp (h1)

call 29B0

ret nc

ld (1x+00),a
inc {x

dec d

dec e

jr nz, 28B8
Jr 28D9

call 29B0

ret nc

ld b,a

call BADC (056D) RAM LAM (IX)
xor D

ld a,03
ret nz

inc  ix

dec d

dec e

Jr nz, 28C7
dec d

jr z,28E2
call 29B0
ret nc

jr 28D9

-I1 41-

28E2
28E5
28E8
28E9
28EA
28EC
28EF
28F0
28F1
28F2
28F3
28F5
28F6

28F7
28FA
28FD
28FE
2900
2901
2902
2904
2905
2907
2908
290B
290€
290E
2911
2914
2915
2916

2919
291A
291D
291€
291F
2920
2921

CDA629
CDBO29
DO

AA
2007
CDB029
DO

AB

37

c8
3E02
B7

C9

CDDCBA
CDF829
DO
DD23
15

1D
20F3
15
2807
AF
CDF829
DO
18F6
CDA629
CDF829
DO

7B
C3F829

D5
CD2329
D1

D8

B7

C8
18F6

call
call
ret
Xor
Jr
call
ret
xor
scf
ret
ld
or
ret

call
call
ret
inc
dec
dec
Jr
dec
Jr
xor
call
ret
jr
call
call
ret
ld
Jp

push
call
pop
ret
or
ret
jr

CASSETTE MANAGER

29A6
29B0

nc

d

nz, 28F3
29B0

nc

e

a,02

BADC (G56D) RAM LAM (IX)
29F8

nc

1x

d

e

nz, 28F7
d

Z, 290€
a

29F8

nc

2904
2946
29F8

nc

a,e
29F8

de
2923
de

c

a

Z
2919

-IT 42-

21CDB8

CASSETTE MANAGER

1,55
29CD CAS Input RD DATA & Test ESC
nc

de, 0000
h,d
29CD CAS Input RD DATA & Test ESC
nc
de,h]l
b, 00
hl,bc
de,hl

h

nz, 292D
h,c

a,C

d

c,a

a,4

b,a
de,hl
hl,bc
de,hl
29CD CAS Input RD DATA 8 Test ESC
nc

a,d

a

a

a,d

h

c, 2939
C
c,2939
a,d

a,d

h,a
(B8CE),h1
29B0

nc
h1,B8CD

-11 43-

2960
2961
2962
2963

2964

AE
co
37
cg

CD892A
210108
CD7C29

3ACDB8
C3F829

212100
06F4
ED78
E604
C8

xor

ret
push
scf
call
pop
dec

ret

CASSETTE MANAGER

(h1)
nz

2A89
h1,0801
297C

nc

â

2A08

nc

a, (B8CD)
29F8

h1,0021
b,F4

a, (C)
04

nz,297C

h1, (B8D3)
h
p, 29A0

a,h

08
ha
a, 1
10
1.a

-I1 4

CASSETTE MANAGER

299F 37 scf

29A0 ED6A adc hl,hl

29A2 22D3B8 ld (B8D3),h1

29A5 C9 ret

29A6 2AD3B8 ld h1, (B8D3)

29A9 7D ld a, ]

29AA 2F cpl

29AB 5F id e,a

29AC 7C ld a,h

29AD 2F cpl

29AE 57 ld d,a

29AF C9 ret

29B0 D5 push de

29B1 1E08 id e,08

29B3 2ACEB8 ld h1, (B8CE)

29B6 CDD429 call 29D4

29B9 DCDD29 call c,29DD

29BC 300D Jr nc, 29CB

29BE 7C ld a,h

29BF 91 sub c

29C0 9F sbc  a,a

29C1 CB12 rl d

29C3 CD9029 call 2990

29C6 1D dec e

29C7 20EA Jr nz, 29B3

29C9 7A ld a,d

29CA 37 scf

29CB D1 pop de

29CC C9 ret

dl haha dahe dede D dt D ES SES SDS SL A A US Input RD DATA & Test ESC
29CD O6F4 ld b,F4 Port A
29CF ED78 in a, (c) Keyb X
29D1 E604 and O4 ESC ?
29D3 C8 ret z oui
29D4 EDSF ld a,r

296 C603 add  a,03

2928 OF rrca

-IT 45-

29D9
29DA
29DC
29DD
29DF
29E0
292
293
295
297
298
2ŒÆA
29EC
29ED
29EF
29F1
29F2

29F3
29F4
29F6
29F7

29F8
29F9
29FB
29FC
29FE
2A01
2A03
2A04
2A06
2A07

2A08
2A0C
2A0F
2A10
2A11
2A13

OF
E61F
4F
06F5
79
c602
4
380E
ED78
AD
E680
20F3
AF
ED4F
CBOD
37
co

AF
ED4F
3C
C9

D5
1E08
57
CB02
CDO82A
3003
1D
20F6
D1

C9

ED4BDOB8
2AD2B8
SF

67

2807

7D

rrca
and
id
ld
id
add
ld
jr
in
xor
and
jr
xor
id
rrc
scf
ret

xor
ld

inc
ret

push
id
ld
ric
call
Jr
dec
Jr
pop
ret

ld
id
sbc
id
jr
ld

CA

1F

c,a
b,F5
a,C
a,02
ca
C,29F3
a, (C)
l

80

nz, 29)

à

7 ©

r,a

de
e,08
d,a

d

2A08
nc, 2A0
e

nz, 29F
de

bc, (B8
hi, (B8
a,a
h,a
Z, 2A1A
a,

SSETTE MANAGER

Port B

Input RD DATA

F

6

C

DO)
D2)

-II 46-

CASSETTE MANAGER

2A14 87 add a,a

2M5 80 add  a,b

2A16 6F id 1.a

2A17 79 ld a,c

2418 00 sub b

2A19 4F ld c,a

2A1A 7D id a, |

2A1B 32D0B8 ld (B8D0),a

2A1E 2E0A ld 1,04 WR DATA éteint
2A20 CD372A call 2A37 CAS Output WR DATA
2h23 3806 Jr c, 2A2B

2425 91 Sub cc

2426 300C jr nc, 2A34

2h28 2F cpl

2A29 3C inc a

2A2A 4F ld c,a

2A2B 7C ld a,h

2A2C CD9029 call 2990

2A2F 2E0B ld 1,0B WR DATA mis
2A31 CD372A call 2A37 CAS Output WR DATA
2A34 3E01 ld a,01

2436 C9 ret

SERBE DE DE AE DE DE DD HE HE DE DE DD ADDED DEEE DEEE DEEE DD DEA DE DIE EC AS Output WR DATA

2A37 ED5F id ar

2A39 CB3F srl a

243B 91 sub c

2A3C 3003 Jr nc, 2441

2A3E 3C inc a

2A3F  20FD jr nz, 2A3E

2A41 O06F7 ld b,F7 Port Control
2A43 ED69 out  (c),1 WR DATA
2A45 F5 push af

2A46 AF xor a

2A47 ED4F ld r,a

2A49 F1 pop af

2AU4A C9 ret

HD MDEI DD HN DE MED DE DEEE EDEN DD DE DD DE DE DEEE DEEE C AG START MOTOR

2AUB 3E10 ld a,10

-IT 47-

2A4D

CASSETTE MANAGER

2A51 CAS RESTORE MOTOR

LEE) LLLL SELLE)... LS, OUNS STOP MOTOR

2A4F

a,EF

SSL LL LL LL LL LES LL LL LS SL LL SL SSL ALLONS RESTORE MOTOR

2A51
2452
2A54

2A72
2473
2A74
2A77
2479
2A7C
2A7D
2A7E
2A80
2A81
2A82
2A83

1802 ir
3EEF ld
C5 push
06F6 ld
ED48 in
04 inc
E610 and
3E08 ld
2801 Jr
3C inc
ED79 out
37 scf
280C Jr
79 id
E610 and
C5 push
01C800 id
37 scf
CC722A call
C1 pop
79 id
C1 pop
C9 ret
C5 push
ES push
CD892A call
3E42 id
CDBD1C call
E1 pop
C1 pop
2007 Jr
0B dec
78 id
B1 or
20ED jr

bc

b,F6 Port C

c,(c)

)

10

a,08

Z, 2A5E

a

(c),a Moteur allumé/éteint

Z, 2A6F

bc,00C8

Z,2A72
bc
a,c

bc

bc

hl

2A89
a,42
1CBD KM TEST KEY
hl

bc

nz, 2A87
bc

a,b

€

nz, 2A72

-11 48-

CASSETTE MANAGER

2A85 37 scf

2A86 C9 ret

2487 AF xor a
2488 C9 ret

2A89 018206 ld bc,0682
2A8C OB dec bc
2A8D 78 id a,b
2A8E BI or C

2A8F 20FB jr nz, 2A8C
2A91 C9 ret

2A92 C7 rst 0
2A93 C7 rst O0

2A94 C7 rst O0
2A95 C7 rst 0

2A96 C7 rst O0

2A97 C7 rst 0

-11 49-

SCREEN EDITOR

2.5.10 SCREEN EDITOR (EDIT)

L'éditeur n'est pas en réalité un pack dans le sens où nous l'avons
compris jusqu'ici. Il n'est en effet pas du tout utilisé par le système
d'exploitation.

11 doit plutôt être considéré comme 1ié aux packs arithmétiques. De même
que ceux-ci, l'éditeur n'est appelé que par le Basic.

Nous ne voyons pas quelles routines individuelles pourraient être
utilisées, si ce n'est tout au plus l'éditeur lui-même globalement.

I1 vous faut pour cela fournir à hl l'adresse de début du texte que vous
souhaitez éditer. Ce texte doit comprendre un maximum de 255 caractères,
ce qui correspond également à la taille maximum d'une ligne Basic.

-11 50-

SCREEN EDITOR

LLLEL LL RL LLLL LE DLL LL LED LED SEL SL LLLL SELS LELLLE, EEE: EDIT

2A98
2A99
2A9A
2A9B
2A9C
2A9F
2AAO
2AA1
2AA2
2AA3
2AA5
2AA8
2AAB
2AAC
2AAF
2AB0
2AB1
2AB4
2AB5
2AB6
2AB9
2ABB
2ABC
2ABF
2AC0
2AC1
2AC2
2AC3
2AC5

C5
D5
ES

ES

01FF00
oc

7E

23

B7
20FA
32DDB8
CD6F2C
E1
CD672D
C5

E5
CDD92D
E1

C1
CDC62A
30F4
F5
CDD22C
F1

ET

Di

C1
FEFC
C9

push
push
push
push
ld
inc
ld
inc
or
jr
ld
call
pop
call
push
push
cal]
pop
pop
call
jr
push
call
pop
pop
pop
pop
Cp
ret

bc
de
hl
hl

pointeur sur buffer d'entrée

bc,O0FF

(a
a,(h1l)
hl

a

compteur caractères dans buffer

nz, 2A9F

(B8DD)
2C6F
hl
2D67
bc

hl
2DD9
hl

bc
2AC6

,a (Insert Flag)

caractère du clavier

exécuter saut EDIT

nc, 2AAF

af
2CD2
af
hl
de
bc
FC

DE DEN DD DE DE DEEE DEN DE DD DEEE DE DE DÉDÉ HE DEDE DE NEDÉDEDÉDÉE EDEDÉDE EHE NE exécuter saut EDIT

2AC6
2AC7
2ZACA
2ACB
2ACC
2ACD
2ACE
2ADO

ES
21E02A
5F

78

B1

7B
200B
FEFO

push
Id
1d
ld
or
id
jr
cp

hl

hl1,2AE0 EDIT Table de saut 1

e,a
a,b
c

a,e

caractère dañs buffer?

nz, 2ADB oui =>

FO

une des touches curseur?

-I1 51-

2AD2
2AD4
2AD6
2AD8
2ADB
2ADE
2ADF

3807
FEF4
3003
211C2B
CDF62D
E3

cg

Jr
CP
jr
id
call
ex
ret

SCREEN EDITOR

c,2ADB
F4

nc, 2ADB
h1,2B1C
2DF6
(sp), h1

non =>

touche curseur & SHFT/CTRL ?

oui =>

EDIT Table de saut
aller chercher adr
manipuler adr, ret
Saut d’après table

2
. de saut EDIT

LAS SL LS LL LL LL LL Lis LL LS LS LL LL LL LS LE LL SL) EDIT Table de saut 1

2AE0
2AE1
2AE3
2AE4
2AE6
2AE7
2AE9
2AEA
2AEC
2AED
2AEF
2AF0
2AF2
2AF3
2AF5
2AF6
2AF8
2AF9
2AFB
2AFC
2AFE
2AFF
2B0t
2B02
2B04
2B05
2B07
2B08
2B0A
2B0B
2BOD

13
012€

db

13
2001

Nombre d'entrées
insérer caractère

ESC

aucun effet

ENTER

CRSR UP (Buffer)
CRSR DWN (Buffer)
CRSR LEFT (Buffer)
CRSR RGHT (Buffer)
CTRL & CRSR UP
CTRL & CRSR DHN
CTRL & CRSR LEFT
CTRL & CRSR RGHT
SHFT & CRSR UP
SHFT & CRSR DHN

SHFT & CRSR LEFT

-11 52-

SCREEN EDITOR

2B0E  982C dw 2098 SHFT & CRSR RGHT
2B10 EO db EO

2B11 EA2C dw 2CEA COPY

2B13 7F db 7F

2B14 3D2C dw 2C3D DEL

2B16 10 db 10

2B17 4A2C dw 2C4A CLR

2B19 E1 db E1

2B1A F92B dw 2BF9 CTRL & TAB (Flip Insert)
LILLLLLLLLLLLLLLLLLLLLLLLLL LE LLLLLLLLLLLLLLELEZZL EL] EDIT Table de saut 2
2B1C Où db 04 Nombre d’entrées
2B1D 2B2B 0" 2B2B KLINGEL

2B1F FO db FO

2B20 2F2B dw 2B2F CRSR UP

2B22 F1 db F1

2B23 332B dw 2B33 CRSR DHN

2B25 F2 db F2

2B26 3B2B dW 2B3B CRSR LEFT

2B28 F3 db F3

2B29 372B dw 2B37 CRSR RGHT

LALLLLLLL LL LIL LL LS LLLELLLL LL ALL LL LLLLLLLLLLELE ES] BIP

2B2B 3E07 ld a,07 BEL

2B2D 180E Jr 2B3D

LILAS LLLLLLLLLLLLILLLLLLLLLLLLLLLLLILLLLLLL LI LL] CRSR UP
2B2F 3EOB ld a,0B

2B31 1804 jr 2B3D

LELLL LL LL LL LL LL I LL LL LLLLLLLL LL LLLLLLLLLLLELLL LL] CRSR DWN
2B33 3EOA ld a, OA

2B35 1806 Jr 2B3D

LILLLLSL LS LILI LL ES LL LILI LILI SL LLLIL LS LIL LLLLLLLLZLEL ZX EX 2 CRSR RGHT
2B37 3E09 id a,09

2B39 1802 Jr 2B3D

LELL LL LS LE LR LL SL LES LL LL LL LL LL LL Li LL LLLL LL, ZE 2 EL) CRSR LEFT

2B3B 3E08 id a,08

-11 53-

SCREEN EDITOR

2B3D CDO014 call 1400 TXT OUTPUT
2B40 B7 or a
2B41 C9 ret

LIL Si Li Li Lil lll::):111):1:1;::1:1;:;:;:;::;;::)::;:) ESC

2B42 F5 push af

2B43 CD492B call 2B49

2B46 F1 pop af

2B47 37 scf

2B48 C9 ret

2B49 CD692B call 2B69 ENTER

2B4C 21612B ld h1,2B61 message *BREAK*
2B4F  CD692B call 2B69 ENTER

2B52 CD8011 call 1180 TXT GET CURSOR
2B55 25 dec h

2B56 C8 ret 7Z

2B57 3EOD ld a,0D CR

2B59 CDO014 call 1400 TXT OUTPUT
2B5C 3EO0A Id a,0A LF

2B5E C30014 Jp 1400 TXT OUTPUT

LELLLLLE LL LL SE LL LS ISLE LI LLE LL EL LL LILI LL LL LT LL LIL LE] message *BREAK*

2B61 2A 42 72 65 61 6B 2A 00 *Break*.

LELLLL LL ELLES LL LL Li Lr EL LLL: Li LLLLLLLL LL LL, , LL] ENTER

2B69 F5 push af

2B6A 7/E ld a,(h1)
2B6B 23 inc hl

2B6C B7 or a

2B6D C4A82D call nz,2DA8
2B70 20F8 Jr nz, 2B6A
2B72 F1 pop af

2B73 37 scf

2B74 C9 ret

HEMENE NE DE DEN DEHHENE DEN DD DH MH DE DE DÉMO DE DE DEEE EE CRSR RGHT (Buffer)

2B75 1601 ld d,01

-11 54-

SCREEN EDITOR

2B77 CD932B call 2B93
2B7A CA2B2B Jp Z, 2B2B BIP
2B/D C9 ret

LÉLL LL SELLES LIL LL SL LL LL LL LL LIL LL LEE LL Lili: CRSR DWN (Buffer)

2B7E CDEB2B call 2BEB

2B81 79 ld a,c

2B82 90 sub b

2B83 BA cp d

2B84 DA2B2B Jp c,2B2B BIP
2B87 180A Jr 2B93
LALLLLILLLLILLLLLLLLLLLLLLLLLLILLLLILLLLLLLLLLLLLL] CTRL & CRSR RGHT
2B89 CDEB2B call 2BEB

2B8C 7A ld a,d

2B8D 93 sub e

2B8E C8 ret 7Zz

2B8F 57 ld d,a

2B90 1801 Jr 2B93
LILELELLLLLLELLLLIL LL LLLLLILILLLLLLLLILLLLLLLLZL ZX) CTRL & CRSR DWN
2B92 51 ld d,c

2B93 78 ld a,b

2B94 B9 cp c

2B95 C8 ret 7Zz

2B96 D5 push de

2B97 CD502D call 2D50

2B9A 7E ld a, (h1l)

2B9B DUA82D call nc,2DA8

2BSE O4 inc b

2B9F 23 inc hl

2BAO D4672D call nc,2D67

2BA3 D] pop de

2BA4 15 dec d

2BA5 20EC jr nz, 2B93

2BA7 F6FF or FF

2BA9 C9 ret

LR SSL LS SELS SELS LS SL LIL LL EL LL LL LS LL LL LL LLLLEL EX EL: CRSR LEFT (Buffer)

2BAA 1601 id d,01

-IT 55-

SCREEN EDITOR

2BAC CDC82B call 2BC8

2BAF CA2B2B Jp Z, 2B2B BIP
2BB2 C9 ret
LLLLLILIILLIILLILLILLLLLLLILILLLILLLIILLLLLILLLIILLLL) CRSR UP (Buffer)
2BB3 CDEB2B call 2BEB

2BB6 78 id a,b

2BB7 BA cp d

2BB8 DA2B2B Jp c,2B2B BIP
2BBB 180B jr 2BC8
LLLLLLLLLLLLELLELLLLLLLLLILLILLLLLILLLLLLLLLLLLLLLL)] CTRL & CRSR LEFT
2BBD CDEB2B call 2BEB

2BCO 7B ld a,e

2BC1 D601 sub 01

2BC3 C8 ret 2z

2BC4 57 ld d,a

2BC5 1801 jr 2BC8
CILLILLILLLILLLLLILLLLELLLILLLLLLLILILLLLLLLLELEL EE) CTRL & CRSR UP
2BC7 51 ld d,c

2BC8 78 id a,b

2BC9 B7 or a

2BCA C8 ret z

2BCB CD4A2D call 2D4A

2BCE 3007 Jr nc, 2BD7

2BD0O 05 dec b

2BD1 2B dec hl

2BD2 15 dec d

2BD3 20F3 Jr nz, 2BC8

2BD5 1811 Jr 2BE8

2BD7 78 ld a,b

2BD8 B7 or a

2BD9 2804 Jr 2, 2BE5

2BDB 05 dec b

2BDC 2B dec hl

2BDD D5 push de

2BDE CD292D call 2D29

2BE1 Di pop de

2BE2 15 dec d

IT 56-

2BE3
2BE5
2BE8
2BEA

2BEB
2BEC
2BEF
2BF0
2BF1
2BF2
2BF3
2BF6
2BF7
2BF8

SCREEN EDITOR

nz, 2BD7
2D67
FF

hl

1256 TXT GET WINDOW
a,d

h

a

d,a

1180 TXT GET CURSOR
e,h

hi

CELL LLLLELI LIL LL LLILLILLLLELLLLLLLLLLLL LL ELLE ES EX EX 2: CTRL & TAB (Flip insert)

2BF9
2BFC
2BFD
2C00

3ADDB8
2F
32DDB8
cg

ld
cpl
ld
ret

a, (B8DD) (Insert Flag)

(B8DD),a (Insert Flag)

CILL LL LILI LL LL LILI LL LL LL LS LE LL LL LS Lili): L;.; insérer caractère

2001
2C02
2C03
2C04
2C07
2C08
2COA
2C0B
2C0C
2C0E
2C0F
2C10
2013
2014
2015
2016

or
ret
ld
ld
or
Jr
id
cp
Ir
ld
ld
call
inc
inc
or
ret

a

Z

e,a

a, (B8DD) (Insert Flag)
a
z,:2C17
a,b

C
z,2C17
(hl),e
a,e
2DA8
hl

b

a

-11 57-

SCREEN EDITOR

2C17 79 ld a,c

2C18 FEFF cp FF

2C1A CA2B2B Jp Z, 2B2B BIP
2C1D AF xor a

2C1E 32DCB8 ld (B8DC),a

2C21 7B ld a,e

2C22 CDA82D call 2DA8

2025 OC inc c

2026 E5 push hl

2C27 7E ld a, (hl)

2028 73 ld (hl),e

2C29 5F ld e,a

2C2A 23 inc hl

2C2B B7 or a

2C2C 20F9 Jr nz, 2C27

2C2E 77 ld (hl),a

2C2F El pop  hl

2C30 Où inc D

2C31 23 inc hl

2C32 CD672D call 2D67

2C35 3ADCB8 ld a, (B8)C)

2C38 B7 or a

2C39 C4292D call nz,2D29

2C3C C9 ret
LLLLLLLLLILLLLLLLLLLLLLILLLLLLISLLLLSLSLLLLLLLS. DEL
2C3D 78 ld a,b

2C3E B7 or a

2C3F CA2B2B Jp Z, 2B2B BIP
2C42 CDAA2D call 2D4A

2C45 D22B2B jp nc,2B2B BIP
248 05 dec b

2C49 2B dec hl

LELLLLLL LL ETS LILI SSL SSL SLLLLSLLLSLLLLSLSSSSLLSL SSL SS CLR
2CUA 78 id a,b

2C4B B9 cp C

2C4C CA2B2B Jp Z, 2B2B BIP
2C4F ES push hl

2C50 23 inc  hl

-11 58-

2051
2C52
2053
2054
2055
2C56
2C58
2C59
2C5B
2C5E
2C5F
2C62
2C63
2C65
2C66
2C67
2C6A
2C6B
2C6E

2C6F
2072
2075

2C76
2C7A
2C7B
2C7C
2C7D
2C7E
2C7F
2C80
2C81

2C82
2C83
2C86
2C87
2C88
2C89

210000
22DEB8
C9

EDSBDEB8

ld
dec
ld
inc
or
Jr
dec

ld
ex
call
ex
ld
pop
dec
ld
or
call
ret

1d
ld
ret

ld
id
xor
ret
Id
xor
ret
scf
ret

ld

id
or
ret
Id

SCREEN EDITOR

a, (h1)
h1
(hl),a
hl

a

nz, 2C50
hl
(h1),20
(B8DC),a
(sp),h1
2D67
(sp),h1
(h1),00
hl

€

a, (B8DC)

a
nz, 2D2D

h1,0000
(B8DE),h1

de, (B8DE)
a,h

d

nz

a,1

e

nz

c,a
h1, (B8DE)
a,h

il

Z

a, 1

-11 59-

SCREEN EDITOR

SHFT & CRSR RGHT

SHFT & CRSR LEFT

SHFT & CRSR UP

SHFT & CRSR DHN

2C8A 81 add  a,c

2C8B 6F ld 1.a

2C8C CDCE11 call 11CE TXT VALIDATE
2C8F 3803 Jr c,2C94

2091 210000 ld h1,0000

2C94 22DEB8 id (B8DE),h1

2097 C9 ret
LILLLLLLLIILLLILLILLLLILLLLLELLLELLILILILLILILLLIII III]
2C98 110001 ld de,0100

2C9B 180D jr 2CAA
LELLLLLILILLELLILLLLLILIIIILILILLIELLIILIILII. LILI.)
2C9D 1100FF ld de,FF00

2CA0 1808 jr 2CAA

LES LL I LIL I LILI LILILILIILILILILLILILIIIIIILLLI III]
2CA2 11FF00 ld de, O00FF

2CA5 1803 jr 2CAA

LELELLLLLE LI LED LIL ELIILIIILILILILLIILILLILIII11)
2CA7 110100 id de,0001

2CAA C5 push bc

2CAB E5 push hl

2CAC 2ADEB8 id h1, (B8DE)

2CAF 7C id a,h

2CB0 B5 or 1

2CB1 CC8011 call z,1180 TXT GET CURSOR
2CB4 7C ld a,h

2CB5 82 add  a,d

2CB6 67 ld h,a

2CB7 7D ld a, 1

2CB8 83 add  8,e

2CB9 6F ld La

2CBA CDCE11 call 11CE TXT VALIDATE
2CBD 300B jr nc, 2CCA

2CBF ES push hl

2CC0O CDD22C call 2CD2

2CC3 E1 pop hl

2CC4 22DEB8 ld (B8DE),h1

-1] 60-

SCREEN EDITOR

2CC7 CDCD2C call 2CCD

2CCA Ei pop hl

2CCB C1 pop bc

2CCC C9 ret

2CCD 116812 id de,1268 TXT PLACE/REMOVE CURSOR
2CD0 1803 jr 2CD5

2CD2 116812 ld de,1268 TXT PLACE/REMOVE CURSOR
2CD5 2ADEB8 id h1, (B8DE)

2CD8 7C ld a,h

2CD9 B5 or 1

2CDA C8 ret 2z

2CDB ES push hl

2CDC CD8011 call 1180 TXT GET CURSOR
2CDF E3 ex (sp),hl

2CE0 CD7411 call 1174 TXT SET CURSOR
2CE3 CD1600 call 0016

2CE6 E1 pop hl

2CE7 C37411 Jp 1174 TXT SET CURSOR
LILLLLLLILELLILLLLLILILILLILLLELLLILLLLLLLLLLLLLL)] COPY
2CEA C5 push bc

2CEB E5 push hl

2CEC CD8011 call 1180 TXT GET CURSOR
2CEF EB ex de,hl

2CFO 2ADEB8 id h1, (B8DE)

2CF3 7C id a,h

2CF4 B5 or 1

2CF5 200€ jr nz,2D03

2CF7 78 ld a,b

2CF8 B1 or C

2CF9 2026 Jr nz, 2D21

2CFB CD8011 call 1180 TXT GET CURSOR
2CFE  22DEB8 ld (B8DE),h1

2D01 1806 Jr 2D09

2D03 CD7411 call 1174 TXT SET CURSOR
2D06 CD6812 call 1268 TXT PLACE/REMOVE CURSOR
2D09 CDAB13 call 13AB TXT RD CHAR

2D0C F5 push af

2D0D EB ex de,hl

-11 61-

SCREEN EDITOR

2D0E CD7411 call 1174 TXT SET CURSOR
2D11 2ADEB8 ld h1, (B8DE )
2D14 24 inc h

2D15 CDCE11 call 11CE TXT VALIDATE
2D18 3003 jr nc, 2D1D

2D1A 22DEB8 ld (B8DE),h1
2D1D CDCD2C call 2CCD

2D20 F1 pop af

2D21 Ei pop hl

2D22 C1 pop bc

2D23 DAO12C jp c, 2001 insérer caractère
2D26 C32B2B jp 2B2B BIP
2D29 1601 ld d,01

2D2B 1802 jr 2D2F

2D2D 16FF ld d,FF

2D2F C5 push bc

2D30 ES push hl

2D31 D5 push de

2D32 CDD22C call 2CD2

2D35 D1 pop de

2D36 2ADEB8 1d h1, (B8DE)
2D39 7C ld a,h

2D3A B5 or l

2D3B 2809 Jr Z, 2D46

2D3D 7C ld a,h

2D3E 82 add a,d

2D3F 67 ld h,a

2D40 CD8C2C call 2C8C

2D43 CDCD2C call 2CCD

2D46 E1 pop hl

2D47 C1 pop bc

2D48 B7 or a

2D49 C9 ret

2D4A DS push de

2D4B 1108FF ld de, FF08

2D4E 1804 Jr 2D54

2D50 DS push de

2D51 110901 ld de, 0109

2D54 C5 push bc

-11 62-

2D55
2D56
2D59
2D5A
2D5B
2D5C
2D5F
2D60
2D63
2D64
2D65
2D66

2D67
2D68
2D69
2D6A
2D6D
2D6E
2D6F
2D70
2D71
2D72
2D75
2D77
2D7A
2D7B
2D7C
2D7D
2D7E
2D81
2D82
2D83
2D84

2D85
2D86
2D87
2D88
2D89

ES
CD8011
7A
84
67
CDCE11
7B
DCO014
ET
C1
D1
C9

C5

ES

EB
CD8011
UF

EB

7E

23

B7
Cu852D
20F8
CD8011
91

EB

85

6F
CD7411
E1

C1

B7

C9

F5
C5
D5
E5
47

push
call
ld
add
ld
call
ld
call
pop
pop
pop
ret

push
push
ex
call
ld
ex
ld
inc
or
call
Jr
call
Sub
ex
add
ld
call
pop
pop
or
ret

push
push
push
push
ld

SCREEN EDITOR

hl

1180 TXT GET CURSOR
a,d

a,h

ha

11CE TXT VALIDATE
a,e

c,1400 TXT OUTPUT
hl

bc

de

bc

hl

de,h]l

1180 TXT GET CURSOR
c,a

de,hl

a,(hl)

hl

a

nz, 2D85

nz, 2D6F

1180 TXT GET CURSOR
C

de,hl

a, 1

La

1174 TXT SET CURSOR
hl

bc

a

af
bc
de
hl
b,a

-11 63-

CD8011

CD8011

pop
ret

push
push
push
push
ld

call

push
cal]
pop
call
push
call
id
push
call
pop
call

SCREEN EDITOR

1180 TXT GET CURSOR
c

a,e

e,a

c,b

11CE TXT VALIDATE
c;2D9B

a,b

a,a

a

a,e

e,a

de,hl

11CE TXT VALIDATE
a,c

c,2DA8

de
bc
af

1180 TXT GET CURSOR

11CE TXT VALIDATE

1334 TXT WR CHAR

1180 TXT GET CURSOR

-11 64-

SCREEN EDITOR

2DC6 91 sub c

2DC7 Cu822C call nz,2C82

2DCA F1 pop af

2DCB 3007 Jr nc, 2DD4

2DCD 9F sbc  a,a

2DCE 32DCB8 id (B8DC),a

2DD1 CDCD2C call 2CCD

2DD4 E1 pop hl

2DD5 D1 pop de

2DD6 C1 pop bc

2DD7 F1 pop af

2DD8 C9 ret
CELLLILILLLILILLLILILLLLLLLLLLILLLLILLILLLLILLLLLLELL] caractère du clavier
2DD9 CD8011 call 1180 TXT GET CURSOR
2DDC 4F ld c,a

2DDD CDCE11 call 11CE TXT VALIDATE
2DE0 CD762C call 2C76

2DE3 DA3CIA Jp c, 1A3C KM WAIT CHAR
2DE6 CD7912 call 1279 TXT CUR ON
2DE9 CD8011 call 1180 TXT GET CURSOR
2DEC 91 sub c

2DED C4822C call nz,2C82

2DF0O CD3CIA call 1A3C KM WAIT CHAR
2DF3 C38112 Jp 1281 TXT CUR OFF

LES LL LL LL EL LLL LL) iii): llLLllilLL::] aller chercher adr. saut EDIT
2DF6 F5 push af

2DF7 C5 push bc
2DF8 46 ld b,(h1)
2DF9 23 inc hi
2DFA ES push hl
2DFB 23 inc hi
2DFC 23 inc  hl
2DFD BE cp (h1)
2DFE 23 inc hl
2DFF 2804 Jr Z,2E05
2E01 05 dec b
2E02 20F7 Jr nz, 2DFB
2E04 E3 ex (sp),h1

-11 65-

SCREEN EDITOR

2E05 F1 pop af
2E06 7E ld a,(hl)
2E07 23 inc hl
2E08 66 ld h, (hl)
2E09 6F id 1,a
2E0A C1 pop bc
2E0B F1 pop af
2E0C C9 ret

2E0D C7 rst O0
2E0E C7 rst O0

2E0F C7 rst O0
2E10 C7 rst O0

2E11 C7 rst O0
2E12 C7 rst 0
2E13 C7 rst O0
2E14 C7 rst O0
2E15 C7 rst O0

2E16 C7 rst O0

2E17 C7 rst O0

-I1 66-

CHARACTERS

2.6 Le générateur de caractères

Ce n'est pas que nous voulions à tout prix abuser de votre patience avec
les pages suivantes ni que nous pensions que l'ouvrage ne comporte pas
encore assez de pages.

Nous pensons simplement que le Jeu de caractères est un outil de travail
important auquel s'appliquent même spécialement certaines instructions du
jeu d'instructions Basic.

Pour que vous n'ayez pas à réinventer la poudre chaque fois que vous
utilisez ces instructions, par exemple lorsque vous voulez produire des
accents, il vous suffit de rechercher la forme du ‘e’ et de rajouter au
dessus les points qui formeront l'accent aigu ou grave, Il vous suffit
alors d'utiliser les valeurs ainsi calculées dans votre instruction de
définition d’un caractère.

Nous nous permettons de vous donner un petit conseil, Vous constaterez
que la plupart des dessins figurant dans les pages suivantes marquent
toujours les lignes verticales par une paire de points (deux points sur
la même ligne horizontale), Il vaut mieux éviter en effet de constituer
des lignes verticales n'ayant qu'un point de largeur. En effet un point
isolé est difficile à discerner à l'écran, surtout si vous disposez d'un
moniteur couleur.

Mais maintenant vous pouvez donner libre cours à votre imagination et
faire vos propres expériences.

-11 67-

3800
3801
3802
3803
3804
3805
3806
3807

3810
3811
3812
3813
3814
3815
3816
3817

3820
3821
3822
3823
3824
3825
3826
3827

3830
3831
3832
3833
3834
3835
3836
3837

3840
3841
3842
3843
3844
3845
3846
3847

3850
3851
3852
3853
3854
3855
3856
3857

CHARACTERS

SRRRERE
Lisess |}

au \\\ ee
EN \\\ EE
Lssss
Lesns
Less, |
SESEEEEE

E

|.

3808
3809
380A
380B
380C
380D
380E
380F

3818
3819
381A
381B
381C
381D
381E
381F

3828
3829
382A
382B
382C
382D
382E
382F

3838
3839
383A
383B
383C
383D
383E
383F

3848
3849
384A
384B
384C
384D
384E
384F

3858
3859
385A
385B
385C
385D
385E
385F

-11 68-

Les, |)
Less, |

EE \ euE
SesRRpes

USB.

3860
3861
3862
3863

3865
3866
3867

3870
3871
3872
3873
3874
3875
3876
3877

3880
3881
3882
3883
3884
3885
3886
3887

3890
3891
3892
3893
3894
3895
3896
3897

38A0
38A1
38A2
38A3
38A4
38A5
38A6
38A7

38B0
38B1
38B2
38B3
38B4
38B5
38B6
38B7

CHARACTERS

COBEEE

UUeUUe. ii)
LiUTEL
Lcssss: |
CEE CC
CEBEEEE )
CEE CEE )
ses | #
CEE CEE |
(BEC EE |

EE EE
CEE EE
CUS C0)

-11 69-

CHARACTERS

LUesss

38C8
38C9
38CA
38CB
38CC
38CD
38CE
38CF

38D8
38D9
38DA
38DB
38DC
38DD
38DE
38DF

38E8
38E9
S8EA
38EB
38EC
38ED
38EE
38EF

38F8
38F9
38FA
38FB
38FC
38FD
S8FE
38FF

3908
3909
390A
390B
390C
390D
390E
390F

3918
3919
391A
391B
391C
391D
391E
391F

-[1 70-

3920
3921
3922
3923
3924
3925
3926
3927

3930
3931
3932
3933
3934
3935
3936
3937

3940
3941
3942
3943
3944
3945
3946
3947

3950
3951
3952
3953
3954
3955
3956
3957

3960
3961
3962
3963
3964
3965
3966
3967

3970
3971
3972
3973
3974
3975
3976
3977

CHARACTERS

DUUUUUU

-I1 71-

3928
3929
392A
392B
392C
392D
392E
392F

3938
3939
393A
393B
393C
393D
393E
393F

3948
3949
394A
394B
394C
394D
394E
394F

3958
3959
395A
395B
395C
395D
395E
395F

3968
3969
396A
396B
396C
396D
396E
396F

3978
3979
397A
397B
397C
397D
397E
397F

CR
[L ses: Le
Se. es

EXC BE

(EEE |)

CHARACTERS

3980 7C
3981 C6
3982 CE
3983 D6
3984 E6
3985 C6
3986 7C
3987 00
3990 3C
3991 66
3992 06
3993 3C
3994 60
3995 66
3996 7E
3997 00
39A0 1C
39A1 3C
39A2 6C
39A3 cc
39A4 FE
39A5 oc
39A6 1E
39A7 00
39B0 3C
39B1 66
39B2 60
39B3 7C
39B4 66
3985 66
3986 3C
39B7 00
39C0 3C
39C1 66
39C2 66
39C3 3C
39C4 66
39C5 66
39C6 3C
39C7 00
39D0 00
39D1 00
39D2 18
39D3 18
39D4 00
39D5 18
39D6 18
3907 00

39E0
39E1
39E2
39E3
39E4
39E5
39E6
39E7

39F0
39F1
39F2
39F3
39F4
39F5
39F6
39F7

3A00
3A01
SA02
3A03
SA04
3A05

3A07

3A10
3A11
3A12
3A13
3A14
3A15
3A16
3A17

3A20
3A21
SA22
SA23
SA24
SA25
3A26
3A27

3A30
3A31
SA32
SA33
3A34
SA35
3A36
SA37

CHARACTERS

39E8
39E9
3S9EA
39EB
39EC
39ED
39EE
39EF

39F8
39F9
39FA
39FB
39FC
39FD
39FE
39FF

3A08
3A09
SA0A
3SA0B
SAOC
3A0D
SA0E
3A0F

3A18
3A19
SA1A
3A1B
SA1C
3A1D
SAÎE
3SA1F

3A28
SA29
SA2A
3A2B
3A2C
3A2D
3SA2E
3A2F

3A38
3A39
3SASA
3A3B
SA3C
SA3D
SASE
SASF

00000000

CESSE |

DESSESEN
EEE
SRE
CREER

3A40
3A41
3A42
3SA43
3A44
3A45
3SA46
3A47

SA50
3A51
SA52
3A53
SA54
3A55
3A56
3A57

3A60
3A61
SA62
3SA63
3A64
SA65
3A66
3SA67

SA70
SA71
SA72
3A73
SA74
SA75
SA76
SA77

3A80
3A81
3SA82
3SA83

3A85
3A86
3SA87

3A90
3A91
3A92
3A93
3A94
3A95
3A96
3A97

sssveseu
Css Ts
[Tess  e
[Less | e
aassesEs
esssss |
a Les
RRQ SA
n] 3A8D
LLLLesss
ssssases
LLUUULUSS
BL es | e
s_ es 0
a Lt se
se es
ses 0e
[DLse_ Le

CHARACTERS

3A48
3A49
SA4A
3A4B
SA4C
3A4D
SA4E
SA4F

3SA58
3A59
SASA
3A5B
SA5C
3A5D
SASE
3A5F

3A68
3A69
3SA6A
3A6B
3A6C
3A6D
SA6E
SA6F

SA78
3A79
SA7A
3A7B
SA7C
SA7D
3SA7E
SA7F

3A88
3SAB9
SA8A
3A8B

SA8E
SA8F

3A98
SA99
SA9A
3A9B
SA9C
SA9D
3SA9E

SES SA9F

(

-I1 74-

CEE EE )
LUUes je
sense

CHARACTERS

3AAO 7E  (CEBSEES SAAB 6 CEE EE)
3AA 5A (EC EE 3AA9 66 (EE ( EN)
3AA2 18 (T[BE LL) 3AAA 66 (EE \ EE
3AA3 18 SAAB 66 (EE | EN)
3AA4 18 3SAAC 66 (EE EN)
3AA5 18 SAAD 66 (CEE { EN
3AA6 3C 3AAEË 3C CUSEEE |
3AA7 0 CEUX) SAAF 0 (XXII

3AB0 66 CE EE | 3AB8 C6 ŒEX EE )
3AB1 66  (CJEN (EE SAB9 C6 EE {EE
SAB2 66 CEE EE SABA C6 EI) EM.
3AB3 66 (EN { En, 3ABB D6  ŒECE EN |
3AB4 66 3ABC FE  EESSESER.
3AB5  3C 3ABD EE SES SE.
3AB6 18

SAB7 0 CEUX OC SABF 00 CXXXIEELL

3AC0 C6 BC COCEEC 3AC8 66 CEE X EE |;
SEC EE

3AC1  6C 3ACO 66 CEE | EN |
3AC2 38 3ACA 66 (EE I EM)
3AC3 38 3ACB  3C
3AC4  6C 3ACC 18
SACS C6 3ACD 18
3AC6 C6 SACE  3C
3AC7 00 3ACF 00

3AD0 FE  ŒESSEEN , 3ADB  3C (CEMEE ;)
3AD1 C6  Œ | { (EE 3AD9 30

3AD2 8c SADA 30
3AD3 18 3ADB 30
3AD4 32 3SADC 30 (\ EE TXT 1)
3AD5 66 3ADD 30 (7) EE À À À)

3AD6 FE BEST 3ADE 3C
3AD7 00 (LIXLIIIL) 3ADF 00
3AE0 C0 ŒIL) 3AE8 3C (CX EEE |)

3AE1 60 3AE9 oc
3AE2 30 SAEA oC
3AE3 18 3AEB oc
3AE4 oC 3AEC oc
3AE5 06 SAED oc
3AE6 02 3AEËE 3C
3AE7 00 3AEF 00
3AF0 18 3AF8 00
3AF1 3C 3AF9 00
3AF2 7E 3AFA 00
3AF3 18 3AFB 00
3AF4 18 3AFC 00
3AF5 18 3AFD 00
3AF6 18 SAFE 00
3AF7 00 3AFF FF

-11 75-

7

CHARACTERS

CEE XX)
(XIE XX)

BEBE X XL)
e_|sss..n

CEBRBE j |
CEE [EE
EE EN |

3B08 00
3B09 00
3B0A 78
3B0B OC
3B0C 7C
3B0D CC
3B0E 76
3B0F 00
3B18 00
3B19 00
3B1A 3C
3B1B 66
3B1C 60
38B1D 66
3B1E 3C
381F 00 CLIC
3823 © CL
3B29 00
3B2A 3C
3B2B 66
3B82C 7E
3B2D 60
3B2E 3C

3B2F 00
3B38 00
3B39 00
3B3A SE
3B3B 66
3B3C 66
3B3D 3E
SF 70 CSSS
3B3F 7C
3B48 18
3B49 00
3B4A 38
3B4B 18
3B4C 18
3B4D 18
3B4E 3C
3B4F 00
3B58 E0
3B59 60
3B5A 66
3B5B 6cC
3B5C 78
3B5D 6C
3B5E E6
3B5F 00

-11 76-

3B60
3B61
3B62
3B63
3B64
3B65
3B66
3B67

3B70
3B71
3B72
3B73
3B74
3B75
3B76
3B77

3B80
3B81
3B82
3B83
3B84
3B85
3B86
3B87

3B90
3B91
3B92
3B93
3B94
3B95
3B96
3B97

3BAO
3BA1
3BA2
3BA3
3BA4
3BAS
3BA6
3BA7

3BB0
3BB1
3BB2
3BB3
3BB4
3BB5
3BB6
3BB7

CHARACTERS

3B68
3B69
3B6A
3B6B
3B6C
3B6D
3B6E
3B6F

3B78
3B79
3B7A
3B7B
3B7C
3B7D
3B7E
3B7F

3B88
3B89
3B8A
3B8B
3B8C
3B8D
3B8E
3B8F

3B98
3B99
3B9A
. 3B9B
3B9C
3B9D
3B9E

ONU 3B9F

COBBCCCE 5e
| 3BA9
CEsERE \ SBAA
CEE XX 3BAB
CEE CCC 3BAC
OMC je
6 3BAE
Ses.» 3BAF

XIE 3BB8
CCC CC 3BB9
LEE | EE _ 3BBA
EE ( EE 3BBB
CEE CEE 3BBC
(EEE \_ 3BBD

CONS See
3BBF

“11 77-

CEsEEE À

3BC0
3BC1
3BC2
3BC3
3BC4
3BC5
3BC6
3BC7

8BD0
3BD1
3BD2
3BD3
3BD4
3BD5
3BD6
3BD7

3BEO
3BE1
3BE2
3BE3
3BE4
3BES
3BE6
3BE7

3BFO
3BF1
3BF2
3BF3
3BF4
3BF5
3BF6
3BF7

3C00
8C01
3C02
3C03
3C04
3C05
3C06
3C07

3C10
3C11
3C12
3C13
3C14
3C15
3C16
3C17

CHARACTERS

OXCEEEE
(CCE

FE

3BC8
3BC9
3BCA
3BCB
3BCC
3BCD
3BCE
3BCF

3BD8
3BD9
3BDA
3BDB
3BDC
3BDD
3SBDE
3BDF

3BE8
3BE9
SBEA
3BEB
3BEC
3SBED
3BEE
SBEF

3BF8
3BF9
3BFA
3BFB
3BFC
3BFD
3SBFE
3SBFF

3C08
3C09
3C0A
3C0B
3C0C
3C0D
3C0E
3C0F

3C18
3C19
3C1A
3C1B
3C1C
3C1D
3C1E
3C1F

-11 78-

OUUDUUU
CREER
CÉTEERE)

CLECELLE)
SSH92929

3C20
3C21
3C22
3C23
3C24
3C25
3C26
3C27

3C30
3C31
3C32
3C33
3C34
3C35
3C36
3C37

3C40
3C41
3C42
3C43
3C44
3C45
3C46
3C47

3C50
3C51
3C52
3C53
3C54
8C55
3C56
3C57

3C60
3C61
3C62
3C63
3C64
3C65
3C66
3C67

3C70
3C71
3C72
3C73
3C74
3C75
3C76
3C77

CHARACTERS

(OOEEE

(XCESEEE

(XXX EE
(XXX EEE
(XXXEES

QUUUUULU
QUUUUUU

(XX EEE
(XXIEEE

DUUUUUL
OQUUUUUL

del nou

-I]1 79-

3C28
3C29
3C2A
3C2B
3C2C
3C2D
3C2E
3C2F

3C38
3C39
3C3A
3C3B
3C3C
3C3D
3C3E
3C3F

3C48
3C49
3C4A
3C4B
3C4C
3C4D
3C4E
3C4F

3C58
8C59
3C5A
3C5B
3C5C
3C5D
3C5E
3C5F

3C68
3C69
3C6A
3C6B
3C6C
3C6D
3C6E
3C6F

3C78
3C79
3C7A
3C7B
3C7C
3C7D
3C7E
3C7F

QUUUUL
DUUUUUL)

QUDUUUUU
QUJUUL
BE \ X 1

BE \ X 7)
UUUSS.
EE À X 1)

BERREESE

QUUUUDU
JUUJUUUU
(UXTEEEE

=

(XX EEE
sens
BEBE
SEE | X 1)
BEBE \ 1]

as À X 0

DUUUUUU
CESSE
BERSRRER

DOUDOU)

ï

3C80
3C81
3C82
3C83
3C84
3C85
3C86
3C87

3C90
3C91
3C92
3C93
3C94
3C95
3C96
3C97

3CA0
SCA
3CA2
3CA3
3CA4
3SCA5
3CA6
3SCA7

3CB0
3CB1
3CB2
3CB3
3CB4
3CB5
3CB6
3CB7

3CC0
3Ccc1
3CC2
3CC3
8CC4
3CC5
8CC6
3CC7

3CD0
3CD1
3CD2
3CD3
3CD4
3CD5
3CD6
3CD7

CHARACTERS

3C88
3C89
3C8A
3C8B
3C8C
3C8D
3C8E
3C8F

3C98
3C99
3C9A
3C9B
3C9cC
3C9D
3C9E
3C9F

3CA8
3CA9
SCAA
3CAB
SCAC
3CAD
3SCAE
3CAF

3CB8
3CB9
3CBA
3CBB
3CBC
3CBD
3CBE
3CBF

3CC8
3CC9
3CCA
3CCB
3CCC
3CCD
3CCE
3CCF

3CD8
3CD9
3CDA
83CDB
3CDC
8CDD
SCDE
3CDF

-11 80-

SEVRES
UOUUOUUUL
ssssss.s.n
ses...
sesssssn

3CE0
3CE1
3CE2
3CE3
3CE4
3CE5
3CE6
3CE7

3CF0
3CF1
3CF2
3CF3
3CF4
3CF5
3CF6
3CF7

3D00
3D01
3D02
3D03
3D04
3D05
3D06
3D07

3D10
3D11
3012
3013
3D14
3D15
3D16
3D17

3D20
8D21
3D22
3023
3D24
3D25
3D26
8D27

3D30
3031
3D32
3033
3D34
3D35
3D36
3037

CHARACTERS

3D95
3D97

CHARACTERS

nr

MOE )E 0 )
(XXX EEE

3D48
3D49
3D4A
3D4B
3D4C
3D4D
3D4E
3D4F

3D58
3D59
3D5A
3D5B
3D5C
3D5D
3D5E
3D5F

3D68
3D69
3D6A
3D6B
3D6C
3D6D
SD6E
3D6F

3D78
3D79
3D7A
3D7B
3D7C
3D7D
3D7E
3D7F

3D88
3D89
3D8A
3D8B
3D8C
3D8D
3D8E
3D8F

3D98
3D99
3D9A
3D9B
3D9C
3D9D
3D9E
3D9F

-I] 82-

Snoomaoæs

8388n388

08

18

_
œ

CC0SeEE |
(EE EE

3DAO
3DA1
3DA2
3DA3
3SDA4
SDAS5
3DA6
3SDA7

3DB0
3DB1
3DB2
3DB3
3DB4
3DB5
3DB6
3DB7

3DCO
3DC1
3DC2
3DC3
3DC4
3DC5
3DC6
3DC7

3DD0
3DD1
3DD2
3DD3
3DD4
3DD5

3DD7

SDE0
SDE1
SDE2
3SDE3
3DE4
3SDES5
3DE6
SDE7

3DF0
3DF1
3DF2
3DF3
3DF4
3DF5
3DF6
3DF7

CHARACTERS

-[]1 83-

3DA8
3DA9
SDAA
3DAB
SDAC
SDAD
SDAE
3DAF

3DB8
3DB9
3SDBA
3DBB
3DBC
3DBD
3DBE
3DBF

3DC8
3DC9
3DCA
3DCB
3DCC
3DCD
3DCE
3DCF

3DD8
3DD9
3SDDA
3DDB
3DDC
3DDD
3DDE
3DDF

SDE8

* SDE9

SDEA
3DEB
3DEC
3DED
3SDEE
SDEF

3DF8
3DF9
3SDFA
3DFB
3DFC
3DFD
3DFE
3DFF

CHARACTERS

3E00 18 8E08 18
8E01 30 3E09 oc
8E02 60 SE0A 06
8E03 Co 3E0B 03
8E04 80 SEOC 01
3E05 00 SE0D 00
3E06 00 3E0E 00
3E07 00 SEO0F 00
8E10 00 3E18 00
3E11 00 3E19 00
8E12 00 SE1A 00
3E13 01 3E1B 80
3E14 03 8E1C Co
3E15 06 3E10 60
3E16 oc SE1E 30
3E17 18 SE1F 18
3E20 18 8E28 18
3E21 3C 3E29 oc
3E22 66 SE2A 06
3E23 C3 3E2B 03
SE24 81 3E2C 03
3E25 00 3E2D 06
3E26 00 SE2E oc
8E27 00 SE2F 18
8E30 00 3E38 18
8E31 00 8E39 30
3E32 00 SE3A 60
3E33 81 3E3B co
3E34 C3 8E3C Co
3E35 66 3E3D 60
3E36 3C SE3E 30
3E37 18 3E3F 18
8E40 18 3E48 18
8E41 30 3E49 oc
3E42 60 SE4A 06
3E43 C1 3E4B 83
3E44 83 3E4C C1
3E45 06 SE4D 60
3E46 oc SE4E 30
3E47 18 SE4F 18
8E50 18 3E58 C3
8E51 3C 8E59 E7
8E52 66 SES5A 7E
3E53 C3 3E5B 3C
8E54 C3 SE5C 3C
3E55 66 8E5D 7E
3E56 3C nn sen SESE E7
8E57 18 8E5F C3

3E60
3E61
3E62
3E63
3E64
3E65
3E66
3E67

3E70
3E71
3E72
3E73
3E74
3E75
3E76
3E77

3E80
3E81
3E82
3E83
3E84
3E85
3E86
3E87

3E90
3E91
3E92
3E93
3E94
8E95
3E96
3E97

SEAO
SEA1
3EA2
3SEA3
3EA4
SEAS
SEA6
SEA7

3EB0
3EB1
3EB2
3EB3
3EB4
3EB5
3EB6
3EB7

CHARACTERS

(XXIXUEE
RE

CXCOBBE
(XOEBE T )

3E68
3E69
3SE6A
3E6B
3E6C
8E6D
SE6E
SE6F

3E78
3E79
SE7A
3E7B
3E7C
3E7D
8E7E
3E7F

3E88
3E89
3SE8A
3E8B
3E8C
8E8D
SE8E
3E8F

3E98
3E99
SE9A
SE9B
SE9C
8E9D
3E9E
3E9F

BE
LL seses
B_ | sen
en || es
CXCME
(XX EEE

XX EEE

CHARACTERS

3ECO AA SECB OA
SECT 55 SEC9 05
3EC2 AA SECA OA
3EC3 55 3ECB 05

3EC4 00 3ECC OA
3EC5 00 3ECD 05
3EC6 00 3ECE OA
3EC7 00 SECF 05
3EDO 00 3ED8 AO
3ED1 00 3ED9 50
3ED2 00 3EDA AO
3ED3 00 3EDB 50
3ED4 AA 3EDC AO
3ED5 55 3EDD 50
3ED6 AA SEDE AO
3ED7 55 3EDF 50
3EE0O AA 3EE8 AA
3EE1 54 3EE9 55
3EE2 A8 SEEA  2A
3EE3 50 3EEB 15
3EE4 AO 3EEC OA
3EE5 40 SEED 05
3EE6 80 3EEE O2
3EE7 00 SEEF O1

SEFO O1 SEF8 00
3EF1 02 3EF9 80
SEF2 05 SEFA 40
3EF3 OA 3EFB AO
3EF4 15 SEFC 50
3EF5  2A 3EFD A8
3EF6 55 3EFE 54

3EF7 AA 3EFF AA
3F00 7E 3F08  7E
3FO1 FF 3F09 FF
3F02 99 3FOA 99
3F03 FF 3FOB FF
3F04 BD 3F0C C3
3F05 C3 3F0D BD
3F06 FF 3FOE FF
3F07  7E 3F0F  7E
3F10 38 3F18 10
3F11 38 3F19 38
3F12 FE RES | 3F1A  7C
3F13 FE  BÉBESET. 3F1B FE
3F14 FE 3F1C  7C
3F15 10 3F1D 38
3F16 38 3F1E 10
3F17 00 (LILI 3F1F 00

-11 86-

3F20
3F21
3F22
3F23
3F24
3F25
3F26
3F27

3F30
3F31
3F32
3F33
3F34
3F35
3F36
3F37

3F40
3F41
3F42
3F43
3F44
3F45
3F46
3F47

3F50
3F51
3F52
3F53
3F54
3F55
3F56
3F57

8F60
3F61
3F62
3F63
8F64
3F65
3F66
8F67

3F70
3F71
8F72
3F73
3F74
3F75
3F76
3F77

CHARACTERS

CCC |

3F28
3F29
3F2A
3F2B
8F2C
3F2D
3F2E
3F2F

3F38
3F39
3F3A
3F3B
3F3C
3F3D
3F3E
3F3F

3F48
3F49
3F4A
3F4B
3F4C
3F4D
3F4E
3F4F

8F58
3F59
SF5A
3F5B
3F5C
3F5D
SFSE
3F5F

8F68
3F69
SF6A
3F6B
8F6C
3F6D
SF6E
3F6F

3F78
3F79
3F7A
3F7B
3F7C
3F7D
3F7E
3F7F

CHARACTERS

3F80 18 3F88 18
3F81 3C 3F89 18
3F82 7E 3SF8A 18
3F83 FF 3F8B 18
3F84 18 3F8C FF
3F85 18 3F8D 7E
3F86 18 3F8E 3C
3F87 18 3F8F 18
3F90 10 3F98 08
3F91 30 3F99 oc
3F92 70 3SF9A 0E
3F93 FF 3F9B FF
3F94 FF 3F9C FF
3F95 70 3F9D 0E
3F96 30 SF9E oc
8F97 10 3F9F 08
3FAO 00 SFA8 00
SFA 00 SFA9 00
SFA2 18 SFAA FF
3FA3 3C 3SFAB FF
SFA4 7E SFAC 7E
8FAS FF SFAD 3C
3SFA6 FF SFAE 18
SFA7 00 SFAF 00
3FBO 80 3FB8 02
3FB1 EO 3FB9 0E
3FB2 F8 SFBA $E
3FB3 FE 3FBB FE
3FB4 F8 3FBC SE
3FB5 E0 3FBD 0E
3FB6 80 SFBE 02
3FB7 00 8FBF 00
3FCO 38 3FC8 38
3FC1 38 8FC9 38
8FC2 92 8SFCA 10
3FC3 7C 3FCB FE
3FC4 10 8FCC 10
3FC5 28 3FCD 28
8FC6 28 SFCE 44
3FC7 28 SFCF 82
8FD0O 38 SFD8 38
8FD1 38 3FD9 38
3FD2 12 SFDA 90
3FD3 7C 3FDB 7C
3FD4 90 3FDC_ 12 (LC EC
3FD5 28 8FDD 28 CUBE CC
3FD6 24 SFDE 48 CE EE XX
3FD7 22 SFDF 88 ŒC XX EN À

-I1 88-

SFEO
SFE1
SFE2
SFE3
3FE4
SFES
3FE6
SFE7

3FFO
3FF1
3FF2
SFF3
SFF4
8FF5
3FF6
8FF7

CHARACTERS

-I1 89-

SFE8
SFE9
SFEA
SFEB
SFEC
8SFED
SFEE
SFEF

3FF8
3FF9
SFFA
SFFB
SFFC
3FFD
SFFE
SFFF

LE BASIC

3.1 L'interpréteur Basic du CPC

Le CPC dispose d’un interpréteur Basic rapide et puissant qui est logé
dans une Rom de 16 K,. Il occupe la zone d'adresses 8C000 à &8FFFF,
parallèlement à la Ram écran. Pour le programme Basic et pour les
variables Basic, la zone de 80170 à &AB80 est disponible, ce qui
représente 43534 octets.

L'interpréteur soutient presque toutes les possibilités offertes par
l'électronique et le système d'exploitation de l'ordinateur, Cela
comprend notamment la sortie sur écran avec Jusqu'à 8 fenêtres, le
graphisme haute résolution, le son ainsi que le traitement d’event. Il
est ainsi pour la première fois possible de faire exécuter en Basic
plusieurs tâches parallèlement. L'’interpréteur Basic offre en outre une
arithmétique avec des nombres entiers de 16 bits (zone de valeurs de -
32768 à 32767) et une arithmétique avec virgule flottante avec puissance
de deux sur 8 bits et une mantisse de 32 bits qui garantit une précision
de 9 décimales pour une zone de valeurs de +- 1E-39 à +- 1E+38.

L'arithmétique entière ou l’arithmétique à virgule flottante ne font
cependant pas partie de l'interpréteur Basic mais de la Rom du système
d'exploitation (adresses 82E18 à &3/FF). Elles sont appelées comme les
autres fonctions du système d'exploitation à travers la table de saut qui
se trouve dans le haut de la Ram (8BB00 à gBDF1) et qui peut être
modifiée en cas de besoin.

L'interpréteur Basic permet également la création, l'édition
(examen/modification) et l'exécution aisées de programmes. La création
de programmes est en effet facilitée par l'instruction AUTO, l'édition
par l'instruction EDIT qui, grâce à la puissance du système
d'exploitation, est à peine moins maniable que l'éditeur plein écran
ainsi que par les instructions RENUM, MERGE et DELETE. L’exécution des
programmes est également facilitée par des instructions puissantes. Par
exemple l'instruction ON ERROR GOTO permet le traitement des erreurs.
L'instruction DEFtype permet de définir le type d’une variable,
l'instruction ERASE permet une suppression sélective de tableaux. 11 est
encore possible d'entrer et de faire sortir lés nombres comme des nombres
décimaux, binaires ou hexadécimaux ainsi que d'utiliser des fonctions

-11 90-

qu’on a soi-même définies, fonctions qui peuvent comporter plusieurs
arguments. Enfin les structures de programme telles que IF ,.. THEN ...
ELSE, FOR .., NEXT et WHILE ... WEND sont un autre aspect très important
de la puissance du Basic du CPC. Il est également possible en Basic de
réaffecter es touches du clavier, de définir les fonctions des touches

de fonction ou de définir des caractères qui apparaîtront à l'écran. Il
ne manque ni l'instruction TRACE ni une très complète instruction PRINT
USING.

Après ce bref aperçu, nous allons nous pencher de plus près sur l'entrée
et le stockage des lignes de Basic, ainsi que sur l'exécution des
programmes par l’interpréteur Basic. Ces informations vous permettront
non seulement de pouvoir tirer le maximum de votre interpréteur Basic
mais également d'écrire vos propres extensions du Basic. Nous vous
donnerons plus loin quelques exemples d'extensions du Basic.

L'entrée de lignes Basic

Lorsque vous entrez une ligne Basic, elle est d'abord placée dans un
buffer de 256 octets qui se trouve aux adresses &ACA4 à &ADA3. L'entrée y
figure en clair, non codée. Si la ligne commence par un numéro, celui-ci
est converti en un nombre binaire de 16 bits et placé dans un second
buffer destiné à recevoir la ligne traitée. Ce buffer comprend 300
caractères et 11 se trouve avant le programme Basic, aux adresses 840 à
&16F. La ligne entrée est alors examinée pour voir si elle comporte des
mots-clé Basic, Ces mots-clés sont remplacés par un octet appelé token,
Par exemple ‘AFTER’ devient le token 880. Les tokens de tous les mots
d'instruction et des opérateurs Basic tels que ‘=’ ou ‘AND’ ont des
valeurs supérieures à 127, c'est-à-dire que leur bit 7 est mis. Les
fonctions Basic comme EXP ou ROUND ont des tokens compris entre 0 et 87F.
Pour les distinguer des caractères ASCII normaux, 11s sont marqués par un
&FF les précédant, Le double-point servant à séparer entre elles deux
instructions est représenté par le code 801, la fin d'une ligne est
marquée par un 800, Si une suite de lettres n’a pu être identifiée comme
étant une instruction ou une fonction, elle est traitée comme étant le
nom d’une variable, Un nom de variable peut comprendre jusqu’à 40
caractères qui sont tous significatifs. Aucune différence n'est faite
entre les majuscules et les minuscules. Supposons que nous ayons entré la
ligne suivante:

-11 91-

10 start=77
Après le numéro de ligne seront placées les valeurs:
80D 800 800 873 e74 861 872 8F4 8EF 819 e4D 800

Le 80D indique qu’il s'agit d’une variable sans marque de type, Ensuite
viennent deux O sur lesquels nous reviendrons plus tard. Puis vient le
nom de la variable, les codes ASCII pour s, t, a et r. Pour la dernière
lettre, ’t’, 880 est ajouté au code ASCII 874 (le bit supérieur est mis)
et nous obtenons 8F4. Le code &EF est le token pour ‘=’, Le code 819 qui
suit indique une constante à un octet: &4D est la valeur de cette
constante (=77 en décimal). Le zéro qui termine marque la fin de la
ligne,

Avant le numéro de ligne, 11 y a encore deux octets qui indiquent la
longueur de la ligne:

812 800 80A 800

La ligne comporte donc 812+256*800 soient 18 octets et elle porte le
numéro de ligne 80A+256*800, soit 10.

Vous voyez donc qu'au contraire de ce qui est le cas avec d’autres
interpréteurs Basic, les constantes ne sont pas placées dans le texte du
programme sous forme de textes ASCII, mais sous la forme de leur
traduction binaire. Ceci présente un avantage décisif. La conversion du
format ASCII au format binaire prend en effet du temps. Avec la technique
utilisée sur le CPC, cette conversion ne s'effectue qu’une seule fois,
lors de l’entrée de la ligne et elle n’a donc pas à être effectuée chaque
fois que la ligne est exécutée, I1 en découle un gain de vitesse dans
l'exécution des programmes qui n’est pas négligeable,

Le CPC connaît d'autre part toute une série de constantes numériques qui
sont désignées par un token particulier. Les constantes qui ne
comprennent par exemple qu'un seul chiffre, soient les nombres de 0 à 9
sont ainsi codées avec les tokens 80E à 817, Elles n'occupent ainsi qu’un
octet dans le texte du programme. Nous avons déjà rencontré le token 819
qui marque les valeurs numériques d'un octet. Pour les valeurs entières
sur deux octets, 11 y a trois tokens différents, suivant que la constante

-II 92-

a été entrée sous la forme décimale, binaire ou hexadécimale, La valeur
de la constante est toujours stockée de la même façon avec un octet
faible et un octet fort,

&1A valeur sur deux octets, décimal
&1B valeur sur deux octets, binaire
&1C valeur sur deux octets, hexadécimal

S'il ne s’agit pas d’un nombre entier ou si sa valeur est supérieure à
32767, le nombre est stocké sous la forme d'une valeur à virgule
flottante qui est désignée par le token 8&1F. Le token est suivi de la
valeur à virgule flottante sur 5 octets. Nous reviendrons plus tard sur
les valeurs à virgule flottante,

Dans ce contexte, les numéros de ligne ont une situation particulière
lorsqu'ils suivent par exemple des instructions telles que GOTO, GOSUB ou
RUN. Ils sont également stockés sous la forme binaire, mais 11s sont
désignés par le token &1E.

Lorsqu'un programme est exécuté et qu’il rencontre par exemple une
instruction GOTO, 11 lit alors le numéro de ligne et il doit rechercher
cette ligne dans tout le programme. Sur des programmes de taille
importante, cela peut durer assez longtemps, Les instructions GOTO et
GOSUB sont souvent utilisées dans des boucles qui sont parcourues des
centaines ou des milliers de fois. Dans ce cas, le temps de recherche des
numéros de ligne peut représenter un part importante du temps d'exécution
du programme, L'’interpréteur Basic du CPC n'effectue cette recherche de
ligne qu'une seule fois. En effet, une fois qu’il a trouvé la ligne
recherchée, il remplace le numéro de ligne figurant à la suite de
l'instruction GOTO par l'adresse de cette ligne qu’il Vient de trouver,
Pour qu’il puisse faire la différence entre un numéro de ligne et une
adresse de ligne, il remplace le token 81E par le token 81D, qui est le
token pour les adresses de ligne, Si la même instruction GOTO est
exécutée une seconde fois, l’interpréteur trouve directement l'adresse à
laquelle le programme doit sauter, ce qui permet bien sûr de gagner
beaucoup de temps.

Cette technique crée cependant quelques difficultés pour les instructions

qui utilisent le numéro de ligne en tant que tel. Lorsque l'instruction
LIST doit par exemple sortir le numéro de ligne, c’est le numéro de ligne

-11 93-

qu'elle doit indiquer et non l'adresse de la ligne. Ce problème est
cependant très facilement résolu, En effet lorsque l'adresse de la ligne
est connue, il est facile d'aller y rechercher le numéro de ligne
puisque, comme nous l'avons vu, le numéro de ligne est stocké dans la
ligne. Lorsque des lignes sont supprimées ou que d’autres lignes sont
ajoutées, les adresses de ligne doivent être remplacées par les numéros
de ligne car de telles opérations entraînent bien sûr une modification
des adresses de ligne. Cela ne présente cependant d’inconvénient que pour
l'entrée et la sortie de lignes de programmes, Ce petit inconvénient est
cependant largement compensé par la vitesse nettement plus grande
d'exécution des programmes.

L'exécution des programmes par l’interpréteur Basic

L'exécution d’une instruction par l’interpréteur Basic se présente, en
simplifiant un peu, de la façon suivante. Chaque ligne de programme
commence, comme nous l'avons dit, par la longueur de la ligne et le
numéro de ligne. Ensuite vient l'instruction Basic proprement dite.
L'interpréteur examine maintenant s’il s’agit d’un token d'instruction,
dont la valeur est toujours comprise entre 880 et aDC. Si c'est le cas,
il utilise ce token comme pointeur d’une table qui contient les adresses
de toutes les instructions Basic. L’instruction Basic est alors exécutée
comme Un sous-programme, On revient ensuite à ce qu’on appelle la boucle
de l’interpréteur. Si l'instruction ne commençait cependant pas par un
token d'instruction, on saute à l'instruction LET,

La partie la plus importante de l'interpréteur Basic est certainement le
calcul des expressions. Le CPC distingue à cet égard trois types
d'expressions: entières, à virgule flottante et chaînes de caractères.
Lorsque par exemple une affectation de valeur à une variable est exécutée
ou lorsque le paramètre d’une instruction doit être calculé, une routine
est appelée qui calcule l'expression et qui fournit la valeur ainsi que
le type de l'expression, Le type de variable peut avoir trois valeurs
différentes:

2 entier

3 chaîne
5 virgule flottante

-11 94-

Ce numéro de type donne en même temps la longueur de la variable. Pour
une chaîne, c’est ce qu’on appelle le Descriptor qui contient la longueur
et l'adresse de la chaîne (voyez également le chapitre sur le pointeur de
variable). Si cependant le type d’une expression est différent du type
d'une variable à laquelle cette expression doit être affectée, une
conversion de type est tentée, mais seulement entre les deux types
numér {ques entier et à virgule flottante. Cette conversion prend bien sûr
un certain temps et 11 est donc préférable d'employer des variables
entières lorsque c'est possible. L'expérience révèle en effet que le type
entier convient dans 90 % des cas. Non seulement le type entier évite les
conversions de types, mais l’arithmétique entière est en outre nettement
plus rapide que l'arithmétique à virgule flottante. Cette remarque vaut
particulièrement pour les variables de comptage utilisées par exemple
dans les boucles FOR...NEXT,

Par contre, si vous tentez d’affecter une expression du type chaîne de
caractères à une variable numérique ou vice versa, le message d'erreur
“Type mismatch’ sera sorti, La conversion de chaîne de caractères à
numér {que et vice versa n'est possible qu'avec les fonctions VAL et STR$.

-I1 95-

3.2 La pile Basic

Une pile ou mémoire de pile (stack) permet de stocker des données suivant
le principe ‘Last in - First out’ (dernier entré - premier sorti), Le
processeur utilise à cet effet la zone de mémoire commençant en &CO000.
Avant chaque entrée, le pointeur de pile (stack pointer) est décrémenté,.
Lorsqu'on retire des données de la pile, le pointeur de pile est
incrémenté immédiatement après, La pile du processeur sert par exemple à
placer les adresses de retour lors de l'appel de sous-programmes et elle
permet, grâce au principe d'accès utilisé, de réaliser une imbrication
des sous-programmes.

L'interpréteur Basic a également besoin d’une pile pour stocker les
paramètres des appels par GOSUB ou des boucles FOR-NEXT et WHILE-WEND.
Seule une pile permet en effet de réaliser une imbrication de ces
différentes structures de programme, On utilise pas à cet effet la pile
du processeur car il existe un pile Basic de 512 octets qui commence à
l'adresse 8&AE8B. Au contraire de la pile du processeur, cette pile croît
vers les adresses plus élevées, au fur et à mesure que le nombre
d'entrées augmente, Jusqu'à l'adresse limite &BO8A. Les cases mémoire
&B08B et e&B08C font office de pointeur de pile,

Voyons d’abord quels paramètres sont placés sur la pile pour une
instruction GOSUB:

&00/801 marque du type de GOSUB

Lo Adresse de l'instruction suivant
Hi l'instruction GOSUB

Lo Adresse de la ligne de

Hi l'instruction GOSUB

806 Taille de l’entrée sur la pile

Un octet est donc tout d’abord placé sur la pile qui détermine type de
l'instruction GOSUB. Pour un GOSUB normal, 11 s’agit d'un octet nul, S'il
s'agit cependant de l’appel d’un sous-programme par une instruction AFTER
ou EVERY, c'est un 1 qui sera placé sur la pile. Viennent ensuite
l'adresse de la prochaine instruction après l'instruction GOSUB ainsi que

-I1 96-

l'adresse de la ligne dans laquelle figure l'instruction GOSUB. Pour que
l'entrée sur la pile puisse être identifiée à nouveau lorsque
l'instruction RETURN sera exécutée, un octet est encore placé sur la pile
qui indique la longueur de l'entrée sur la pile et indique ainsi
implicitement qu’il s’agit d'un enregistrement concernant une instruction
GOSUB.

Les données pour une boucle WHILE-WEND sont placées de façon similaires:

Lo Adresse de ta ligne de

Hi l'instruction WHILE

Lo Adresse de

Hi l'instruction WEND

Lo Adresse de

Hi la condition WHILE

&07 Taille de l'entrée sur la pile

L'entrée comporte donc trois adresses et un octet d'identification qui
vaut 7 et qui indique également le nombres d'octets de données entrés sur
la pile.

Les choses se compliquent un peu avec la boucle FOR-NEXT. On fait ici une
distinction selon que la variable de comptage est du type entier ou du
type réel, Dans le premier cas, non seulement le temps d'exécution est
plus court, mais la place occupée sur la pile est en outre moindre.
Considérons tout d’abord la structure d’une boucle de type entier.

Lo Adresse de la

Hi variable de comptage
Lo Valeur finale de la

Hi variable de comptage

Lo Valeur STEP

Hi

Sgn Signe de la valeur STEP

-11 97-

Lo Adresse de

Hi l'instruction FOR

Lo Adresse de la ligne de

Hi l'instruction FOR

Lo Adresse de

Hi l'instruction NEXT

Lo Adresse de la ligne

Hi de l’'instruction NEXT

&10 Taille de l'entrée sur la pile

L'entrée sur la pile pour une boucle FOR-NEXT avec variable entière est
donc longue de 16 octets, Si une boucle utilise une variable de comptage
de type réel, ce sont 22 octets qui seront placés sur la pile,

Lo Adresse de la
Hi variable de comptage

Valeur à virgule Valeur finale de la
flottante sur variable de comptage
5 octets

Valeur à virgule Valeur STEP
flottante sur

5 octets

sgn Signe de la valeur STEP
Lo Adresse de

Hi l'instruction FOR

Lo Adresse de la ligne de
Hi l'instruction FOR

Lo Adresse de

Hi l'instruction NEXT

-11 98-

Lo Adresse de la ligne
Hi de l'instruction NEXT

816 Taille de l'entrée sur la pile

Outre le stockage des structures de programme, la pile Basic sert
également au stockage d'expressions provisoires pour les calculs
numériques, par exemple pour le calcul d'expressions imbriquées entre
parenthèses et pour réaliser une hiérarchie pour les opérateurs
arithmétiques et logiques.

-11 99-

3.3 Les vecteurs Basic

Si vous voulez réaliser vos propres instructions ou fonctions Basic, vous
pouvez utiliser des Roms d'extension ou RSX mais vous pouvez également
utiliser ce qu'on appelle les vecteurs Basic.

L'interpréteur Basic utilise dans tous les endroits stratégiques un sous-
programme de la Ram qui ne se compose normalement que d'une instruction
RET et qui n’influence donc pas le cours des opérations. Par exemple, si
une instruction doit être exécutée, on teste d’abord si l'instruction
commence par un token d'instruction valable, Si c'est bien le cas,
l'adresse de l'instruction correspondante est calculée grâce au token et
on saute à cette adresse, Si par contre aucune instruction valable n’a
été identifiée, un SYNTAX ERROR est annoncé. Avant que ne soit cependant
sorti le message d'erreur -et c'est 1à qu'est l'astuce- la routine dont
nous parlions plus haut est appelée. Celle-ci ne se compose normalement
que d’une instruction RET et elle est donc normalement immédiatement
abandonnée, Si vous voulez donc intégrer votre propre instruction, il
vous suffit de remplacer cette instruction RET qui figure en Ram par un
saut à votre propre routine qui contrôlera la validité de la nouvelle
instruction et l’exécutera,

Cette méthode est plus souple que la méthode RSX -on n’est pas limité aux
arguments entiers (voir chapitre 3.5.2)- et son exécution est plus rapide
puisqu'il n'est pas nécessaire de rechercher le mot d'instruction
correcte dans la table de toutes les extensions.

Le tableau suivant contient les adresses des 9 routines qui peuvent être
manipulées par l'utilisateur. L'interpréteur Basic n'appelle pas ici une
routine, mais saute directement à la sortie des erreurs (voir listing du
Basic, adresse 8D078),

ACO1-AC03 Branchement pour mode READY

ACO4-ACO6 Branchement pour entrée ERROR

ACO7-AC09 Branchement pour exécuter instruction

ACOA-ACOC Branchement pour calcul de fonction

ACOD-ACOF Branchement pour aller chercher constante (inutilisé)
AC10-AC12 Branchement pour entrée, convertir ligne en token
AC13-AC15 Branchement pour sortie, lister token

AC16-AC18 Branchement pour entrée, conversion de chiffres

-[1 100-

AC19-AC1B Branchement pour opérateurs

L'exemple suivant montre un exemple d'application de ces vecteurs. On
utilise pour cela l'instruction SWAP dont le token existe déjà pour
échanger deux variables entre elles.

:Swap de variables
LE. 15/12/84

00E7 SWAP EQU  2gE7;token SWAP

CA94 ERROR  EQU  eCAÿä;sortie erreurs

000D MISM EQU 13; 'TYPE MISMATCH'’

D686 GETVAR EQU  eD686;chercher variable
DD37 CHECK  EQU  &DD37;:examiner caractère
DD3F BLANK  EQU  gDD3F:ignorer les espaces
ACO7 ORG  &ACO7

ACO7 C30080 JP SWPNEW;: détourner le vecteur
8000 ORG 88000

8000 FECE SWPNEW CP SWAP*2 & eFFstoken SWAP?
8002 CO RET NZ; 'SYNTAX ERROR'

8003 D1 POP ‘ DE:retirer adresse de retour de pile
8004 CD3FDD CALL BLANK

8007 CD86D6 CALL GETVAR; chercher variable
800A CD37DD CALL CHECK

800D 2C DEFB “,"stester si virgule
800E ED534C80 LD (VAR1), DE

8012 C5 PUSH BC;et ranger le type
8013 CD86D6 CALL GETVAR

8016 ED534E80 LD (VAR2),DE

801A 79 LD AC

801B C1 POP BC

801C B9 CP C:comparer les types

801D 2805 JR Z, TYPOK

801F 1E0D LD E,MISM

8021 C394CA JP ERROR

-11 101-

8024 E5 TYPOK PUSH HL;ranger le pointeur de programme

8025 0600 LD B,0

8027 2A4E80 LD HL, (VAR2)

802A 114780 LD DE, TEMP

802D C5 PUSH BC

802E EDBO LDIR ;variable 2 => TEMP

8030 C1 POP BC

8031 2A4C80 LD HL, (VART)

8034 ED5B4E80 LD DE, (VAR2)

8038 C5 PUSH BC

8039 ED80 LDIR variable 1 => variable 2
803B C1 POP BC

803C 214780 LD HL, TEMP

803F ED5B4C80 LD DE, (VAR1)

8043 EDBO LDIR ;TEMP => variable 1

8045 E1 POP  HL:retirer le pointeur de programme
8046 C9 RET

8047 TEMP DEFS 5;mémoire provisoire

804C VAR DEFS 2;adresse variable 1

804E VAR2 DEFS 2;:adresse variable 2

La case mémoire 8&ACO0 joue également un rôle, Normalement c'est un O qui
y figure. La conséquence en est qu’une ligne Basic est reçue comme elle a
été entrée. Si toutefois on charge dans cette case mémoire une valeur non
nulle, les espaces superflus de la ligne entrée seront ignorés et ne
seront pas stockés avec la ligne.

Si vous entrez par exemple la ligne suivante:
10 FOR 1=1 TO 100: PRINT “Bonjour” : NEXT
vous obtiendrez:
10 FOR i=1 TO 100:PRINT“Bonjour”:NEXT
Cette fonction peut être utile lorsque la place en mémoire encore
disponible se fait rare, Le programme est ainsi comprimé autant qu’il est

possible. L’inconvénient de cette méthode est que le programme risque de
perdre en clarté et lisibilité. Les structures de programme dont voici un

-11 102-

exemple peuvent en effet être repérées plus difficilement,
10FORI=1 TO 100

20 PRINT"Bon] our ”
3ONEXT

-11 103-

3.4 La Ram Basic

Voici une liste vous présentant la signification des adresses de la Ram
utilisées par l’interpréteur Basic.

AB80-ABFF Matrice de caractère pour CHR$(240) à CHR$(255)
ACOO Ignorer le flag pour espace supplémentaires
ACO1-AC03 Branchement pour mode READY

ACOU-AC06 Branchement pour entrée ERROR

ACO7-AC09 Branchement pour exécuter instruction

ACOA-ACOC Branchement pour calcul de fonction

ACOD-ACOF Branchement pour aller chercher constante (inutilisé)
AC10-AC12 Branchement pour entrée, convertir ligne en token
AC13-AC15 Branchement pour sortie, lister token

AC16-AC18 Branchement pour entrée, conversion de chiffres
AC19-AC1B Branchement pour opérateurs

ACC Flag pour mode AUTO

AC1D-AC1E Numéro de ligne pour AUTO

ACIF-AC20 Incrément pour AUTO

AC21 Numéro stream actuel

AC22 Canal d'entrée

AC23 Position actuelle imprimante

AC24 WIDTH

AC25 actuelle position sur cassette

AC26 Flag pour premier parcours FOR-NEXT

AC27-AC2B Mémoire provisoire pour variable FOR
AC2C-AC2D Adresse de l'instruction NEXT correspondante
AC2E-AC2F Adresse de l'instruction WEND correspondante
AC34-AC35 Adresse ON BREAK

AC36-AC3+

AC38-AC43 Sound-Queue 0

AC&4-ACUF Sound-Queue 1

ACSO-AC5SB Sound-Queue 2

ACSC-AC6D Event-block 0

ACGE-AC7F Event-block 1

AC80-AC91 Event-block 2

AC92-ACA3 Event-block 3

ACAU-ADA3 Buffer d'entrée

ADA6-ADA7 Adresse de la ligne ERROR

ADA8-ADA9 Pointeur de programme après ERROR

-11104-

ADAA
ADAB-ADAC
ADAD-ADAE
ADAF-ADBO
ADB1

ADB2

ADB3

ADB4
ADB5-ADB6
ADB7

ADB8
ADB9-ADBA
ADBB-ADBC
ADCB-ADCF
ADDO-AEO3
AEO4-AEO5
AEO6-AEOB
AEOC-AE25
AE2D
AE2E-AE2F
AE30-AE31
AE32-AE33
AE34-AE35
AE36-AE37
AE38
AE3F-AE4O
AEU

AEU2
AEU3-AEU4
AE5
AEUG-AE78
AE72-AE73
AE74
AE75-AE76,
AE77-AE78
AE79
AE7B-AE7C
AE7D-AE7E
AE7F-AE80
AE81-AE82

Numéro ERROR

Pointeur de programme après interruption
Adresse de ligne après interruption
Adresse de la routine ON-ERROR

Routine de traitement des erreurs activée
Paramètre sound état canal

Paramètre sound courbe d’enveloppe du volume
Paramètre sound courbe d’enveloppe du ton
Paramètre sound période

Paramètre sound période bruit

Paramètre sound volume

Paramètre sound durée

ENV & ENV

Mémoire provisoire pour nombre à virgule flottante
Table pour variable d'échelle

Table pour FN

Table pour tableaux

Types de variable A-Z

Caractère de séparation pour instruction INPUT
Adresse de ligne pour instruction READ
Pointeur DATA

Mémoire pour pointeur de pile Basic
Adresse de l'instruction actuelle

Adresse de la ligne de programme actuelle
Flag TRACE

Adresse de début pour LOAD

Flag pour CHAIN MERGE

Type de fichier

Longueur de fichier

Flag pour programme protégé

Buffer pour conversion en ASCII

Adresse pour instruction CALL
Configuration pour instruction CALL

hl pendant l'instruction CALL

sp pendant l'instruction CALL

Largeur du .tabulateur

Pointeur sur HIMEM

Pointeur sur fin de Ram libre

Pointeur sur début Ram libre

Pointeur sur début de programme

-11105-

AE83-AE84 Pointeur sur fin de programme
AE85S-AE86 Pointeur sur début des variables
AE87-AE88 Pointeur sur début des tableaux
AE89-AE8A Pointeur sur fin des tableaux
AE8B-B08A Pile Basic
BO8B-B08C Pointeur de pile Basic
BO8D-BO08E Début des chaînes de caractères
BO8F-B090 Fin des chaînes de caractères
BO9A-BO9B Pointeur sur pile de descripteur de chaîne
BO9C-B0B9 Pile de descripteur de chaîne
BOBA-BOBC Descripteur de chaîne
BOC1 Type de variable
BOC2-B0C3 Variable entière ou
Adresse d’un nombre à virgule flottante
Pointeur sur descriptor de chaîne
B8E4-B8E7 Valeur RND
B8E8-B8EC Mémoire provisoire pour nombre à virgule flottante
B8ED-B8F1 Mémoire provisoire pour nombre à virgule flottante
B8F2-B8F6 Mémoire provisoire pour nombre à virgule flottante
B8F7 Flag pour DEG/RAD

-11106-

3.5 Basic et langage-machine
3,5,1 L'instruction CALL

L'instruction CALL sert de lien entre le Basic et le langage-machine.
Elle permet en effet d'appeler à partir d’un programme Basic un programme
en langage-machine, L'’instruction ÆCALL doit être accompagnée d'une
adresse 16 bits qui indique en quelle adresse figure le programme en
langage-machine, par exemple:

CALL &8000

Cette instruction appellera un programme en langage-machine figurant à
l'adresse 88000 ou 32768 en décimal. Si le programme en langage-machine
se termine par une instruction RET, le contrôle est rendu à
l'interpréteur qui poursuit l'exécution du programme Basic.

Avec l'instruction CALL on ne peut accéder directement au système
d'exploitation ou à l'interpréteur Basic. Pour toute la zone d'adresses
de 64 K, c'est la Ram qui est sélectionnée automatiquement. Il est
cependant possible évidemment d'appeler des routines du système
d'exploitation à travers les adresses d'entrée qui figurent en 8B000. Ces
routines s'occupent elles-même de réaliser la configuration Rom/Ram qui
convient, Si vous voulez accéder avec une instruction CALL à des routines
de l'interpréteur Basic ou à des routines du système d'exploitation qui
ne peuvent être appelées avec des vecteurs, vous pouvez utiliser les
routines RST 3 et RST 5 qui réalisent la commutation.

L'instruction CALL permet cependant également de transmettre des
paramètres du Basic à la routine en langage-machine, Vous pouvez pour
cela transmettre Jusqu'à 32 paramètres qui doivent être placés à la suite
de l'instruction CALL, séparés par des virgules. Ces paramètres, ainsi
que l'adresse elle-même doivent donner une Valeur 16 bits. Ils sont
placés par le Basic sur la pile. L’interpréteur Basic transmet l'adresse
de base du bloc de paramètres dans le registre IX. Dans l'’accumulateur
figure le nombre de paramètres transmis. Le dernier paramètre figure donc
à l'adresse IX, l’avant-dernier à l'adresse IX+2 et le premier paramètre
à l'adresse IX+2*(A-1).

Pendant l'instruction CALL, les contenus de tous les registres peuvent

-11107-

etre modifiés. Le pointeur de pile peut lui aussi être modifié pour
autant qu’on soit sûr que lors de l'exécution de l'instruction RET qui
termine le programme en langage-machine, c’est bien la bonne adresse de
retour qui sera retirée de la pile.

Les applications possibles de l’instruction CALL sont très diverses et
vous pouvez dans ce domaine donner libre cours à votre imagination. Vous
pouvez par exemple créer des fonctions graphiques nouvelles telles que le
dessin de cercles, le remplissage de surfaces, etc...

La transmission de paramètres en retour, de la routine en langage-machine
au Basic n'est pas prévue mais elle reste cependant possible par un petit
détour. Si par exemple le résultat d’un programme en langage-machine doit
etre affecté à une variable, on peut transmettre l'adresse de cette
variable à travers l'instruction CALL, grâce au signe ‘’arobas':

CALL &AB00,@A

L'adresse de la variable A sera ainsi à la disposition du programme en
langage-machine qui pourra modifier directement la valeur de cette
variable. Cette possibilité est décrite plus précisément dans le chapitre
sur le pointeur de variable.

3,5.2 Extensions du Basic avec RSX

Le système d'exploitation et le Basic du CPC soutiennent la possibilité
d'intégrer ses propres instructions dans le Basic. C’est ce qu’on appelle
RSX ‘Resident System eXtension’. Ces extensions peuvent être appelées en
Basic à travers un nom et elles permettent une transmission de
paramètres comme nous l'avons déjà décrite pour l'instruction CALL, Si
nous voulons par exemple écrire une extension graphique qui dessine un
carré sur l'écran, l'appel de cette fonction se présentera ainsi:

10 IQUADRAT, 100, 100, 50

Nous voulons ainsi dessiner un carré dont l'angle supérieur gauche aura
les coordonnées 100, 100 avec un côté d’une longueur de 50 points.

Comme vous voyez, une extension d'instruction est marquée par un trait

-11108-

vertical (SHIFT@) placé devant le mot instruction.

Une telle extension d'instruction peut figurer dans une Rom d'extension,
comme c'est le cas lorsque vous connectez le lecteur de disquette, ou
bien également en Ram. Cela nous donne donc la possibilité d'écrire nos
propres extensions d'instruction. Pour que le système d'exploitation
sache où 11 doit chercher une telle extension, l'extension doit d’abord
etre ‘intégrée’, On emploie pour cela une routine du système
d'exploitation: KL LOG EXT. L'exemple suivant réalise l'instruction
évoquée ci-dessus pour dessiner un carré et montre comment l'intégration
se réalise.

3RSX - EXTENSIONS D’ INSTRUCTION
3L.E. 21/12/84

BCD1 LOGEXT EQU  gBCD1 ; intégrer extension

BBC6 ASKCUR- EQU  8BBC6 ; aller chercher curseur graphique
BBCO MOVABS EQU  &gBBCO ;fixer curseur graphique

BBF9 DRAWRE EQU  &BBF9 ;tracer ligne relativ.

BDC7 CHGSGN EQU  &gBDC7 ;:modifier signe

8000 ORG 8000

8000 010980 LD BC, RSX sadresse de la table
d'instructions RSX

8003 211680 LD HL,KERNAL ;4 octets Ram pour Kernal

8006 C3D1BC JP LOGEXT ; intégrer extension

8009 0E80 RSX DEFWN TABLE ; Adresse des mots d'instruction
800B C31A80 JP QUADRAT

800E 51554144 TABLE DEFM  ”QUADRA”

8014 D4 DEFB "T"+880

8015 00 DEFB O0 ; fin de la table

8016 KERNAL DEFS 4 ; mémoire pour Kernal

801A FEO3 QUADRA CP 3 ; trois paramètres?

801C CO RET NZ

801D CDC6BB CALL  ASKCURS :aller chercher curseur graphique
8020 D5 PUSH DE ; ranger coordonnée X

8021 E5 PUSH HL ; ranger coordonnée Y

8022 DD5605 LD D, (I1X+5)

8025 DD5EO4 LD E,(IX+4) ;coordonnée X

8028 DD6603 LD H, CIX+3)

802B DD6E02 LD L,CIX+2) ;coordonnée Y

-11109-

802E CDCOBB CALL MOVABS ;curseur graphique sur coordonnées
X5 Y

8031 DD5601 LD D, (IX+1)

8034 DD5E00 LD E, (IX) ; longueur dans de comme offset X
8037 D5 PUSH DE ;ranger

8038 210000 LD HL,0 offset Y

803B CDF9BB CALL DRAWREL ;: tracer ligne horizontale
803E E1 POP  HL

803F ES PUSH  HL

8040 CDC7BD CALL CHGSGN ;offset YŸ négatif

8043 ES PUSH HL

8044 110000 LD DE,0

8047 CDF9BB CALL DRAWREL ; tracer ligne verticale
804A D1 POP DE offset X négatif

804B 210000 LD HL,0 soffset Y nul

804E CDF9BB CALL DRAWREL ;:tracer ligne horizontale
8051 E1 POP  HL

8052 110000 LD DE,0

8055 CDF9BB CALL DRAWREL ; tracer ligne verticale
8058 E1 POP HL

8059 D1 POP DE

805A C3COBB JP MOVABS ;rétablir coordonnées

Après que ce programme ait été chargé (comme fichier binaire à partir de
la cassette ou de la disquette) ou qu’il ait été placé en mémoire avec un
programme de chargement de DATA, il doit être initialisé une seule fois.
I1 faut pour cela utiliser l'appel CALL 88000. La nouvelle instruction
est alors disponible. Deux tables sont utilisées pour l'intégration. La
première, appelée RSX dans notre exemple, contient tout d’abord l'adresse
de la seconde table, appelée ici TABLE, suivie des instructions de saut à
l'extension proprement dite. La seconde table contient les noms sous
lesquels les nouvelles instructions peuvent être appelées. Les majuscules
et les points sont autorisés. Le dernier caractère d’un mot instruction
est marqué par son bit 7 qui est mis. La fin de la table est indiquée par
un octet nul. Chaque table doit bien sûr contenir le même nombre
d'entrées, Pour chaque mot d'instruction doit figurer l'adresse de saut
correspondante dans la première table, Sous l'étiquette KERNAL, nous
devons mettre 4 octets à la disposition du système d'exploitation qui
sont utilisés pour la gestion de l'extension. Les 4 octets doivent être
placés entre l'adresse 84000 et l'adresse 8BFFF.

-11110-

La routine de dessin d’un carré commence par l'étiquette QUADRAT
(quadrate en anglais=carré). On contrôle d’abord si trois paramètres ont
bien été transmis. Si ce n’est pas le cas, on quitte la routine
immédiatement, Mais si c'est le cas, on va chercher la position actuelle
du curseur graphique et on la range sur la pile. On va ensuite chercher
dans de et hl tes coordonnées X et Y transmises. La base du bloc de
paramètres se trouve en IX, Après que le curseur graphique ait été fixé
sur ces coordonnées, la routine de dessin d'une ligne relativement à la
position actuelle peut être appelée quatre fois. Pour calculer un offset
négatif, on appelle la routine CHGSGN de l’arithmétique entière, Pour
finir, on rétablit la position originelle du curseur.

Voici un exemple d'utilisation de cette routine:

1OCLS

20FOR i=35 TO 400 STEP 20
30 IQUADRAT, i, 1,30

LONEXT

3.5.3 Le pointeur de variable ‘@'’

Une fonction particulièrement intéressante pour le programmeur en
langage-machine est constituée par le pointeur de variable qui est appelé
avec l'’arobas. Cette fonction renvoie l'adresse où est placée une
variable. L'appel de cette fonction se présente ainsi:

PRINT@a
On sort ainsi l'adresse de la variable a, Si la variable n'avait pas
encore été initialisée, le message d'erreur ‘Improper argument’ sera

sorti.

Si nous voulons maintenant accéder au contenu de la variable, nous
devons distinguer entre les 3 différents types possibles.

La situation est très simple en ce qui concerne les variables entières.

La valeur 16 bits est placée à l'adresse fournie. Nous pouvons donc
obtenir la valeur de la variable a% avec la formule:

-11111-

PRINT PEEK @a%) +256*PEEK(@a%+1 )

Nous pouvons ainsi obtenir des valeurs entre 0 et 65535, Si nous voulons
tenir également compte du signe, nous devons utiliser la fonction UNT.

PRINT UNT(PEEK(@a%) +256*PEEK(@a% +1 ) )

Pour les variables à virgule flottante, le pointeur de variable est
également dirigé sur la valeur de la variable, mais celle-ci est exprimée
avec 5 octets. Les 5 premiers octets sont ce qu’on appelle la mantisse et
le cinquième octet est la puissance de 2 par laquelle doit être
multipliée la mantisse pour obtenir la valeur de la variable. Si nous
désignons les 4 octets de la mantisse par m1 à m4 et l'’exposant par ex,
nous obtenons la valeur à virgule flottante avec la formule suivante:

x=(1-2*SGN(m4 AND 128))*21(ex-129)*
(1+((m4 AND 127)+(m3+(m2+m1/256)/256)/256)/128)

La formule met en évidence que le signe du nombre à virgule flottante se
trouve dans le bit supérieur de m4 et que les octets de la mantisse m1 à
m4 ont des valeurs croissantes, La puissance de 2 contient un offset de
129 ce qui donne des valeurs de 21-129.à 21127. Essayons notre formule:

100a=-13:'variable a virgule flottante examinee
110ad=a : ’adresse de a
120m1=PEEK (ad) :m2=PEEK(ad+1) :m3=PEEK(ad+2)
130m4=PEEK(ad+3) :ex=PEEK(ad+4)
1UYOPRINT(1-2*SGN(m4 AND 128))*21(ex-129)*

(1+(Cm4 AND 127)+(m3+(m2+m1/256)/256)/256)/128)

Si vous faites tourner ce programme, vous obtiendrez en résultat la
valeur -13, Remplacez si vous le voulez la ligne 100 par INPUT a et vous
pourrez tester n'importe quelles valeurs.

La fonction de pointeur de variable trouve son application dans
l'instruction CALL qui ne peut en effet transmettre que des valeurs 16
bits. Si vous voulez donc travailler avec des valeurs à virgule
flottante, vous pouvez transmettre avec ‘@’ l'adresse d’une variable à
virgule flottante. Vous pourrez ensuite vous référer à cette adresse.
Cette méthode permet également bien sûr de modifier directement la valeur

-11112-

d’une variable à virgule flottante.

Le cas des variables alphanumériques est encore plus intéressant, Ici
aussi, nous pouvons utiliser le pointeur de variable qui nous renvoie
l'adresse de la variable. Ce n'est cependant pas directement l'adresse de
la chaîne de caractères mais celle de ce qu’on appelle le descripteur de
chaîne. Ce descripteur de chaîne est long de trois octets. Le premier
octet contient la longueur de la chaîne, soit une valeur entre C et 255.
Les deux octets suivants contiennent l’adresse de la chaîne.

100INPUT a$

110ad-ea$

1201=PEEK(ad)

130sa=PEEK(ad+1)+256*PEEK(ad+2)

14OFOR i=sa TO sa+I-1:PRINT CHR$(PEEK(I) )3 :NEXT

Ce programme va chercher la longueur et l'adresse de la chaîne, la lit et
la sort.

Ici aussi, il est possible de transmettre une chaîne à l'instruction CALL
à travers le pointeur de variable.

Les chaînes peuvent être encore employée en liaison avec l'instruction
CALL de façon tout à fait différente, On peut par exemple placer tout
simplement un programme en langage-machine dans une chaîne et l'appeler
avec l'instruction CALL et le pointur de variable. Le programme en
langage-machine doit pour cela être transposable (11 ne doit pas contenir
d'adresse absolue interne) et 11 ne doit pas comprter plus de 255
octets. La plupart des petits programmes utilitaires remplissent ces
conditions. Si vous voulez utiliser cette méthode, il vous faut procéder
ainsi:

Le programme en langage-machine est d'abord placé dans la variable
alphanumér ique. On utilisera le plus souvent READ et DATA à cet effet. Si
vous voulez ensuite faire exécuter le programme, il vous suffit de faire
calculer l'adresse de début de la chaîne de caractères (et donc du
programme) avec l’arobas.

-11113-

3,6 Le listing de la Rom Basic
3.6,1 L'arithmétique à virgule flottante

Toutes les fonctions arithmétiques qu'utilise l’interpréteur Basic se
trouvent dans la Rom du système d'exploitation. Elles sont appelées à
travers une table de saut placée en &BD3D à &BDC7. Si vous voulez
modifier les routines arithmétiques, il vous suffit d'insérer à
l'emplacement voulu un saut à cette routine.

Nous allons vous montrer comme exemple d'application des routines avec
virgule flottante une routine de calcul de la racine carrée d’un nombre,
L'interpréteur Basic du CPC nous fournit certes déjà cette fonction mais
nous voulons démontrer que celle-ci peut être encore améliorée par
l'emploi d'algorithmes plus puissants.

La fonction SGR intégrée travaille d'après le même algorithme que le
calcul de la puissance.

SOR(X)=EXP(LOG(X)*0.5)

I1 faut donc calculer chaque fois les fonction exponentielle et
logarithme ce qui s'effectue à travers des calculs de polynômes
compliqués et longs. La racine carré peut cependant être calculée
simplement à travers un processus d'itération.

XEN+T)=CXCN)+A/XCN) )/2

où A est le nombre dont la racine doit être extraite, X(N) est l'ancienne
et X(N+1) la nouvelle valeur approchée. Comme valeur de départ, on peut
prendre le nombre À lui-même. On obtient une meilleure valeur approchée
lorsqu'on divise par deux la puissance de deux du nombre à virgule
flottante. Le résultat ne se modifie plus ensuite, après 4 itérations,
dans le cadre de la précision de calcul. Notez également que la division
par deux n'a pas été réalisée avec une division à virgule flottante qui
prend beaucoup de temps. On a simplement décrémenté de 1 la puissance de
deux, Le gain de temps dû à ce procédé est significatif. La routine SGR
de l'interpréteur met en effet 27 millisecondes, alors que notre routine
exécute la même tâche en 8 millisecondes. Elle est donc plus de trois
fois plus rapide.

-11114-

AB00
BD70
BD64
BD58

ABOO
ABO3
ABO4
ABO5
ABO8
ABOA
ABOB

ABOC
ABOD
AB10
AB13
AB15

AB16
AB17
AB19

AB1C
ABIE
AB1F
AB20
AB22

AB25
AB27
AB28
AB29
AB2C
AB2F
AB31
AB32
AB33
AB36

CD/70OBD
3F

c8
F20CAB
3E01
B7

C9

E5
1153AB
010500
EDBO
E1

ES
DDE1
DD/EO4

D681
3F

1F
C601
DD7704

0604
C5

E5
1158AB
010500
EDBO
E1

E5
1153AB
EB

3 ROUTINE SQR RAPIDE
3L.E. 18/12/84

SGN
DIV
ADD

NEWSGR

GOON

ITER

ORG
EQU
EQU
EQU

CALL
CCF
RET
JP
LD
OR
RET

PUSH
LD
LD
LDIR
POP

PUSH
POP

LD
SUB
CCF
RRA
ADD
LD

LD
PUSH
PUSH
LD
LD
LDIR
POP
PUSH
LD
EX

&ABOO
&BD70
&BD64
&BD58

SGN :examiner signe

Z ;zéro, déjà terminé

P, GOON

À,1 : "IMPROPER ARGUMENT"
À

HL

DE, STORE

BC,5

:ranger radicande
HL

HL
IX

A, (IX+4) ;puissance
&81 ;:normaliser

sdiviser puissance par deux
A,1
(IX+4),A ;comme valeur de départ

B,4 ;:4 itérations

BC

HU

DE, STORE2

BC,5

ranger valeur approchée
HL

HL

DE, STORE1

DE,HL

-11115-

AB37 010500 LD BC,5

AB3A EDBO LDIR aller chercher radicande
AB3C E1 POP HL

AB3D 1158AB LD DE, STORE2

AB4O CD64BD CALL DIV

AB43 1158AB LD DE, STORE2

AB46 CDS58BD CALL ADD

AB49 ES PUSH HL

ABUA DDE1 POP IX

ABUC DD3504 DEC  (IX+4) snombre/2
ABUF C1 POP BC

AB50O 10D5 DJNZ ITER

AB52 C9 RET

AB53 STORE1 DEFS 5

AB58 STORE2 DEFS 5

Mais comment faire pour que l’interpréteur utilise la nouvelle routine?
C'est le vecteur 8BD79 qui sert pour la fonction SGR. Il faut donc placer
en cet endroit un saut à notre routine:

JP &AB00

Lorsque la routine est appelée en Basic, le registre HL doit être pointé
sur la valeur à virgule flottante, Après exécution de la routine, le
registre HL doit être pointé sur le résultat. Normalement la valeur de
registre ne doit pas avoir été modifiée. Les flags indiquent l'état des
erreurs de la fonction:

Etat des erreurs de la fonction:

C=1 exécution correcte

C=0 & Z=1 ‘Division by zero”

C=0 & N1 ‘Overflow’

C=0 g Z=0 ’Improper argument”

Vous trouverez dans les pages suivantes le listing de l'arithmétique à
virgule flottante. Chaque routine contient également l'adresse de la
table de saut à travers laquelle elle est appelée par l'interpréteur
Basic. Vous trouverez ensuite au chapitre 3.6.3 l'’arithmétique entière

-11116-

qui est utilisée par l’interpréteur chaque fois que c'est possible. En
effet comme elle ne travaille qu'avec des valeurs sur deux octets, cette
arithmétique est toujours nettement plus rapide que le calcul avec des
nombres à virgule flottante. Servez-vous également de ce fait dans vos
programmes et utilisez autant que possible des variables entières. Cela
vaut notamment pour les boucles FOR-NEXT (voyez également à ce sujet le
chapitre 3.2).

-11117-

ARITHMETIQUE À VIRGULE FLOTTANTE

ns ses sssssssssssss BD3D copier variable de (de) dans (h1)

2E18 E5 push hl

2E19 DS push de

2E1A C5 push bc

2E1B EB ex  dejhl

2E1C 010500 id bc,0005 copier
2E1F EDB0 Idir 5 octets
2E21 EB ex dehl

2E22 2B dec hl

2E23 7E Id a,(hl) a=exposant
2E24 C1 pop bc

2E25 Di pop de

2E26 El pop hl

2E27 37 scf

2E28 C9 ret
Sosssssssssessssssssssssss BD40 convertir entier en virgule flottante
2E29 D5 push de

2E2A C5 push bc

2E2B F67F or 7F

2E2D 47 id ba

2E2E AF xor a

2E2F 12 Id (de),a

2E30 13 inc de

2E31 12 Id (de),a

2E32 13 inc de

2E33 0E90 id c,90 exposant, 2115
2E35 7C Id ah

2E36 B7 of a

2E37 2008 jr n2,2E41

2E39 4F id ca

2E3A 65 ld h}

2E3B 6F ld la

2E3C B4 or h

2E3D 2800 j z,2E4C

2E3F 0E88 id c,88 exposant, 217
2E41 FA4B2E jp m,2E4B

2E44 29 add hihl

2E45 0D dec c

2E46 B4 or h

2E47 F2442E jp p,2E44

2E4A 7C Id ah

2E4B A0 and D

2E4C EB ex de,hl

2E4D 73 ld Mh,e

2E4E 23 inc  hNl

2E4F 77 Wd {hi},a

2E50 23 inc  hl

2E51 71 Id Mc

2E52 C1 pop bc

2E53 Ei pop hl

2E54 C9 ret

-11 118-

ARITHMETIQUE A VIRGULE FLOTTANTE

ssssssssessssssmsws#s BD43 convertir valeur 4 octets en virgule flott.

2E55 C5 push be

2E56 0100A0 kb bc,A000 exposant, 2131
2E59 CD602E cal  2E60 convertir

2E5C C1 pop bc

265D C9 ret

“ssssssssessssssssss BD94 convertir valeur 4 octets en virgule flott.
2E5E O06AB id b,AB exposant, 2139
2E60 D5 push de

2E61 CDA136 call  36A1 conversion

2E64 Di pop de

2E65 C9 ret

nn essssssssss ss: BD46 virgule flottante => entier
2E66 E5 push hl

2E67 DDE1 pop  ix

2E69 AF xor a

2E6A DD9604 sub  (ix+04) exposant

2E6D 281B j 2,2E8A nombre égal zéro?
2E6F C690 add  a,90

2E71 DO ret nc

2E72 D5 push de

2E73 C5 push bc

2E74 C610 add  a,10

2E76 CD3D36 call  363D

2E79 CB21 sla c

2E7B ED5A adc hide

2E7D 2808 j 2,2E87

2E7F DD7E03 Id a,(ix+03) signe de la mantisse
2E82 B7 of a

2E83 3F ccf

2E84 C1 pop be

2E85 Di pop de

2E86 C9 ret

2E87 9F sbc aa

2E88 18F9 ï 2E83

2E8A 6F ld La

2E8B 67 id ha zéro dans hl
2E8C 37 scf

2E8D C9 ret

Msn nn ss sssss es: BD49 virgule flottante => entier
2E8E CDA12E call  2EA1 FIX

2E91 DO ret nc

2E92 F0 ret p

2E93 E5 push hl

2E94 79 ld a,c

2E95 34 inc fl)

2E96 2006 j n2,2Ë9E

2E98 23 inc hl

2E99 30 dec a

2E9A 20F9 j nz,2E95

-11 119-

ARITHMETIQUE A VIRGULE FLOTTANTE

2E9C 34 inc  (h)

2E9D 0C inc  c

2EJÆ F1 pop  h

2E9F 37 scf

2EA0 C9 ret

OU msn eseossusssesss BD4C FIX
2EA1 E5 push

2EA2 DS push de

2EA3 E5 push hi

2EA4 DDE1 pop ix

2EA6 CD0436 call 3604 fonction FIX
2EA9 D1 pop de

2EM Eï pop  hl

2EAB C9 ret

OR NU 0 es meesssseseeusssse BD4F INT
2EAC CDA12E call  2EA1 FIX

2EAF D0 re nc

2E80 C8 ret z

2EB1 CB78 bit 7,b

2EB3 C8 re z

2EB4 18DD jr 2E93

LLILERELEZLENSELEEEEELEEEELEEELREEE ELITE EEE TEEESSIIEEIE "7
2EB6 CDE835 cal 35E8

2EB9 47 Wd ba

2EBA 2852 jr z,F0E

2EBC FCFB35 call m,35FB négatif,, alors inversion de signe
2EBF E5 push h!

2EC0 DD7E04 Wd a,(ix+04) normaliser
2EC3 D680 sub 80 exposant
2ECS 5F Wd ea

2EC8 9F sbc aa

2EC7 57 d da

2EC8 6B Id Le

2EC9 62 d hd

2ECA 29 add hihl

2ECB 29 add hihl

2ECC 29 add hih

2ECD 19 add hide

2ECE 29 add hihi fois 77
2ECF 19 add hide

2ED0 29 add hhl

2ED1 29 add hihl

2ED2 19 add hlde

2ED3 7C id ah

2ED4 D609 sub 09

2ED6 5F W ea

2E07 E1 pop hl

2ED8 C5 push bc

2ED9 D5 push de

2EDA C41F2F cal n2,2F1F multiplier nombre par 10fa
2ED0 FD21132F Id iy,2F13 3124999.98
2EE1 CDAO35 call  35A0 comparer

-11 120-

ARITHMETIQUE A VIRGULE FLOTTANTE

2EE4 281B k 2,2F01 égal?

2EE6 3008 l'A nc,2EFO0 supérieur?
2EE8 CD1234 cal 3412 multiplication par 10
2EEB D1 pop de

2EEC 1D dec €

2EED D5 push de

2EEE 18ED k 2EDD

2EFO FD21182F Wd iy,2F18 1E9

2EF4 CDA035 cal  35A0 comparer
2EF7 3808 ÿ c2F01 inférieur?
2EF9 CD9B34 call  349B division par 10
2EFC D1 pop de

2EFD 1C n  e

2EFE DS push de

2EFF 18EF y 2EF0

2F01 CDS8E2E call  2E8E

2F04 79 \d ac

2F05 D1 pop de

2F06 C1 pop bc

2F07 4F d ca

2F08 3D dec a

2F09 85 add a!

2FOA 6F Wd la

2F0B D0 rt nc

2F0C 24 inch

2F0D C9 ret

2F0E 5F Id ea

FOF 77 id fa

2F10 0E01 Wd c01

2F12 C9 ret

CORTE LELELIITTTTLIIEETI TILL LTITILETEEEEELEELILEEELLLELLELLELE,) LE)
2F13 F0 1F BC SE 9% 3124999.98
2F18 FE 27 6B 6E %ŒÆ 1E9

ON Sn e ns nsssssesessesss BD55 multiplier nombre par 10fa
2F1D 2F cp a

2FIE 3C inc a

2F1F B7 of a

2F20 37 scf

2F21 C8 ret Z

2F22 4F d ca

2F23 F2282F P p,2F28

2F26 2F cp a

2F27 3C inc a

2F28 COSE2F cal 2F3ÆE

2F2B 2809 ÿ 2,2F36

2F2D CS push bc

0F2E F5 push af

2F2F CD362F call  2F36

2F32 Fi pop af

2F3 C1 pop bc

-11 121-

ARITHMETIQUE A VIRGULE FLOTTANTE

2F34 18F2 x 2F28

2F36 79 ld ac

2F37 B7 of a

2F38 F29E34 jp P,349E division

2F3B C31534 jp 3415 multiplication
QFSE 118F2F Wd de,2F8F 1£13

2F41 D60D sub  OD -13

2F43 D0 ret nc supérieur égal?
2F44 C60C add a,0C +12

2F46 5F ld ea

2F47 87 add aa

2F48 67 add aa fois 5

2F49 83 add ae

2F4A C653 add a,53

2F4C 5F Id ea 2F53, puissances de 10
2F4D CE2F adc  a,2F

2F4F 93 sub €

2F50 57 Wd da

2F51 AF xor a

2F52 C9 ret

UN nn nes sssusessssssss: constantes à virgule flottante
2F53 00 00 00 20 84 10

2F58 00 00 00 48 87 100

2F5D 00 00 00 7A 8A 4000

2F62 00 00 40 1C 8&E 10000

2F67 00 00 50 43 91 100000

2F6C 00 00 24 74 94 1000000

2F71 00 80 96 18 98 10000000

2F76 00 20 BC 3E 9B 100000000

2F7B 00 28 6B 6E 9E 1E9

2F80 00 F9 02 15 A2 1E10

2F85 40 B7 43 3A A5 1En1

2F8A 10 B5 D4 68 AB 1E12

2F8F 2A E7 84 11 AC 1E13

DONS nn nn nee ss sesssssssessueesssses BD97 RNDInit
2F94 216589 Id hi,8965

2F97 22E6B8 id (B8E6),hl

2F9A 21076C Id hi,6C07

2F9D 22E4B8 ld (B8E4)hi

2FAD C9 ret

ROM RON ns eesesesseseuseueessess BDJA Random Seed
2FA1 EB ex  dehl

2FA2 CD942F cal  2F94 RNDInit

2FAS EB ex dehl

2FA6 CDEB35 cal  35E8 SGN

2FA9 C8 ret  2z

2FAA 11E4B8 id de,B8E4 pointeur sur mantisse RND
2FAD 0604 id b,04 4 octets

2FAF A d a,(de)

2FB0 AE xor (nl créer nouvelle mantisse

-11 122-

ARITHMETIQUE A VIRGULE FLOTTANTE

oFB1 12 0] (de),a
2FB2 13 inc de

2FB3 23 inc hl

2FB4 10F9 dnz 2FAF octet suivant
2FB6 C9 ret
RON neo sssseseseseseseesss BD9D RND
2FB7 E5 push hl

2FB8 2AE6B8 id hl,(B8E6)
2FBB 01076C Wd bc,6C07
2FBE COFA2F cal  2FFA
2FC1 E5 push hl

2FC2 2AE4B8 d hl,(B8E4)
2FC5 016589 W bc,8965
2FC8 COFA2F cal  2FFA
2FCB D5 push de

2FCC E5 push hl

2FCD 2AE6B8 ld hl,(B8E6)
2FD0 CDFA2F cal  2FFA
èFD3 E3 ex (sp}hl
2FD4 09 add hlbe
2FD5 22E488 Wd (B8E4)hl
2FD8 E1 pop h

2FD9 01076C W bc,6C07
2FDC ED4A adc hlbc
eFDE C1 pop bc

FDF 09 add hlbc
èFEO C1 pop bc

èFE1 09 add hlbc
èFE2 22E6B8 ld (B8E6),hl
èFE5 Ei pop hl

sonne ssssessssssssss BDAO amener dernière valeur RND

0FE6 E5 push

2FE7 ODE1 pop à

2FE9 2AF4B8 W hl,(88E4)
2FEC ED5BE6B8 W de,(B8E6)
2FFO 010000 Id bc,0000
2FF4 DD360480 d (x+04),80 exposant
2FF7 C3B136 j 36B1
2FFA EB ex dehl
2FFB 210000 Wd hi,0000
FFE 3E11 W a,11
3000 3D dec a

3001 C8 ret 2

3002 29 add hihi
3003 CB13 f e

3005 CB12 d d

3007 30F7 ï  nc3000
3009 09 add hibc
S00A 30F4 j nc,3000
300€ 13 inc de
3000 18F1 j 3000

-11 123-

ARITHMETIQUE A VIRGULE FLOTTANTE

DR O nn usenuueeseueenesess BD82 LOG10

300F 118830 id de,308B LOG10{2)

3012 1803 x 3017
RSR nee untenenesessssss BD7F LOG
3014 118630 Id de,3086 LOG(2)

3017 CDE835 cal  35EB SGN

301A 3D dec a

301B FEO1 cp 01

301D DO ret nc

S01E D5 push de

301F CD6C35 cal  356C tester exposant

3022 F5 push af

3023 DD360480 Wd (ix+04),80 exposant, nombre 0.5 à 1

3027 118130 ld de,3081 1/SQR(2)

302A CD9A35 call  359A comparer

3020 3006 ÿr nc,3035 supérieur?

302F DD3404 inc (ix+04) augmenter exposant, nombre doublé
3032 F1 pop af

3033 3D dec a

3034 F5 push af

3035 CD1633 cal 3316 stockage provisoire du résultat
3038 D5 push de

3039 113233 W de,3332 1

303C CD3F33 call  333F Addition

303F EB ex  dehl

3040 Ei pp  h

9041 D5 push de

3042 113233 ld de,3332 1

3045 CD3733 call 3337 soustraction

3048 Di pop de

3049 CD9E34 call 34 division

304C CDA932 cal  32A9 calcul de polynôme

sense esessessssssssss constantes à virgule flottante pour LOG
904F 04 degré de polynôme

3050 4C 4B 57 5€ 7F 0.434259751

3055 0D 08 9B 13 80 0.576584342

805A 23 93 38 76 80 0.961800762

305F 20 3B AA 38 82 2.88539007

CPTES TITI TT LIT T TSI TT TS SLT STE LI STI T TITLE TT LEI ELLE LLEELLE
3064 D5 push

3065 CD1534 cal 3415 multiplication

3068 D1 pop de

3069 E3 ex (sp}hl

306A 7C W ah

3068 B7 of a

306C F27130 jp p,3071

306F 2F cpl a

3070 3C inc a

3071 6F Wd la

3072 7C ld ah

3073 2600 Wd h,00

3075 CD292E cal  2E29 convertir entier en virgule flottante

-11 124-

ARITHMETIQUE A VIRGULE FLOTTANTE

3078 EB ex de,hl

3079 El pop hl

307A CD3F33 cal  333F Addition

307D D1 pop de

307E C31534 Le) 3415 multiplication
CRXEEETETTIEEEELSIEELLILILELLLLL LL IL LLLELLLLLEEELL ELELLLLLLEEEZX]
3081 34 F3 04 35 80 .707106781 1/SQR(2)

3086 F8 17 72 31 80 .693147181 LOG(2)

3088 85 JA 20 1A 7F .301029996 LOG10(2)

RO 0 sense sessessosessese BDB5 EXP
3090 O6E1 ld bE1

3092 CD0733 call 3307 comparer exposant

3095 D22833 jp nc,3328 1 comme résultat

3098 110031 ld de,3100 LOG(plus grand nombre représentable)
3098 CD9A35 cal  359A comparer

309E F2EC36 jp p.36EC supérieur, alors dépassement

30A1 110531 Wd de,3105 LOG(plus petit nomb

M cal 354 RATE P re représentable)
30A7 FAE636 jp m,36E6 inférieur, alors dépassement par le bas
30AA 11FB30 Id  de,30FB 1/L0G(2) ’
30AD CDD432 cal 3204 zéro
3080 7B W ae

3081 F2B630 je) p,3086

30B4 ED44 neg a

3086 F5 push af

3087 CD1D33 cal  331D multiplier

30BA CDOF33 call  330F stockage provisoire variable

3080 D5 push de

30BE CDAC32 cal  32AC calcul de polynôme

ons sssenessessesssss#si constantes à virgule flottante pour EXP
30C1 03 degré de polynôme

30C2 F4 32 EB OF 73 6.86256€-5

30C7 08 B8 D5 52 78 2.57367E-2

30CC 00 00 00 00 80 0.5

COPIE TE LITE TEE TEE TITI T EST ETS ET SSII ETES TETE STE EST EI TITIEE)
3001 E3 ex ),
3002 CDAC32 cal 32AC calcul de polynôme

se sssesssesssssssssessconstantes à virgule flottante pour EXP
degré de polynôme

30D6 09 60 DE 01 78 1.98184E-3

3008 F8 17 72 31 7E 0.173286795

CPTLEPLTTE STE CTT TS S SIT IST TS TEST I TES SSI ST STI TESTS TT TEST
30E0 CD1534 cal 3415 multiplication

30E3 D1 pop de

30E4 E5 push hl

30E5 EB ex deh

30E6 CD373 cal 3337 soustraction

90E9 EB ex de,hl

JEA Ei pop hl

-11 125-

ARITHMETIQUE A VIRGULE FLOTTANTE

30EE 11CC30 Id de,30CC 0.5

30F1 CD3F33 call  333F Addition

30F4 DD3404 inc (ix+04) augmenter exposant, nombre doublé
30F7 Fi pop af

30F8 C37B35 jp 357B multiplier nombre par 2TA

CPRREE TEST TESTS TESTS TT ST
30FB 29 3B AA 38 81 1.44269504 110G(2)

3100 C7 33 OF 30 87 88.0269919 LOG(plus grand nombre)
3105 F8 17 72 B1 87 -88.7228391 LOG(plus petit nombre)
DR nn ones sous senmesseessosensss se BD79 SOR
310A 11CC30 d de,30CC 0.5

ons ssssssssssss BD7C élévation à la puissance
3100 EB ex de,hl

310E CDE835 cal  35E8 SGN, signe de l'exposant

3111 EB ex dehl

3112 CA2833 jp 2,3328 zéro, alors 1 comme résultat

3115 F5 push af

3116 CDE835 call  35E8 SGN, signe de la base

3119 2825 jr z,3140

311B 47 Wd ba

311C FCFB35 cal m,35FB négatif, alors changer signe

A11F E5 push hl

3120 CD8231 call 3182

3123 E1 pop hl

3124 3825 ÿ c,314B

3126 E3 ex  (spjhl

3127 E1 pop h

3128 FA4831 jp m,3148

312B C5 push bc

312C D5 push de

312D CD1430 cal 3014 LOG

3130 D1 pop de

3131 DC1534 cal  c,3415 multiplication

3134 DC9030 cal  c,3090 EXP

3137 C1 pop bc

3138 DO ret nc

3139 78 d ab

313A B7 of a

313B FCFB35 call  m,35FB

313€ 37 scf

313F C9 ret

3140 F1 pop af

3141 37 scf

3142 FO ret p

3143 CDEC36 cal  36EC dépassement

3146 AF xof a

3147 C9 ret

3148 AF xor a

3149 3C inc a

314A C9 ret

-11 126-

ARITHMETIQUE A VIRGULE FLOTTANTE

3148 4F Wd ca

314C F1 pop af

314) C5 push be

314E F5 push af

314F 79 Yd ac

3150 37 scf

3151 8F adc aa

3152 30FD ÿ nc,3151

3154 47 d ba Fat .
3155 CDO0F33 call 330F stockage provisoire variable
3158 EB ex del

3159 78 d ab

315A 687 add aa

3158 2815 ÿ 23172

3150 F5 push af

315E CD1D33 cal  331D multiplier par résultat intermédiaire
3161 3016 jr nc.3179

3163 F1 pop af

3164 30F4 ÿ nc,315A

3166 F5 push af

3167 11E8B8 Wd de,B8E8

316A CD1534 cal 3415 multiplication
316D 3004 ÿ nc,3179

316F F1 pop af

3170 18E8 y 315A

3172 F1 pop af

3173 37 scf

3174 FCFD32 call m,32FD former complément
3177 18BE ÿr 3137

3179 F1 pop af

317A F1 pop af

3178 C1 pop bc

317C FAE636 pr) m,36E6 dépassement par le bas, zéro
317F C3EE36 jP 36EE dépassement

3182 C5 push bc

3183 CD1733 cal 3317 aller chercher résultat intermédiaire
3186 CDA12E cal  2EAÏ FIX

3189 79 Id ac

318A C1 pop bc

318B 3002 j nc,318F

318D 2803 ÿ 2,9192

318F 78 Wd ab

3190 87 or a

3191 C9 ret

3192 4F id ca

3199 7E id ah)

3194 1F rra

3195 9F sbc aa

3196 A0 and b

3197 47 id ba

3198 79 id ac

-11 127-

ARITHMETIQUE A VIRGULE FLOTTANTE

3199 FE02 cp  ®
3198 9F sbc aa
319C D0 rt nc
3190 7E Wd a,fh)
319E FE27 œp 27
3140 D8 rt oc
31A1 AF xor a
31A2 C9 ret

DO ssssesssssssesesssse BD76 PI
31A3 11A931 Wd de,31A9 7

31A6 C3182E jp 2E18 aller chercher variable

CCPLTETTE TT ST S TS TT S TITI T SITES TESTS TT T TITI E TITI IT TII TITLES)
41A9 A2 DA OF 49 82 3.14159265 #

OO nn nes sssesusesesssesses BD73 DEG/RAD
S1AE 32F7B8 Wd (B8F7),a

31B1 C9 ret

Unes esesssssessess BDBB COS
3182 CDE835 cal  35E8

3185 FCFB35 cal  m,35FB négatif, alors changer signe
31B8 F601 of 01
31BA 1801 j 3180

D 0 nes se messssesesesessse BDS8 SIN
31BC AF xof a

3180 F5 push af

31BE 111032 KW de,321D 1%

31C1 06F0 d b,F0

31C3 3AF7B8 Id a,(B8F7) DEG?

31C6 B7 of a

3107 2805 ï Z31CE

3109 112232 Wd de,3222 1/180

31CC 06F6 d bF6

31CE CD0733 cal 3307 comparer exposant

3101 3034 ÿ nc, 3200

31D3 F1 pop af

3104 CDD532 cal  32D5

3107 D0 rt nc

31D8 7B Ce] 4,6

31D9 1F ra

31DA DCFB35 cal  c,35FB changer signe

31DD 06E8 W b,E8

31DF CD0733 cal 3307 comparer exposant

31E2 D2E636 jp nc,36E6 dépassement par le bas, zéro
31E5 DD3404 inc (ix+04) augmenter exposant, nombre doublé
91E8 CDA932 cal 32A9 calcul de polynôme
sonne sesesessssssses constantes à virgule flottante pour SIN
31E8 06 degré de polynôme

31EC 1B 20 1A E6 6E -3.42879E-6

31F1 F8 FB 07-28 74 1.60247E-4

31F6 01 89 68 99 79 —4.68165E-3

-11 128-

ARITHMETIQUE À VIRGULE FLOTTANTE

31FB E1 DF 35 23 70 7.96926€-2

3200 28 E7 5D A5 80 —0.645964095

3205 A2 DA OF 49 81 1.57079633 7/2

COPIE N ECTS TSS ITS TESTS TS TITLE TT ST ITS SSI TSI S ETS TSI IITS
320A C31534 jp multiplication

320D F1 pop af

920E C22833 ÿ n2,3328 SIN?, alors 1 comme résultat

3211 3AF7B8 Wd a.(B8F7)

3214 FEO1 cp 01 DEG?

3216 D8 ret c non, terminé

3217 112732 W de,3227 par pi/180

321A C31534 jp 3415 multiplier

CPPTEIEIENE ETES TS TS SIT T SSI SSSSSSST ST TT ST TS
3210 6E 83 F9 22 7F 0.318309886  1/

3222 B6 60 0B 36 79 5.55556E-3  1/180

3227 13 35 FA 0E 7B 1.74533E-2  3/180

322C D3 EO0 2E 65 86 57.2957795  180/7

OO nn nn none esse sssseusese BDBE TAN
3231 CD0F33 cal  330F stockage provisoire nombre

3234 D5 push de

3235 CDB231 cal  31B2 cos

3238 E3 ex (sp)hl

3239 DCBC31 cal  c,31BC SIN

323C D1 pop de

323D DA9E34 jp c,349E Division

3240 C9 ret

OO nn nee esnnmessssssses se BD91 ATN
3241 CDE835 cal  35E8
3244 F5 push af

3245 FCFB35 cal m,35FB négatif, alors changement de signe
3248 06FO0 id b,FO

324A CD0733 call 3307 Comparer exposant

3240 304A j  nc329

324F 3D dec a

9250 F5 push af

3251 F4FD32 call  p,32FD former complément

3254 CDA932 call  32A9 calcul de polynôme

tonnes esssesesssssss constantes à virgule flottante pour ATN
3257 0B degré de polynôme

3258 FF C1 03 OF 77 1.09112E-3

325D 83 FC E8 EB 79 -7.19941E-2

3262 6F CA 78 36 7B 2.227T44E-2

3267 D5 3E B0 B5 7C -4.43575E-2

326C B0 C1 8B 09 7D 6.71611E-2

3271 AF E8 32 B4 7D -8.79877E-2

3276 74 6C 65 62 7D 0.110545013

327B D1 F5 37 92 7E —0.142791596

3280 7A C3 CB 4C 7E 0.199996046

3285 83 A7 AA AA 7F -0.333333239

-11 129-

ARITHMETIQUE A VIRGULE FLOTTANTE

328A FE FF FF FF 7F 0.5

ÉPTTTT TS T SSII SSI TSI TITI SLIST TILL TI TTL SSL LLLL) LL) LL), ),);),
328F CD1534 call 3415 multiplication

9292 F1 pop af

3293 110532 Id de,3205 af2

3296 F43B33 cal  p,333B soustraction

3299 3AF7B8 ld a,(B8F7) DEG ?

329C B7 of a

329D 112032 d de,322C 180/7 .

32A0 C41534 cal  nz,3415 si DEG, alors multiplier

32A3 F1 pop af |
32M4  FCFB35 cal  m,35FB négatif, alors changer signe
32A7 37 scf

32AB C9 ret

ROOMS RMS esse ssesssesss calcul de polynôme
32A9 CD1D33 cal  391D multiplier

32AC CD1633 cal 3316 stockage provisoire variable
32AF EB ex de,hl

32B0 Di pop de

3281 1A W a,(de) aller chercher degré polynôme
32B2 13 inc de

32B3 47 W ba dans b

32B4 CD182E call  2E18 aller chercher variable

32B7 13 inc de

32B8 13 inc de

3289 13 inc de plus 5, prochain coefficient
32BA 13 inc de

32BB 13 inc de

32BC D5 push de

32BD 11EDB8 W de,B8ED stockage provisoire

32C0 05 dec b prochain coefficient

32C1 C8 ret 2

32C2 C5 push bc

32C3 11F2B8 Id de,B8F2 stockage provisoire

32C6 CD1534 call 3415 multiplication

32C9 C1 pop bc

32CA D1 pop de

32CB D5 push de

32CC C5 push bc

32CD CD3F33 call  333F Addition

32D0 C1 pop bc

32D1 Di pop de

32D2 18E3 ÿ 32B7

CÉTTTISI STILL SI ST ST SIT RTS SIT T TT TT TTL LITTLE LLLLLILILLIL LL LL LEE)
32D4 AF xor a

32D5 F5 push af

32D6 CD1534 call 3415 multiplication

32D9 F1 pop af

32DA 11CC30 ld de,30CC 0.5

32DD C43F33 call  nz,333F Addition

32E0 E5 push hl

92E1 CD662E cal 2E66 virgule flottante à entier

-11 130-

ARITHMETIQUE À VIRGULE FLOTTANTE

32E4 3013 j nc,32F9

32E6 D1 pop de

32E7 E5 push h

32E8 F5 push af

32E9 D5 push de

32EA 11EDB8 Id de,B8ED

32ED CD292E call  2E29 convertir entier en virgule flottante
32F0 EB ex de,hl

32F1 E1 pop hl

32F2 CD3733 call 3337 soustraction

32F5 F1 pop af

32F6 Di pop de

32F7 37 scf

32F8 C9 ret

32F9 E1 pop hl

32FA AF xor a

32FB 3C inc a

32FC C9 ret

MR OMR ne sens esesesssssess former complément
32FD CD1633 cal 3316 stockage provisoire de variable
3300 EB ex del

3301 CD2833 cal 3328 aller chercher 1

3304 C39E34 jp 349E Division
RON ss COMpPArTET exposant
3307 CD6C35 cal  356C

330A F0 rt pp

330B B8 cp b

330C C8 ret 2

3300 3F ccf

33%0E C9 ret

etes ssssesssssesssssstockage provisoire de variable
330F EB ex de,hl

3310 21E8B8 Wd hi,B8E8 adresse objet

3313 C3182E jp 2E18 copier variable

MO sem ssseusssssss+ stockage provisoire de variable
3316 EB ex de,hl

3317 21F2B8 id hi,B8F2

331A C3182E jp 2E18 aller chercher variable
CÉLELERSEEE SIT TEE T EE TT TT TT TE TE ET TS TT TT TT ET TS TT TT ST TT)
%1D EB ex de,hl

931E 21EDB8 Wd hi,B8ED

9321 CD182E cal  2F18 aller chercher variable

9324 EB ex de,hl

39325 C31534 jp 3415 multiplication

Ne nes esssssssssssss aller chercher constante 1
3328 D5 push de

3329 113233 Id de,3332 1

332C CD182E cal  2E18 aller chercher variable

-11 131-

ARITHMETIQUE A VIRGULE FLOTTANTE

332F D! pop de
3330 37 scf
3931 C9 ret

ÉPLTTTLLETTSSSI TITI SI IST LI ILES SSII) LLLEEE EE.
3332 00 00 00 00 81

sonne sesssesseseessss ss BD5B soustraction (h1):=(h1)-(de)

3397 3E01 id a,01
3339 1805 ÿ 3340

sus sussesssssesseses BD5SE soustraction (h1):=(h1)-(de)
333B 3E80 W a,80

333D 1801 ï 3340

RON nn ssnenesenessssssesesses BD58 Addition (hi}:= (h}) +(de)
333F AF xor a annuler carry
3340 E5 push hl

3341 DDE1 pop x

3343 D5 push de

3344 FDEi pop jy

3346 DD4603 Hd  biix+03) signe premier opérande
3349 FD4E03 id c:(fy+03) signe second opérande
334C B7 of a

3340 2808 ï 2,335A

334F FA5833 j m,3358

3352 3E80 Wd a,80

3354 A9 xor  C

3355 4F d ca

3356 1802 ÿ 335A

3358 A8 xor  b

3359 47 Id ba

335A DD7E04 Wd a.(ù+04) exposants
335D FDBE04 cp (y#04) comparer

3360 3014 ÿ nc,3376 ,

3362 50 d db

3363 41 d bc

3364 4A d cd

3365 67 or a

3366 57 Wd da

3367 FD7E04 W a.(y+04) exposant

336A DD7704 Le] (x+04),a exposant

336D 2854 ï Z,

336F 92 sub d

3370 FE21 œ 21

3372 304F ÿ nc,3303

3374 1811 ï 3387

3376 AF xo a

3377 FD9604 sub  (y+04) exposant

337A 2859 ÿ z,33D5

337C DD8604 add  a,fix+04) exposant

337F FE21 cp à1

3381 3052 ï nc,33D5

-11 132-

ARITHMETIQUE A VIRGULE FLOTTANTE

3383 E5 push hl

3384 FDE1 pop  iy
3386 EB ex de,h
3387 5F Id ea
3388 78 td ab
3389 A9 Xor  C

338A F5 push af

338B C5 push bc
338C 7B Wd a,
3380 CD4336 call

3390 79 Wd ac
3391 C1 pop bc

3392 4F W ca
3393 F1 pop af

3394 FADA33 jp m,33DA
3397 FD7E00 Id a,(iy+00)
339A 85 add ‘ll

339B 6F d La
339C FD7E01 I  afy+01)
339F 8C adc ah
33A0 67 Wd ha
S3A1 FD7E02 Wd 8,(y+02)
33M4 8B adc ae
33A5 5F Wd ea
33A6 FD7E03 W 8,(y+03)
33A9 CBFF set 7,4
33AB 8A adc ad
43AC 57 id da
33AD D2BA36 jp nc, 36BA
3380 CB1A " d

33B2 CB1B " e

33B4 CBC " h

3386 CB1D T I

3388 CB19 " c

33BA DD3404 inc (ix#+4) augmenter exposant
33BD C2BA3%6 P nz,36BA
33C0 C3EE36 jp 36EE dépassement
CLLELELLELLEEE ETS ETES TESTS ST T TS EST SIT TT TT TTTS
3303 FD7E02 Id e,(fy+02)
33C6 DD7702 Id (x+02),a
33C9 FD7EO Id © afy+0t)
33CC DD7701 d (x+01},a
33CF FD7E00 W a,(y+00)
33D2 DD7700 W (x+00),a
3305 007003 Wd (x+03),b
3308 37 scf

33D9 C9 ret

33DA AF xor a

33DB 91 sub  c

33DC 4F Id ca
3300 FD7E00 W a.(y+00)
33E0 9D sbc al

33E1 6F id la

-I1 133-

ARITHMETIQUE A VIRGULE FLOTTANTE

33E2 FD7E01 W a,(y+01)
33E5 9C sbc ah
33E6 67 id ha
33E7 FD7E02 Id a,(y+02)
33EA 9B sbc ae
33EB 5F id ea
33EC FD7E03 d a.(y+03)
33EF CBFF set 7,4
33F1 9A sbc ad
33F2 57 Wd da
33F3 3016 j nc,340B
33F5 78 Id a,b
33F6 2F cpl a
33F7 47 W ba
33F8 AF Xor à
33F9 91 sub  c
33FA 4F id ca
33FB 3E00 id a,00
33FD 9D sbc al
FE 6F d la
33FF 3E00 ld a,00
3401 9C sbc ah
3402 67 d ha
3403 3E00 id a,00
3405 9B sbc ae

5F ld e,a
3407 3E00 id a,00
3409 9A sbc ad
340A 57 Id da
340B 87 add aa
340C DABA36 jp c,36BA
340F C3B136 jp 3681

DO sens sesssssssssesss multiplication par

3412 11590F Wd de,2F53 10

BRON 80588685 BD6] multiplication

3415 D5 push

3416 FDE1 pop y
3418 E5 push hl
3419 DDE pop ix
341B FD7E04 id a,(y+04) exposant
JAIE B7 of a
341F 282C ï 2,344D
3421 3D dec a
3422 CD4835 cal 3548
3425 2826 x z,344D
3427 3021 ï nc,344A
3429 F5 push af
342A C5 push be
342B CD5034 cal 3450
342€ 79 id ac
342F C1 pop bc
3430 4F Wd ca
3431 Fi pop af

-11 134-

LRERELEE IEELESLELEE LE LE LELEEL LEE ELLILELLL IRL LELL EL LLLLEL)]

CB7A bit
200D ï
30 dec
2814 ÿ
CB21 sla
CB15 d
CB14 f
CB13 U]
CB12 d
DD7704 id
B7 or
C2BA36 jp
C3EE36 P
C3E636 jp
210000 Id
5D id
54 Id
F07E00 d
CD9334 call
FD7E01 id
CD9334 call
FD7E02 Wd
CD9334 call
F07E03 id
F680 of
0608 ld
1F rra
4F Id
3014 ÿ
70 D]
DD8600 add
6F id
7C id
DD8E01 adc
67 id
7B id
DD8E02 adc
5F Id
7A id
DD8E03 adc
57 id
CBIA "
CB1B "
CB1C "
CBiD "
CB19 "
10DE djnz
Ca ret

ARITHMETIQUE A VIRGULE FLOTTANTE

7,d
n2,3443

a
z,344D

ao

(x+04),a exposant
a

nz,36BA
86EE dépassement

96E6 dépassement par le bas

hl,0000
el

dh
a,(y+00)
3493
a.fy+0t)
3493
a,(y+02)
3493
a,fy+03)
80

b,08

ca
nc,3486
a
a.(ix+00)
la
ah
a,(ix+01)

-11 135-

ARITHMETIQUE A VIRGULE FLOTTANTE

3493 B7 or a
3494 20D6 ï nz,346C
3496 6C W 1h
3497 63 W he
3498 5A Wd ed
3499 57 W da
349A C9 ret

RU mem sseseseseess division par 10
3498 11532F ld de,2F53

OO OS Se esse eesseusessses BD64 division

349E D5 push de
349F FDE1 pop ji
S4A1 E5 push hl
34A2 DDE1 pop ix
34A4 AF xor a
34A5 FD9604 sub  (y+04) exposant
SAAB 2858 ÿ z,
34AA CD4835 call 3548
AD CAEG36 jp 23666 dépassement par le bas
3480 304D ï nc,34FF
34B2 CS push bc
34B3 4F Wd ca
34B4 5E Wd e,h)
34B5, 23 inc

3486 56 Wd d(h})
3487 23 inc h
3488 7E Id a{hl)
34B9 23 inc hl
34BA 66 Wd h{h)
34BB 6F id la
34BC EB ex dehl
34BD W b.(y+03)
34C0 CBF8 set  7.b
34C2 CD3235 call 3532
34C5 3006 F4 nc,34CD
3407 79 id ac
34C8 B7 or a
34C9 2008 ÿ n2,34D3
34CB 1831 ÿ JAFE
34CD 0D dec c

S4CE 29 add hihi
SACF CB13 d e

3401 CB12 d d
3403 0D7104 id (x+04),c exposant
34D6 CD0735 cal 3507
3409 DD7103 id (+08), c
34DC CD0735 call 3507
34DF BD7102 ld (ix+02),c
34E2 CD0735 cat 3507
S4E5 D07101 Id (x+01),c
34E8 CD0735 call 3507
94EB D43235 cal nc,3532

-11 136-

ARITHMETIQUE À VIRGULE FLOTTANTE

QMEE 9F sbc aa
34EF 69 id lc
34F0 DD6601 Id hfix+01)
34F3 DD5E02 W e.(ix+02)
34F6 DD5603 id d'(x+03)
34F9 C1 pop bc
JAFA 4F Wd ca
34FB C3BA36 jp 36BA
FE C1 pop bc
34FF C3EE36 jp 36EE dépassenent
3502 CD9435 cal 3594
3505 AF xof a
3506 C9 ret

3507 0E01 Id coi
3509 3808 rad c,3513
350B 7A Id ad
350C B8 Œp  b
3500 3F ccf

350€ CC3635 cal  z,3536
3511 3013 ÿ nc,3526
3513 7D d a
3514 FD9600 sub  (y+00)
3517 6F id la
3518 7C Wd ah
3519 FD9E01 sbc  afiy#01)
351C 67 ld ha
351D 7B Wd a,8
351E FD9E02 sbc  a,fy+02)
3521 5F id ea
3522 7A Wd ad
3523 98 sbc ab
3524 57 ld da
3525 37 scf

3526 CB11 d c
3528 9F sbc aa
3529 29 add hihl
352A CB13 d e
352C CB12 d d

352€ 3C inc a
352F 20D8 ï n,3509
3531 C9 ret

3532 7A d a,d
3533 B8 cp b
9594 3F

3535 C0 ret nz
3536 7B Id ae
3537 FDBEO2 cp  (iy+02)
353A 3F ccf

353B C0 ret nz
353C 7C Id ah
3530 FDBEO! cp (y#0i)

-11 137-

ARITHMETIQUE A VIRGULE FLOTTANTE

3540 3F ccf

3541 C0 rt nz

3542 7D d ail

3543 FDBEO0 cp (+00)

3546 3F ccf

3547 C9 ret

3548 4F W ca

3549 DD7E03 d a,(ix+03)

354C FDAEO3 xor  (iy+03)

354F 47 W ba

3550 DD7E04 Id a,(ix+04) exposant

3553 87 of a

3554 C8 ret z

3555 81 add .ac

3556 4F d ca

3557 1F rra

3558 A9 xor €

3559 79 id ac

355A F26835 JP p.3568

355D DDCBO3FE set  7,(ix+03) signe négatif
3561 067F sub  7F

3563 37 scf

3564 C0 rt nn

3565 FEO1 œ 01

3567 C9 ret

3568 B7 or a

3569 F8 rt m

356A AF xor a

3568 C9 ret

356C E5 push hl

356D DDE1 pop ix

356F DD7E04 ld a,(ix+04) exposant

3572 B7 of a

3573 C8 ret  z

3574 D680 sub 80

3576 37 scf

3577 C9 ret

OU nn nn nv essvsssse BD67 multiplier par 2°a
3578 E5 push hl

3579 DDE1 pop ix

357B 67 or a puissance de deux dans accu
357C FAB935 JP m,3589 négatif?

9357F DD8604 add  a(ix+04) augmenter exposant
3582 DD7704 td (x+04),a et sauvegarder à nouveau
3585 3F ccf

3586 D8 ret c L

3587 180B ÿr 3594 dépassement?

-I1 138-

ARITHMETIQUE À VIRGULE FLOTTANTE

3589 DD8604 add  a,(x+04) additionner exposant

358C 3802 ï c,3590 pas de dépassement par le bas
358E AF xor a zéro comme résultat

358F 27 scf

3590 DD7704 id (x+04),a sauvegarder à nouveau l'exposant
3593 C9 ret

3594 DD4603 id b,(ix+03) signe de la mantisse

3597 CDEE3% cal  S6EE dépassement

RO nn nn ns ss sssesssssssssys BDGA comparer
959A E5

3598 DDE1 pop  ix

3590 D5 push de

359€ FDE1 pop y

35A0 DD7E04 k a,(ix+04) comparer
35A3 FDBE04 cp (y+04) exposants
35A6 383A ÿ c,35E2

35A8 2033 j nz,35DD

35AA B7 of a

35AB C8 ret 2z

35AC DD7E03 W a.(ix+03)

35AF FDAEO3 xor  (iy+03)

3582 FADD35 jp m,350D

3585 DD7E03 W a,(x-+03)

35B8 FD9603 sub  (y+03)

35BB 2017 y n2,35D4

35BD DD7E02 d a,(ix+02)

35C0 FD9602 sub  (y+02)

35C3 200F y n2,35D4

35C5 DD7E01 Wd a.(ix+01)

35C8 FD9601 sub  (iy+01)

35CB 2007 ÿ nz,35D4

35CD DD7E00 ld a,(ix+00)

3500 FD9600 sub  (y+00)

35D3 C8 re z

85D4 9F sbc aa

3505 FDAEO3 xor  (y+03)

3508 87 add aa

3509 9F sbc aa

35DA D8 rt c

35DB 3C in a

35DC C9 ret

35DD DD7E03 Id a,(ix+03)

35E0 18F6 ÿ 85D8

35E2 F07E03 d a,(y+03)

35E5 2F cpl a

35E6 18F0 ÿ 35D8

DR RN N  n esnennssseuussmesesvvus BD70 SGN
35E8 E5 push hl

95E9 DDE1 pop ix

35E8 DD7E04 d a,(ix+04) exposant

-11 139-

ARITHMETIQUE À VIRGULE FLOTTANTE

95EE B7 or a

J5EF C8 ret 2

35F0 DD7E03 Id a,(ix+03)

95F3 87 add aa

35F4 9F sbc aa

95F5 D8 ret  c

95F6 3C inc a

35F7 C9 ret

ns ss ssssssæsses BD6D changer signe
95F8 ES push hl

35F9 DDE1 pop ix

35FB DD7E03 id a,(ix+03) signe de la mantisse
35FE EE80 xor 80 inverser

3600 DD7703 id (x+03),a

3603 C9 ret

DORA NU nn nn nn nn nn en s sn nnnnussssssssss FIX
3604 AF xor à

3605 DD9604 sub  (ix+04) exposant

3608 200A ÿ nz,3614 nombre non nul, alors à entier
360A 0604 ld b,04

960€ 77 \d {ha supprimer mantisse

360D 23 inc  hl

360E 10FC djnz 360€

3610 O0EO1 d c01

3612 37 scf

3613 C9 ret

snssssssssssssssssssssss conversion virgule flottante à entier
3614 CGAO add  a,A0

3616 DO ret nc
9617 ES push hl
3618 CD3D36 call  363D
361B AF xor a
361C B8 cp b
361D 8F adc aa
361E B1 of C
361F 4D Id c
3620 44 ld bh
3621 Et pop hl
3622 71 id hi,c
9623 23 inc HN
3624 70 d Mb
3625 23 inch
3626 73 Id he
3627 23 inc  hl
3628 5F Id ea
3629 7E Id a,fh))
362A 72 Id fhi,d
362B E680 and 80
362D 47 Wd ba
362€ 0E04 ld c04
3630 AF xof a
3631 B6 of {h))

-11 140-

3679

ÿ

sx 88

38e

Le]
Tv

&Saceseses Fegs 5838308380

ge? 2 2 F£agTe

=
2

ARITHMETIQUE A VIRGULE FLOTTANTE

nz,3639
hl

nz,3631

-11 141-

ARITHMETIQUE À VIRGULE FLOTTANTE

367A B4 or h
367B B5 of l
367C B1 or c
367D C8 ret z
367E 7A Id ad
367F D608 sub 08
3681 381C x c,369F
3683 C8 rt z
3684 53 d de
3685 5C Wd eh
3686 65 Id h}
3687 69 ld lc
3688 0E00 id c,00
368A 14 in d
368B 15 dec d
368C 28F1 ÿ 2,367F
368E F8 ret m
368F 3D dec a
3690 C8 ret z
3691 CB21 sa c
3693 CB15 d [
3695 CB14 d h
3697 CB13 d e
3699 CB12 d d
3698 F28F36 jp p,368F
369E C9 ret

369F AF xof a
36A0 C9 ret

sense ssensssessssss conversion entier à virgule flottante

36A1 ES push hl
36A2 DDE1 pop  ix
36A4 DD7004 Co] (x+04)b exposant
36A7 47 id ba
36A8 5€ Id eh)
36A9 23 inc  h
36AA 56 id di}
36AB 23 inc h
36AC 7E Id a{h)
36AD 23 inch
S6AE 66 id hh)
S6AF 6F id La
3680 EB ex  deh
36B1 DD7E04 Wd a,(x+04) exposant
3684 CD7336 cal 3673
3687 DD7704 Wd (x+04),a exposant
J6BA CB21 a €
36BC 3013 r nc, 3601
J6BE 2C nc |
36BF 2010 jr n2,3601
36C1 24 inc h
36C2 2000 ÿ n2,36D1
1C inc  e
36C5 200A jr n2,3601

-[1 142-

36C7 14 in d

36C8 2007 ÿ nz,36D1

86CA DD3404 inc (x+04) exposant
36CD 281F ÿ ZJ6EE dépassement
36CF 1680 d d,80

36D1 78 Wd ab

36D2 F67F of 7F

36D4 A2 and d

3605 DD7703 Wd (x+03),a

36D8 DD7302 d (ix+2),e

36DB DD7401 Wd (+01), h

36DE DD7500 Id  (x+00)!

96E1 DDES5 push ix

36E3 El pop  hl

36E4 37 scf

36E5 C9 ret

en nssssesssessssssssssssssssdépassement par le bas, zéro
36E6 AF xor a

36E7 DD7704 Wd (ix+04),a exposant
36EA 18F5 ÿ 86E1
tésssessssssssssssssssss dépassement, plus grand nombre positif
36EC 0600 Wd b,00 signe positif
SEE 78 d ab

S6EF F67F or 7F

36F1 DD7703 id (x+03),a mantisse avec signe
36F4 FGFF or FF

36F6 DD7704 Wd (ix+04),a exposant
36F9 DD7700 Id (ix+00),a

36FC DD7701 d (x+01),a

36FF DD7702 Wd (x+02),a

3702 C9 ret

3703 C7 rst 0

3704 C7 rst 0

3705 C7 rst 0

3706 C7 rst 0

3707 C7 rst 0

ARITHMETIQUE À VIRGULE FLOTTANTE

-11 143-

ARITHMETIQUE AVEC ENTIERS

CFRSIIIITIITILIEILIIIZILILLILI) LIL IL LLL) LIL LL) LL LL LL LL LL LL, BDA3

3708 44 d  bh ranger signe

3709 CDD137 cal 3701 former valeur absolue

370C 1802 ï 370

RM nn nes seen eseesenessssss BDAG
370E 0600 Wd b,00

3710 1E00 W 6,00

3712 0E02 C4] c.0

3714 C9 ret

Re esse esssessss BDA9 accepter signe dans b
3715 7C Id ah

3716 B7 or a

3717 FA2037 p m,3720

371A B0 or b signe du résultat

371B FAD437 jp m,37D4 inverser signe

A71E 37 scf

371F C9 ret

3720 EE80 xor 80 inverser bit signe

3722 85 or I

3723 C0 rt nn

3724 78 W ab

3725 37 scf

3726 6F adc aa

3727 C9 ret

SR en ssesessssenessesses BDAC Additionhl:=hi+de
3728 B7 of a annuler flag carry

3729 ED5A adc hl,de addition

372B 37 scf

372C E0 ret po résultat positif?

3720 F6FF or FF fixer flags

372F C9 ret

RO esse ss BDB2 soustraction hl:= de-hl
3730 EB ex  dehl échanger opérandes
mens sensssssensss BDAF soustraction hl:= hl-de
3731 B7 or a annuler flag carry

3732 ED52 sbc hide soustraction

de o e po résultat positif?

3736 FGFF o FF FERRP ÉRIQR

3738 C9 ret

Hem eseesessesss BDB5 multiplication avec signe
3739 CD4537 cal 3745 déterminer signe du résultat
373C CD5037 cal 3750 multiplication sans signe
373F 091537 jp nc,3715 accepter signe

3742 F6FF of FF

3744 C9 ret

-I1 144-

ARITHMETIQUE AVEC ENTIERS

RUSSE déterminer signe du résultat

3745 7C W ah signe de h1

3746 AA xor et signe de de

3747 47 Wd ba amener dans b

3748 EB ex de,hl

3749 CDD137 cal  37D1 former valeur absolue de de
374C EB ex de,hl

3740 C3D137 je 3701 former valeur absolue de hl
nn sensesssssesssess BDBE multiplication sans signe
3750 7C W ah

3751 67 or a

3752 2805 x 2,3759

3754 7A d ad

3755 B7 of a

3756 37 scf

3757 C0 ret nz

3758 EB ex de,hl

3759 B5 or |

375A C8 rt 2

3758 7A Ce] a,d

375C B3 or e

3750 7D d al

375€ 6B d le

375F 62 d hd

3760 C8 ret 2

3761 FE03 cp 03

3763 3810 ÿr c,3775

3765 37 scf

3766 &F adc aa

3767 30FD jr nc,3766

3769 29 add hihi

376A D8 rt cC

376B 87 add aa

376C 3002 jr nc,3770

376€ 19 add hide

376F D8 ret C

3770 FE80 cp 80

3772 20F5 ru n2,3769

3774 C9 ret

3775 FEO1 cp 01

3777 C8 ret 2

3778 29 add hihl

3779 C9 ret

M ORRRRRNeneesetsess BDBB division avec signe
377A CD8937 call 3789 division hl:= hl/de
9770 DA1537 jp c,3715 accepter signe

3780 C9 ret

DR noms seesssnesssesusese BDBB MOD

3781 4C Wd ch ranger signe
3782 CD8937 call 3789 division
3785 EB ex dehl reste dans hl

-11 145-

ARITHMETIQUE AVEC ENTIERS

3786 41 d bc rappeler signe
3787 18F4 ï 3770 et accepter
3789 CD4537 cal 3745 déterminer signe du résultat
sense esessssenesssessssese BDC1 Divisionhl:= hl/de, de := Rest
378C 7A Wd ad sans signe
3780 B3 or e diviseur zéro, alors terminer
378€ C8 re z

978F C5 push be

3790 EB ex de,hl

3791 0601 Id b,01

3793 7C Wd ah

3794 B7 or a

3795 2009 jr nz,37A0

3797 7A id ad

3798 BD cp |

3799 3805 jr c,37A0

379B 65 id h}

379C 2E00 Id 1,00

379E 0609 W b,09

J7A0 7B Wd ae

37A1 95 sub |!

37A2 TA Hd ad

37A3 9C sbc ah

37A4 3805 ÿ c,37AB

37A6 04 mc  b

37A7 29 add hih!

37A8 30F6 4 nc,37A0

J7AA 3F ccf

37AB 3F ccf

37AC 78 id ab

37AD 44 Wd bh

37AE 4D ld ci

37AF 210000 ld hl,0000

3782 3D dec a

37B3 2003 4 nz,37B8

3785 1817 l'A 37CE

37B7 29 add hihi

3788 F5 push af

37B9 78 Wd ab

37BA 1F ra

37BB 47 [] ba

37BC 79 Id ac

37BD 1F ra

37BE 4F d ca

37BF 7B Wd a,e

37C0 91 sub  c

37C1 7A ld ad

37C2 98 sbc ab

37C3 3805 ÿ c,37CA

37C5 57 d da

37C6 7B Id ae

9707 91 sub  c

-11 146-

ARITHMETIQUE AVEC ENTIERS

37C8 5F d ea

37C9 2C inc |

37CA F1 pop af

37CB: 30 dec a

37CC 20E9 y n2,37B7

a7CE 37 scf

37CF C1 pop bc

3700 C9 ret

RSR Rss ss former valeur absolue
37D1 7C id ah tester signe

3702 B7 or a

3703 F0 ret p positif, alors déjà terminé
en enson sos sesésssssssess BDC7 changement de signe h1
37D4 AF XOr a

37D5 95 sub |

3706 6F id la

37D7 9C sbc ah

37D8 95 sub |

3709 BC cp h

370A 67 W ha

370B 37 scf

370C C0 ret on

3700 FEO01 cp 01

370F C9 ret
sms nn ns eme susss BDCA SGN signe de hi
37E0 7C ld ah

37E1 87 add aa

37E2 9F sbc a,a

37E3 D8 rt  c

37E4 B5 or I

97E5 C8 ret 2

37E6 AF xof a

37E7 3C inc a

37E8 C9 ret

MR ns ss ss BDCA comparer h1 <> de
S7E9 7C Id ah signe de h1l

S7EA AA xor  d et signe de de

37E8 7C kW ah

37EC F2F437 P p,37F4 comparer nombres avec même signe
S7EF 67 add aa

37F0 9F sbc aa

37F1 D8 re c

97F2 3C inc. a

37F3 C9 ret

37F4 BA Œp  d

37F5 20F9 ni nz,37F0

37F7 7D ld all

37F8 93 sub &

37F9 20F5 ÿ nz,37F0

37FB C9 ret

-I1 147-

BASIC

6 EH HER EME HE CHHE GE DEEE EEE M EE EH EEE

CO00
coût
C002
C003
Co04

80
01
00
00
4CCO

db
db
db
db
dw

80
01
00
00
CO4C

AE HD DEEE DEMO AE JE MEME HE HE DE DE HE MEME DE HE EEE SEE EEE

CO06
C009
CO0C
COO0F

C012
c015
Co17
C019
COTA
Co1c
COTE
C021
CO24
CO25
C028
CO2B
CO2E
Co51
Co34
CO37
CO3A
CO3D

3100C0
CDCBBC
CDCHF A
DAO00O0

2100AC
3600
061B
23
36C9
10FB
213FC0
CD37C3
AF
3200AC
CDCBDD
CD84CA
CD97BD
CDD3C0
CD3SECI
11F000
CDO6F7
1825

ld
call
call
jp

Id
id
1d
inc
id
djnz
id
call
xor
ld
call
call
call
call
call
id
call
jr

sp, CO00
BCCB
F4C4
c,0000

h}, ACOO
(h1),00
b,1B

hl
(h1),C9
C019
h1,C03F
C337

a
(ACOO), a
DDCB
CA84
BD97
COD3
C13E
de, OOFO
F706
CO6ù

HE EH MEME DE HE NE HE DEDE HE HE EME HE HE JE NEED EH EE

CO3F 20 42 41 53 49 43 20 31

COu7 2E 30 OA OA O0

PELISILLILILLILSSLISSLESRS SSSR SS SES)

CO4C 42 41 53 49 C3 00

ADEME DEEE DEEE DE HE DE DE EN DE D DE DE DE DE HE DE DE EDEN

-[IT 1-

1.0

ROM-Header

première Rom de devant
Mark 1

Version 0
Modification 0
Adresse du nom

Initialisation du Basic

pile à partir de CO00

KL ROM WALK

configurer la mémoire

trop peu de mémoire, alors Reset

fret’ de ACO1 à ACIB

pointeur sur ’ BASIC 1.0"
sortir

suppr. flag pour ‘ignorer espaces”
adresse de ligne actuelle sur zéro
supprimer numéro d'erreur

RND Init

supprimer mode AUTO

NEW (instruction)

240

SYMBOL AFTER 240

au mode READY

“BASIC 1,0 LF,LF

BASI', ‘C'+80H, OOH

Instruction Basic5EDIT

BASIC
CO52 CDEICE call CEE
CO55 C0 ret nz
C056 3100C0 ld sp, CO00
CO59 CD9AE7 call E79A
CO5C CD63E1 call E163
CO5F  CD43CA call CAU3
C062 3854 jr c,COB8
CO64  CDOIAC call ACO1
C067 3100C0 id sp, CO00
CO6A  CD62C1 call C162
CO6D CDD6DD call DDD6
CO70  DCB6BC call c, BCB6
C073 CD48BB call BB48
C0O76 CD86C3 call C386
CO/79 3AUSAE ld a, (AE45)
CO/C B7 or a
CO/D C43EC1 call NZ C1SE
CO80 3AAAAD ld a, (ADAA)
CO83 D602 SUb 02
CO85 2009 jr nz, C090
CO87 32AAAD Id (ADAA),a
CO8A CDDFCA call CADF
CO8D EB ex de,hl
CO8E  38C6 jr c, C056
CO90 21CCC0 Id h1,COCC
C093 CD41C3 call C341
C096 CDCBDD call DDCB
C099 3ATCAC ld a, (AC1C)
CO9C B7 or a
CO9D 2811 jr z, COBO
CO9F CDO2C1 call C102
COA2 30C0 jr nc, CO064
COAU 7E Id a, (hl)
COAS B7 or a
COAG  28F1 jr z,C099
COA8 CDD2E6 call E6D2
COAB CD7AC1 call C17A
COÂAE 18E9 jr C099

-1I1 2-

1.0

aller chercher numéro de ligne
dans de

initialiser la pile

chercher ligne Basic de (existe?)
lister ligne Basic dans buffer
aller chercher ligne d'entrée

mode READY
ret

aller chercher adresse de ligne
SOUND HOLD

KM DISARM BREAK

initialiser écran

programme protégé ?

oui supprimer programme et variables
numéro ERROR

’Syntax error’ ?

non

numéro ERROR sur zéro

aller chercher numéro de ligne

de ligne ERROR

à l'instruction EDIT

Ready”

sortir

adresse de ligne actuelle sur zéro
flag AUTO mis ?

non
présenter prochain numéro de ligne
au mode READY

convertir instruction en code
interpréteur

BASIC

OH EH HE HE HE HE EE HE EE JE EEE

COBO CD3BCA call CA3B
COB3 30FB jr nc, COBO
COBS  CDAEC3 call C3UE
COB8 CDBCEG call E6BC
COBB 3005 jr nc, COC2
COBD C47ACI call nz,C17A
COCO 18D4 jr C096
COC2 CDBBDE call DEBB
COC5 CD53C4 call Cu53
COC8 2B dec hl

COC9 C374DD jp DD74

HN DE EME DE HE EEE MEME EE HE DEEE HE DE EEE EME EE EH

COCC 52 65 61 64 79 OÀ 00

MEME EH DE DE HE HEHEHEHEMEE HE HE MEME HE DE HD HE DEEE

COD3 AF
COD4

xor a
1805 ir CODB

HOHEHE HE HD EH MEHE HE DEMO HE HE EEE EH HE HE HE HONG

COD6 221DAC ld (ACID), I
COD9 3EFF 1d a, FF

CODB 321CAC id (ACIC),a
CODE C9 ret

HHEOHEOHEME MH HE HE MEME HOME HE HE HE MEME MEME HE HE DE HEHHE MEME ME HEEHH

CODF 110400 ld de, 0004
COE2 2802 jr Z, COE6
COE4 FE2C CP 26

COE6 CAEICE call nz, CEE
COE9 DS push de

COEA 110400 1d de, O00A
COED CDS5DD call DD55
COFO DCEICE call c, CEE
COF3 CDUADD call DD4A
COF6 EB ex de,hl
COF7 221FAC id (AC1F),h1
COFA ET pop hl

-III 3-

aller chercher ligne d'entrée
'ESC' enfoncée, alors répéter
sortir LF

convertir ligne en code interpréteur

instruction directe ?

copier ligne dans buffer à partir de 840

autoriser interruption par ‘Break’

à la boucle de l'interpréteur

Ready’, LF, OH

supprimer mode AUTO
0

fixer mode AUTO
numéro de ligne

fixer flag pour AUTO

Instruction Basic AUTO
10, Defaut

1!
4

chercher No de ligne dans de

10, Defaut

virgule suit?

oui, chercher No de ligne dans de
fin de ligne, sinon ‘’Syntax error’

ranger incrément AUTO

BASIC

COFB  CDD6CO call COD6

COFE C1 pop bc

COFF  C396C0 jp C09,6
C102 2A1DAC 1d h1, CACID)
C105 ES push hl

C106 CD/EE call EE79
C109 D1 POP de

C10A CDA3E/ call E7A3

C10OD 3E2A Id a, 2A

C10F 3802 jr c,C113
C111 3E20 1d a, 20

C113 CD56C3 call C356
C116 CDD3C0 call COD3
C119 CD3BCA cali CA3B

C11C DO ret nc

C11D CDAEC3 call C3UE
C120 ES push hl

C121 2ATFAC ld hl, (ACIF)
C124 19 add hl,de
C125 DA4D6CO call nc, COD6
C128 EI pop hl

C129 37 scf

C12A C9 ret

C12B CO ret nz

C12C CD3ECI call CTSE
C12F C364C0 jp CO64
HORMONE MEME HE JE EH JE HE JE JE EE HE EEE HE HE EE HE HE HE
C182;"E5 push hl

C133 CD8CCI call C18C
C136 CD5BCI call C15B
C139 CD7/ACI call C17A
C13C EI pop hl
C13D C9 ret

HOHORODEONECHEDE DE DEN HE EE HE HE DE HE DEEE NH HER EEK

C13E 2A/FAE Id h1, CAE7F)

-II]I 4-

1.0

fixer flag pour mode AUTO

No de ligne
sortir No de ligne

chercher ligne

1#1

ligne existe ?

‘6

sortir

supprimer mode AUTO

aller chercher ligne d'entrée
ESC enfoncée ?

sortir LF

No de ligne

plus incrément
fixer mode AUTO

Instruction Basic NEW

supprimer programme et variables
au Mode READY

Instruction Basic CLEAR

supprimer programme et variables
début de la Ram libre

c141
C142
C145
C148
c149
C14A
C14B
C14c

C14F
C152
C155
C158
C15B
C15E
C15F
C162
C165
C168

C16B
C16E
C171
C174

C177
C17A
C17D
C180
C183
C186
C189

EB
2A7BAE
CDDAFF
62

6B

13

AF

77
EDBO
2USAE
D76E6
D8Cc1
D6BC1
DADD2
CD/3BD
CDB3FB
CDFDD9
C39DC1

Z»z»OON0C0ON

CDE6DD
CDD3C0
CDF2F1
CD76E6

CDB1D5
CDD9CB
CDABCB
CDEDC8
CD8EF5
CDD2D5
C3ESDC

ex
Id
call
id
id
inc
xor
id
idir
Id
call
call
call
call
xor
call
call
call
jp

call
call
call
call

cal]
call
call
call
call
call
jp

de,hl
h1, (AE
FFDA
nd
l,e

de

a
(hl),a

CAE45)
E676
C18C
C16B
D2AD
a
BD73
FBB3
D9FD
C19D

DDE6
COD3
F1F2
E676

D5B1
CBD9
CBAB
C8ED
F58E
D5D2
DCES

BASIC

7B)

4

RHONE OHOHOHEHE HE HE GE HE EME EME HE DE HE EEE HE EE EE

C18C
C18D
C18
C191
C194
C197

C5
ES
CDCAFS
CDAEDS
CDFCDS
CD8SE9

push
push
call
cal]
call
call

bc
hi
F5CA
D5AE
D5FC
E989

HI :5R

1,0

HIMEM
bc := hl - de

vider l'accu

vider début Ram libre jusqu'à HIMEM
supprimer flag pour progr, protégé
fin du programme := début du progr.
supprimer les variables

interrompre 1/0 cassette

fixer mode RAD
initialiser pile du descripteur

TROFF

supprimer mode AUTO

fixer TAB-Stops sur 13

fin de programme := début de
programme

restaurer pointeur de variable
supprimer ON-ERROR

interdire CONT

reset SOUND et Event
initialiser pile Basic
supprimer flag pour FN

RESTORE

supprimer variables
restaurer pointeur de chaîne

restaurer pointeur de variable
Variables A-Z sur ‘Real’

C19A
C19B
C19C

EÎ
C1
C9

pop
pop
ret

BASIC

hl
bc

HOHEOHOHEDECHOHEOHHE HE HE H HE HE E HE EM MEN EE JE EH He

C19D
C1
CTA1
CTA2
CTA3
CTA4
CTAG
CTA9
CTAA
CTAD

TAF
1B0
1B3
C1B4

F2: 362

C1B8
C1B9

C3

(æ)
N

C1C6

AF
CDAFCI
ÂF

ES

F5
FE08
DCB4BB
F1
2121AC
1804

ES
2122AC
D5
5F
JE
75
D1
ET
C9

3A21AC
FEU8
C9

3A22AC
FE09
C9

CDE3C1
18D7

CDE3C1
18DF

xor
call
Xor
push
push
Cp
cali
pop
Id
jr

push
id
push
1d
ld
ld
POP
pop
ret

id
CP
ret

CP

ret

call
jr

cal}
fr

a
CTAF
a

hi
af
08
c,BBB4
af
hl,AC21
C1B3

hl, AC22
de

e,a

a, (h1)
(hl),e
de

hl

a, (AC21)
08

a, (AC22)
09

CTE3
CTA2

CTE3
CTAF

SNS

1.0

& 8 1?
TXT STR SELECT

numéro stream act,

canal d'entrée

numéro stream act.
imprimante?

canal d'entrée
cassette ?

BASIC

CHERE DE HE ECHO EHE HE HE HOME HE HE HOME HE EN H

C1D0
C1D3
C1D5
C1D7
C1DA
C1DB
C1DC
C1DF
C1E0

CDE3C1
FEO8
302E
CDA2C1
C1

F5
CDF9FF
F1
C3A2C1

call
Cp
jr
call
pop
push
call
Pop
jp

C1E3
08

nc, C205
C1A2

DC

af

FFF9

af

C1A2

HE HE HEOHOMOHEHEOHEHOHEHE EME HOMME HE HE EME CHE HE CH HE EEE

C1E3
C1E4
C1E6
C1E8
C1E9
C1EC
C1ED
CFO
C1FS

7E
FE23
3E00
CO
CDF5C1
F5
CDS5DD
D4UHADD
(I

Id
CP
id
ret
call
push
call
call
TEp

a,(hl)
23

a,00

nz

C1F5
af

DD55
nc, DD4A
af

HOMME HE HE HE HE DE HE HE DE HE D ME E DE  DEHEHÉHÉEHEHE EEHEN

CiF5
C1F8
C1F9
C1FB
C1FC
C1FD
C1FE
C201
C202
C203
C204
C205
C207

CD37DD
23
3EOA
C5

D5

47
CD67CE
B8

Di

C1

D8
1E05
C39uCA

call
db
Id
push
push
Id
call
CP
Pop
pop
ret
ld
jp

DD37
23
a, OA
bc
de
D,a
CE67
b

de
bc

C
e,05
CA94

RHONE HER HEURE EH EE HO EEE

C20A

CDDOC1

call

C1D0

SES

aller chercher numéro stream

‘Improper argument‘

jp (bc) exécuter fonction

tester si numéro stream

0 si defaut
aller chercher numéro stream

virgule suit ?
non,

aller chercher numéro stream
Tester si encore un caractère
Fat

10, valeur maximale

aller chercher valeur 8 bits
comparer avec D

inférieur à b, ok
‘Improper argument
sortir message d'erreur

Instruction Basic PAPER
aller chercher numéro stream

alors fin de l'instruction?

BASIC

C20D 0196BB 1d bc, BB96
C210 1806 IA C218

HOME HE HE DEN DE DE JE DE DE HE ED DE DE EME EE EE NÉ NE

C212 CDDOC1 call C1D0

C215 0190BB 1d bc, BB90
C218 CDABC2 call C24B
C21B ES push hl

C21C CDF9FF call FFF9
C21F ET pop hl

C220 C9 ret

HHHEDEHE DEEE HD HE HE DE MEHÉHEH EEE HÉHE HE E E

C221 CD3CC2 call C23C

C224 ES push hl
C225 CD38BC call BC38
C228 EI POP hl
C229 C9 ret

HUE HD DE DE DEMHEHE EDEN DE DEEE MEME HE DEEE DE HE DEEE

C22A CD4BC2 call C24B

C22D F5 push af
C22E CD37DD call DD37
C231 2C db 2C
C232 CD3CC2 call C23C
C235 F1 POP af
C236 E5 push hl
C237 CD32BC call BC32
C23A El POP nl
C23B C9 ret

HDMI

C23C CD4uC2 call C24t

C23F 41 Id b,c
C240 CDS5SDD call DD55
C243 DO ret nc
C244 3E20 Id a, 20
C246 CDFBCI call C1FB
C249 UF Id c,a
C24ÿA C9 ret

SLT :8

TXT SET PAPER

Instruction Basic PEN

aller chercher numéro stream
TXT SET PEN

aller chercher argument < 16

jp (bc) exécuter fonction

Instruction Basic BORDER
aller chercher argument(s) < 32

SCR SET BORDER

Instruction Basic INK
aller chercher argument < 16

Tester si encore un caractère

1
,

aller chercher argument(s) < 32

SCR SET INK

aller chercher argument(s) < 32
aller chercher argument < 32

virgule suit?

non

32

aller chercher argument < 32

BASIC 1.0

EEE HE EME JE DE DE DE DEEE JE JE HE ED JE DE DEEE EEE HE HE aller chercher argument L 16
C24B 3E10 Id a,10 16

C24D 18AC jr C1FB aller chercher argument < 16
RH DEEE HE DE ÉD HE EH DE ÉD EEE EME EEE EE Instruction Basic MODE

C24F 3E03 ld a,03 z

C251 CDFBCI call C1FB aller chercher argument < 3
C254 ES push h1

C255 CDOEBC call BCOE SCR SET MODE

C258 ET pop hl

C259 C9 ret

HONNEUR HE HE HE HE EE HE HE EE HE EE HE EH Instruction Basic CLS

C25A  CDDOC1 call C1D0 aller chercher numéro stream
C25D 3E0C id a, OC FF

C25F C36EC3 jp C36E sortir

HOMME DEMO HE HE HE HE DEEE EE HE EEE EH VPOS

C262 0167C2 id bc,C267

C265 1812 jr C279

C267 3A21AC 1d a, AC21 numéro stream act.

C26A FEO8 Cp 08 > 8 ?

C26C 3097 jr nc, C205 ‘Improper argument’

C26E CD/8BB call BB78 TXT GET CURSOR

C271 CD87BB call BB87 TXT VALIDATE

C274 JD ld a, l

C275 C9 ret

LÉELSES SLI EL LLL SL LLLLLLLLSILLS LS LSS. POS

C276 0190C2 Id bc, C290

C279 CDF5C1 call C1F5 aller chercher valeur < 10
C27C CDA2C1 call C1A2 Select Stream

C27F F5 push af

C280 CD37DD call DD37 Tester si encore un caractère
C283 29 db 29 )'

C284 ES push hl

C285 CDF9FF call FFF9 jp (bc) exécuter fonction
C288 CDOAFF call FFOA accepter contenu accu comme nombre

entier

AIT

BASIC

C28B EI POP hi
C28C F1 POP af
C28D C3A2CI jp C1A2

HE EEE GE DE DE HE EE HE GE HE EEE HE ECHO HE EE NE

C290 3A21AC Id a, (AC21)
C293 FEO8 CP 08

C295  CADFC3 jp z, C3DF
C298  3A25AC 1d a, (AC25)
C29B DO ret nc

C29C C39CC3 jP C39C

EH MEME HEHE DEEE EE HD EME ONE DE HE HE SECHE EH

C29F 3A21AC ld a, (AC21)
C2A2 FEO8 CP 08
C2A4 280D jr z,C2B3
C2A6 DO ret nc
C2A7 DS push de
C2A8 ES push h]l
C2A9 CD6SBB cal] BB69
C2AC 7A ld a,d
C2AD 94 SUD h

C2AE 3C inc a

C2AF EI pop hl
C2B0 D1 pop de
C2B1 37 scf

C2B2 C9 ret

C2B3 3A2UAC ld a, (AC24)
C2B6 FEFF CP FF
C2B8 C9 ret

C2B9 ES push pl
C2BA CDBFC2 call C2BF
C2BD El pop hl
C2BE C9 ret

C2BF 67 ld ha
C2C0 CD9FC2 call C29F
C2C3 3F ccf

1.0

Select Stream

aller chercher position PRINT act,

Numéro stream act.

aller chercher position imprimante

aller chercher position cassette

aller chercher position écran

numéro stream act.
imprimante?

oui

cassette ?

TXT GET WINDOW

WIDTH

-[1I 10-

C2Cu
C2C5
C2C6
C2C9
C2CA
C2CB
C2CC
C2CD
C2CE
C2CF
C2D0
C2D1

D8
6F
CD90C2
3D
LA
C8
84
3F
DO
3D
BD
C9

ret
ld
call
dec
scf
ret
add
ccf
ret
dec
CP
ret

BASIC

EE ERRONÉE HE HE HE EME HE DE DH HEC HN

C2D2
C2D5

C2D8
C2D9
C2DA
C2DB
C2DC
C2DF
C2E0

CDDOC1
CD27C3

ES
EB
24
2C
CD/5SBB
ET
C9

call
call

push
ex
inc
inc
call
pop
ret

C1D0
C327

hl
de,hl
h

l
BB75
hl

HOME HEOH HE HOHOHE HE HE HE HER HHE HE MH HE HE HE EME MEME EH

C2E1
C2E2
C2E4
C2E6
C2E9

C2EC
C2ED
C2F0
C2F1

C2F4
C2F5
C2F6

JE
FEE7
2817
CDDOC1
CD27C3

D5
CD37DD
2C
CD27C3

E3
7A
55

Id
CP
jr
cal]
call

push

cal]

db
call

ex
id
Id

a, (hl)

E7

z, C2FD

C1D0
C327

de

DD37

2C
C327

(sp),hl
a, d
d,1

1.0

Instruction Basic LOCATE
aller chercher numéro stream
aller chercher 2 valeurs 8 bits non

nulles

TXT SET CUSROR

Instruction Basic WINDOW

SWAP"

Aller chercher numéro stream

Aller
nulles

Tester

Or
»

aller
nulles

-lII 11-

chercher 2 valeurs 8 bits non

si encore un caractère

chercher 2 valeurs 8 bits non

BASIC 1.0

C2F7 6F 1d l,a

C2F8 CD66BB call BB66 TXT WIN ENABLE

C2FB EI pop hl

C2FC C9 ret

HORDE DE M 6 MH D dE JE DE HE HE HE EH EME HE JE HE HE JE WINDOW SWAP

C2FD CD3FDD call DD3F ignorer espaces

C300 CDIi2C3 call C312 aller chercher argument < 8

C303 48 ld C,b

C304  CDS5SDD call DD55 virgule suit ?

C307 0600 ld b,00 valeur défaut 0

C309 DC12C3 cal c,C312 oui, aller chercher argument < 8

C30C ES push hl

C30D CDB/BB cal BBB7 TXT SWAP STREAMS

C310 EI pop hi

C311 C9 ret

HE EE MD DE HE JE MH DE HE DE HE HE JE MH JE HE EE Xe aller chercher argument < 8

C312 3E08 ld a,08 8

C314  CDFBCI call C1FB aller chercher argument < 8

C517 47 Id b,a

C318 C9 ret

EEE HE HER HE HE MH HE HE HE HE HE HE HE HE EH HE DEN E HE Instruction Basic TAG

C319 CDDOCI call C1D0 Aller chercher numéro stream

C31C 3EFF Id a, FF

C31E 1804 jr C324

CHOHOHEOMEOME DE HE JE DE HE DE DE ME DE ME HE ME DE ME HE DE HE HE HE JE HE EH JE HE Instruction Basic TAGOFF

C320 CDDOCI call C1D0 Aller chercher numéro stream

C323 AF xor a

C324 C363BB jp BB63 TXT SET GRAPHIC

EE EL UE DS aller chercher 2 valeurs 8 bits non
nulles

C327 CD2FC3 call C32F aller chercher première valeur

C32A 53 ld d,e

C32B CD37DD call DD37 Tester si encore un caractère

CS2E: 20 db 2C da

C32F DS push de

id 2e

BASIC

C330 CD6DCE call CEGD

C333 D1 pop de

C334 SF ld e,a
C335 1D dec e

C336 C9 ret

C337 3E84 ld a,84
C339 3224AC ld (AC24),a
C33C E5 push hl

C33D CD9DC1 call C19D
C340 ET POP hl

C341 F5 push af

C342 ES push hl

C343 7E id a,(hl)
C344 23 inc hl

C345 B7 or a

C346 CAS6C3 cal] nz,C356
C349 20F8 ir nz,C343
C34B ET pop hl

C34C F1 pop af

C34D C9 ret

CHENE DE HE HER MEME DE HE DEEE HE EME EME EME DE JE HE JE DE EEE HE
C3HE F5 push af

C34F 3EOA Id a, OA
C351 CD56C3 call C356
C354 F1 pop af

C355 C9 ret

EHHHEHEHEHEOHHH HEC ME GE GE EHEME DÉEEDEHEHHE AE

C356 F5 push af

C357 CDSCC3 call C35C
C35A F1 pop af

C35B C9 ret

C35C FECA CP OA

C35E 200 TE nz, C36E
C360 3A21AC Id a, (AC21)

1.0

aller chercher valeur 8 bits non

nulle

sortir chaîne

132

WIDTH sur 132

adresse de début de la chaîne
sélectionner canal de sortie

aller chercher un caractère
augmenter pointeur

octet nul, donc fin de la chaîne
sortir un caractère

non nul, alors caractère suivant

sortir LF

LF
sortir

sortir un caractère

sortir un caractère

LF°?

numéro stream act.

DOiRES

C363
C365
C368
C36B

FE08

CAA8C3
D2EAC3
C392C3

CP
jp
jp
jp

BASIC

08
z,C3A8
nc, C3EA

- C392

MOUCHE HE DEHE DH DH DE HE DE DEEE EM MED EEHEEEK

C36E
C36F
C370
C371
C374
C375
C376

F5
cs
UF
CD77C3
C1
F1
c9

push
push
Id
call
pop
pop
ret

af
DC
c,a
C377
bc
af

HE ÉD DE HD HD DE DEEE MED DÉ EEE DEMO HE HE DEEE

C377
C37A
C37C
C37F
C382
C385

3A21AC
FE08
CAB5SC3
D2F8C3
79
C399c3

1d
CP
jp
jp
ld
jp

a, (AC21)
08
z,C3B5
nc, C3F8
a,C
C399

HE HEOHHE HE DE HE MEME HE EH OH DE HE EME ME EE EH HE NH

C386
C387
C38A
C38D
C390
C391
C392
C394
C397
C399

AF
CD63BB
CD54BB
CD9Cc3
3D

C8
3EOD
CD99c3
3E0A
C35ABB

xor
call
call
call
dec
ret
Id
call
Id
jP

a

BB65
BB54
C39C
a

Z

a,0D
C399
a, OA
BB5SA

ECHEMHE DEEE DEDE NEED DH HE HE NEED DE DE DE HE EE EDEN

C39C
C39D
C39E
C3AT
C3AU

C5
ES
CD78BB
CD8/BB
7C

push
push
call
call
id

bc
hl
BB78
BB87
a,h

1.0

imprimante?
oui
cassette
écran

sortir un caractère

caractère dans c
sortir

sélectionner courant de sorti
numéro stream act.

imprimante ?

cassette ?

caractère dans accu

sortir caractère sur l'écran

initialiser écran

TXT SET GRAPHIC
TXT VDU ENABLE
Curseur dans position autorisée

CR

sortir

LF

TXT OUTPUT

Curseur dans position autorisée

TXT GET CURSOR
TXT VALIDATE

=TIT 14-

BASIC

C3AS ET pop hl

C3A6 C1 POP bc

C3A7 C9 ret

HONOR JE MEME DEEE GE HE HE HE HE EH ECM GE EE EE HE
C348 C5 push bc

C3A9 CEOD id c,0D
C3AB CDB5C3 call C3B5
C3AE  OEOA 1d c,0A
C3BO CDB5C3 call C3B5
C3B3 C1 pop bc

C3B4 C9 ret

HOMME JE JM HE DE EH DE HE EH EE CHE EE CHE EEE HE
C3BS ES push fl

C3B6 79 Id a,C

C3B7 EEOD xor OD

C3B9 2813 ir z,C3CE
C3BB 79 1d a, C

C3BC FE20 CP 20

C3BE 3814 jr c,C3D4
C3C0 2A23AC 1d hl, (AC23)
C3C3 24 inc h

C3C4 JD 1d a, |

C3C5 2807 jr 2ACSCE
C3C7 BC CP h

C3C8 CCA8C3 call z,C3A8
C3CB 3A23AC 1d a, (AC23)
C3CE* :3C inc a

C3CF 2803 jr z,C3D4
C3D1 3223AC ld (AC23),a

1.0

sortir CR & LF sur l'imprimante

CR
sortir
LF
sortir

sortir caractère sur l'imprimante

CR

16’
ne pas compter caract. de contrôle
position imprimante act. et WIDTH

position imprimante act.

position imprimante act.

ALES

BASIC

C3D4 E1 POP hl
C3D5 79 Id a,C
C3D6 CD2BBD call BD2B
C3D9 D8 ret (e
C3DA CD3CC4 call C43C
C3DD 18F6 jr C3D5

MH DE DE HE HE EH JE DE HE MEME DEEE DEEE HE

C3DF 3A23AC Id a, (AC23)
CSE2: C0 ret

C3E3 CD6DCE call  CE6D
C3E6 3224AC Id (AC24), a
C3E9 C9 ret

EDEN DE D EH HE D D DD DE HHEHEENE HE HE ÉÉ
C3EA 3E01 1d a,01
CZEC 3225AC Id (AC25),a
C3EF 3E0D id a,0D
C3F1 CDODCH call  C4OD
C3F4  3EOA 1d a,OA
C3F6 1815 jr C4OD
OMEHOHEOHOHEHEOMEDE HE DE DH HE D HE DE EH HE DE HE DE HE
C3F8 ES push hl

C3F9 2125AC Id h1,AC25
C3FC 79 1d a,c

C3FD 0601 1d b,01
C3FF FEOD cp 0D

C4O1 2808 jr z, CUOB
C4O3 FE20 cp 20

CUOS 3805 jr c,C4OC
C4O7 46 1d b, (h1)
C4O8 O4 inc b

C4O9 2801 jr z, CHOC
CuOB 70 Id (h1),b
CHOC E1 pop hl

MC PRINT CHAR
sortie ok ?

interruption par 'ESC' ?

Position imprimante act.

Instruction Basic WIDTH
aller chercher valeur
nulle

fixer WIDTH

8 bits non

nouvelle ligne sur cassette

position cassette sur 1
CR

sortir sur cassette

LF

sortir sur cassette

position cassette

pour nouvelle ligne, position sur 1
CR

‘6!
ne pas compter caractères contrôle

charger le compteur de caractères
et l'augmenter

ranger nouvelle valeur compteur

-111 16-

BASIC

C4OD  CD95BC call BC95
CW10 D8 ret C

C411 C36BCB jp CB6B

OH OHOHOMOHOMEOHEHOH HE DE EEE HE GE M EEE JE NE
C414  C386BC jp RC86
LÉRRTSÉES ESS SSSSSSSSÉESSSSSLSSSSSSSS)
C417 ES push hl

C418 CD89BC call RC89
C41B 28F4 jr 2,041
CH1D 3F ccf

CUITE  9F sbc a,a

CHF CDOSFF call FFO5
Cu22 E1 pop hi

C423 C9 ret

He De DH DE HE HE DE DE DEEE DE JE JE JE EH

C424  3A22AC Id a, (AC22)
C427 FEO9 Cp 09

C429 CA8OBC jp z, BC80
C42C CDOSBB call BBO9
Cu2F D8 ret C

C430 CD81BB call BB81
Cu33 CDO6BB call BB06
Cu36 C38LBB jp BB84

HOME HE HE EME HE HE MEME EH HE ME EEE HE DE EEE

C439 C3O9BB jp BBO9
LÉRÉEESES RSS SSRSSSILSSRSSLSSILLSSSSS.
C43C CDO9BB call BB09
C43F DO ret nc
C44O FEFC CP FC
C442 CO ret nz
Cuyz3 C5 push bc
Ca DS push de
CUS ES push hl
C4U6  CD6FC4 call CU6F
C449  DAG6BCB jp c, CB6B

1,0

CAS OUT CHAR

pas appuyé touche ESC ?
‘Break’, mode READY

CAS RETURN

Variable réservée EOF
CAS TEST EOF

ESC enfoncée ?

accepter signe comme nombre entier

aller chercher un caractère dans canal d'entrée

canal d'entrée
cassette ?

oui, CAS IN CHAR
KM READ CHAR
Touche enfoncée ?
TXT CUR ON

KM WAIT CHAR

TXT CUR OFF

KM READ CHAR

Tester si interruption avec ‘ESC’
KM READ CHAR

Break’ ?

attendre seconde frappe de touche
"ESC', alors interruption

-III 17-

Cauc
CHF
C450
C451
C452

CD53C4
ET
D1
C1
C9

call
pop
pop
pop
ret

BASIC

C453
pl
de
DC

HCHOHOMOMEOHOHE EME HE HE HE EME EH HE HOMME HE HE JE HE EEE

C453
C4Su
Cu57
ca5g
C45C
CUSD

ES
115EC4
OEFD
CD4SBB
ET

C9

push
ld
ld
call
pop .
ret

hl

de, CUSE
c,FD
BB45

nl

HOME DE EH ME HE HE HE DE HE MEME HE ME HE EME EHE ÉEHEH

CUSE
CUSF
Cu62
Cu64
Cu66
C468
CU6B
C46C

HOHHHOHEOHEHEHEHE HD ME EH HHE EH HOME DEEE

CUGF
C472
C473
Cu76
Cu78
CU7A
C47C
CU7E
C480
Cu83
C48u
Cu87
Cu88

cu8g

ES
CDO9BB
3004
FEEF
20F7
CD6FC4
ET
C347C8

CDB6BC
F5
CD30C4
FEEF
28F9
FEFC
280B
FE20
CUOCBB
F1
DCB9BC
B7

C9

F1

push
call
Jr
CP
jr
call
pop
jp

call
push
call
CP
jr
Cp
Jr
CP
call
POP
call
or
ret

Pop

fl

BB09
nc, C468
EF
nz,C4SF
C46F

fl

C847

BCB6
af
C430
EF
z,C473
FC
z,C48g
20

nz, BBOC
af

c, BCB9
a

af

1.0

autoriser interruption par ‘Break

autoriser interruption par ‘Break'

Adresse de la routine Break-Event
BASIC-ROM sélectionnée
KM ARM BREAK

routine Break-Event

KM READ CHAR

aucune touche enfoncée ?

Break par ’ESC' 7?

ignorer touches frappées avant ‘ESC’
attendre un second ’ESC'

Tester si ON BREAK GOSUB

attendre frappe d'une touche après ESC’

SOUND HOLD

attendre frappe d’une touche
Break par 'ESC' ?

‘Break’ ?

"6" ?
non, ranger caractère KM CHAR RETURN

SOUND CONTINUE

-[11 18-

CL8A
C48B

HORDE HE EH HE HE EH HE HE EEE EEK Xe

C48c
C48F
C490
c491
C494
C496
cugg
CU9A
Cu9B

37
(ae!

CDTACS
C5

D5
CDS5DD
3018
CDIACS
C5

D5
CD37DD

“amstrad 5"

CUSE
CU9F
CHA2
CHA3
C4AU
CUA7
C4AB
C4A9
CHAA
CHAD
CUAE
CUAF
C4BO
C4B3
CuB4

C4B5
CuB8
CUBA
CuBD
C4Co
C4CT
caca
CaCS

dede De He HE EH HE HE EME EME GE HE HE DE HE JE HE DE HER HEEHE

2C
CDIACS
C5
E3
CDD2BB
E1
D1
E3
CDCFBB
ET
D1
E3
CDC9BB
ET
C9

CD51DD
3806
CDABC2
CDE4BB
E5
CDDBBB
ET

cg

scf
ret

call
push
push
call
jr

call
push
push
cal]

db
call
push
ex
call
pop
pop
ex
call
pop
pop
ex
call
pop
ret

call
jr
call
call
push
call
pop
ret

BASIC

CSTA

DC

de

DD55
nc, CUAE
C51A

bc

de

DD37

2C

C51A

bc
(sp),hl
BBD2

fl

de
(sp),hl
BBCF

hl

de
(sp),hl
BBC9

hl

DD51
c,C4CO
C24B
BBE4
hl
BBDB
hl

1.0

Instruction Basic ORIGIN
aller chercher 2 arguments

Virgule suit ?
Non
aller chercher 2 arguments

Test auf nachfolgendes Zeichen

aller chercher 2 arguments

GRA WIN HEIGHT

GRA WIN WIDTH

GRA SET ORIGIN

fin de l'instruction ?

oui

aller chercher argument < 16
GRA SET PAPER

GRA CLEAR WINDOW

Instruction Basic DRAW

-III 19-

BASIC

C4C6 O1FGRB id bc, BBF6
C4C9 180 jr CuD8
HORREUR RE NORD D HE DE DE DE EM EE
C4CR 01FS9BB Id bc, BBF9
C4CE 1808 ÏE C4D8
HOME DEDDEDE MNDDEDE É DEDE HE DEHÉEE
C4DO O1EABB ld bc, BBEA
C4D3 1803 jr C4D8

HD DOHE DE DE DEMO HEDEDDEHE HE HE  É DHÉHÉMHEHÉ  EH HE É e
C4DS O1EDBB ld bc, BBED
C4D8 CS push bc

C4DS CDIACS call CS1A
C4DC CDSSDD call DD55
C4DF 3006 PE nc, CUE7
CUE1T  CD4BC2 call C24B
CUEU  CDDEBB call BBDE

CHE7 1828 jr C511
HOMME HE JE HE HE JE D EE HE HE HE He HE HE DH De DE HE EE HE EE
CUES 01FOBB ld bc, BBFO
CHEC 1803 jr C4F1
CREER EE LE SE
CUEE 01F3BB Id bc, BBF3
C4FT CS push bc

CUF2  CDIACS call C51A
CUFS  CD37DD call DD37

CUF8 29 db 29
C4FQ E3 ex (sp),hi
CUFA C5 push bc
CUFB E3 ex (sp),hl
CUYFC C1 pop DC

CUFD CDF9FF call FFF9
C500  CDOAFF call FFOA

C503- "E1 pop hi
C504 C9 ret

1.0

GRA LINE ABSOLUTE

Instruction Basic DRAWR
GRA LINE RELATIVE

Instruction Basic PLOT
GRA PLOT ABSOLUTE

Instruction Basic PLOTR
GRA PLOT RELATIVE

aller chercher 2 arguments
virgule suit ?

non

aller chercher argument < 16
GRA SET PEN

TEST
GRA TEST ABSOLUTE

TESTR
GRA TEST RELATIVE

aller chercher 2 arguments
tester si encore un caractère
DE

jp (bc), exécuter fonction
accepter contenu accu comme nombre
entier

=[1E 20-

HER EH DE HN ME EME DE ME GE MEME GENE EE GENE GE DE EN

BASIC 1.0

Instruction Basic MOVE

C505 01COBB ld bc, BBCO GRA MOVE ABSOLUTE

C508 1803 jr C50D

HOMME EE HHHOHHEHEHE HOME EE EME DE HE HCH EE Instruction Basic MOVER

C50A O1C3BB ld bc, BBC3 GRA MOVE RELATIVE

CS0OD C5 push bc

CSOE  CDTACS call CS1A aller chercher 2 arguments

EST: ES ex (sp),hl

C512 C5 push bc

C513 E3 ex (sp),hl

C514 C1 pop bc

C515 CDF9FF call FFF9 jp (bc), exécuter fonction

C518 El pop hi

C519 C9 ret

RASE aller chercher deux arguments entiers dans de, bc

CS1A CD86CE call CE86 aller chercher valeur 16-bits -32768

- +32767

C51D DS push de

CSIE CD37DD call DD37 Tester si encore un caractère

C521 2C db 2C Het

C522  CD86CE call CE86 aller chercher valeur 16-bits -32768

- +32767

C525 42 id b,d

C526 4B Id C;e 2ème argument dans bc

C527 D1 pop de 1r argument

C528 C9 ret

HORDE HE ECM JE JE JE He HE HE EE DE EEE EE EE GE GENE Instruction Basic FOR

C529 CDB3D6 call D6B3 lire variable

C52C: ES push hl

C52D C5 push bc

C52E D5 push de

C52F  CDCSC9 call C9C5 chercher NEXT correspondant

C532 222CAC ld (AC2C),h] ranger adresse

C535 DS push de

C536 ES push hl

C537 EB ex de,hl

C538 CD32C6 call C632 chercher boucle FOR-NEXT ouverte

SEAT AE

C53B
CS3E
C53F
C5u2
C545
C548
C549
C54A
C5UB
CSC
C5UD
CSUE
C551
C554
C555
C558
C559
C55C
C55D
C55E
C55F
C560
C561
C562
C565
C566
C568
C56B
C56C
C56E
C570

C573
C574
C577
C578
C579
C57A
C57B
C57C

CCACF5
EI
CD51DD
110000
D486D6
44

UD

ET

E3

7A

B3
CUBSFF
C2F6C5
EB
CDD2DD
E3
CDCEDD
EI

F1

E3

D5

C5

E5
010516
B9
280B
010210
B9
2805
1E0D
C394CA

78
CDBOFS5
73
23
72
23
E3
CD37DD

call
pop
call
ld
call
ld
id
pop
ex
ld
or
call
jp
ex
call
ex
call
pop
pop
ex
push
push
push
id
CP
Jr
ld
CP
Jr
Id
JP

id
call
id
inc
Id
inc
ex
call

BASIC

Z,F5SAC
hl

DD51
de, 0000
nc, D686
b,h

C1

hl
(sp),hi
a,d

e
nz,FFB8
nz, C5F6
de,hl
DDD2
(sp),hl
DDCE

hl

af
(sp),hl
de

bc

hi
bc,1605
C
z,C573
bc, 1002
C
z,C573
e,0D
CA94

a,D
FSBO
(hl),e
hl
(h1),d
hl
(sp),hl
DD37

1,0

trouvé, fixer pointeur de pile Basic
fin de l'instruction ?

zéro par défaut
non, aller chercher variable

comparer hl <> de
’Unexpected NEXT'’

adresse de ligne actuelle dans hl

fixer adresse de ligne actuelle

22 octets, type 5 ’Real'

16 octets, type 2 ‘Integer’

‘Type mismatch'
sortir message d'erreur

réserver place dans pile Basic

adresse de variable sur pile Basic

Tester si encore un caractère

-III 22-

C57F
C580
C583
C584
C587
C588
C58B
C58E
C58F
C592
C593
C596
C597
C598
C59B
C59E
C59F
C5AO
CSA
C5SAU
C5A7
C5A8
C5A9
CSAB
CSAD
C5BO
CS5B3
C5B4
C5B7

EF
CDFBCE
79
CDD/FE
E5
2127AC
CD62FF
E1
CD37DD
EC
CDFBCE
E3

79
CDD/FE
CD62FF
EB

E3

EB
210100
CDODFF
EB

7E
FEE6
2006
CD3FDD
CDFBCE
79
CDD/FE
ES

“amstrad 6"

C5B8
C5BB
C5BE
C5BF
C5CO
C5C1
C5C2
C5C3

C5C6

CD62FF
CDA3FD
EB
77
23
EB
ET
CDAADD

EB

call
1d
call
push
1d
call
pop
call
db
call
ex
ld
call
call
ex
ex
ex
ld
call
ex
ld
Cp
jr
call
call
id
call
ex

call
call
ex

ld
inc
ex
pop

call

ex

BASIC 1.0

EF "=!
CEFB aller chercher expression
a,C
FED7 comparer type de variable
hl
h1,AC27 mémoire provisoire pour variable FOR
FF62 copier variable dans h]
hl
DD37 Tester si encore un caractère
EC "TO"
CEFB aller chercher expression
(sp},hl
a,C
FED7 comparer type de variable
FF62 valeur finale sur pile Basic
de,hl
(sp),hl
de,hl
h1,0001 un comme valeur STEP par défaut
FFOD accepter nombre entier hl
de,hl
a, (hl)
E6 "STEP"
nz, C5B3
DD3F ignorer espaces
CEFB aller chercher expression
a,C
FED7 Comparer type de variable
(sp),hl
FF62 copier variable dans (hl)
FDA3 aller chercher signe
de,h]
(hl),a signe de STEP sur pile Basic
hl
de,hl
hi
DD4A fin de l'instruction, sinon ‘’Syntax
error’
de,h]

-III 23-

C5C7
C5C8

C5C9
C5SCA
CSCB
C5CC
C5CF
C5D0
CSD

C5D2
C5D3
CSD4
CSD5
CSD6

CSD7
C5D8
C5D9
C5DD
C5DE

CSDF
C5EO
CSE1

CSE2
CSE3
CSE6
CS5E9
CSEA
CSED
CSEE
C5F1
CSF4

C5F6
C5F8

73
23

72
23
EB
CDD2DD
EB
73
23

2
23
Di

73
23

72
23
ED5B2CAC
75
25

72
23
70

D1
2127AC
CD66FF
AF
3226AC
ET
CDCEDD
2A2CAC
1804

1E01
C394CA

ld
inc

1d

inc

ex

call

ex

Id
inc

ld
inc
POP
ld
inc

Id

inc

1d

1d
inc

Id
inc
ld

pop
Id
cal]
xor
1d
POP
call
ld
Jr

Id
JP

BASIC

(hl),e
hi

(h1),d
hl
de,hl
DDD2
de,hl
(hl1},e
nl

(h1),d
pl
de
(hl),e
hi

(hl),d
h]
de, (AC2C)
(hl),e

hl

(h1),d
hl
(h1),b

de
hl,AC27
FF66

a
(CAC26),a
hl

DDCE

h1, (AC2C)
C600

e,01
CA94

1.0

Adresse de l'instruction FOR sur la
pile Basic

Adresse de ligne act. dans hl

Adresse de ligne de FOR sur pile
Basic

Adresse de l'instruction NEXT sur
pile Basic

adresse de l'instruction NEXT sur
pile Basic

&10 ou 816 pour Integer/Real sur

pile

pointeur sur mémoire provisoire
aller chercher variable FOR

Flag pour premier parcours
fixer adresse de ligne act.
à l'instruction NEXT

’Unexpected NEXT'
sortir message d'erreur

-IIT 24-

BASIC

ERRONÉE HE DE EME EME DE ME DE ME DE HE HE DE EEE

CSFB 3EFF 1d a,FF
C5SFD 3226AC id (AC26),a
C600 EB ex de,hl
C601 CD32C6 call C632
C604 20F0 jr nz,C5F6
C606 EB ex de,hl
C607 CDACFS call F5SAC
C60A EB ex de,hi
C60B E5 push hl

C60C CD61C6 call C661
C60F  280F jr z, C620
C611 F1 pop af

C612 23 inc hl

C613 SE ld e,(hl)
C614 23 inc hl

C615 56 ld d,(h1)
C616 23 inc hl

C617 7E 1d a,(h1l)
C618 23 inc hl

C619 66 ld h, (h1)
C61A 6F Id 1,a
C61B CDCEDD call DDCE
CGiE EB ex de,hl
C61F C9 ret

C620 010500 id bc, 0005
C623 09 add hl,bc
C624 GE Id e,(hl)
C625 23 inc hl

C626 56 ld d,(h1)
C627 El pop hl

C628 CDACFS call F5SAC
C62B EB ex de,hl
C62C CD55DD call DD55
C62F 38CF jr c, C600
C631 C9 ret
LÉELLLLÉSLLLLLLLELLLLLLLLLLLLLLLLL)
C632 2A8BB0 ld h]1, (B08B)

1.0

Instruction Basic NEXT
flag pour additionner incrément

chercher boucle FOR-NEXT ouverte
‘Unexpected NEXT'

fixer pointeur de pile Basic

Tester si fin de boucle

pointeur de programme dans de

adresse de ligne dans h1

fixer adresse de ligne act.

pointeur de pile Basic
plus 5

pointeur de programme dans ’NEXT'

fixer pointeur de pile Basic
virgule suit ?
oui, prochaine boucle NEXT

chercher boucle FOR-NEXT ouverte
Pointeur de pile Basic

-1I1 25-

C635
C636
C637
C638
C639
C63A
C63B
C63C
C63D
C63E
C63F
C640
C641
C643
C645
C647
C649
C64B
C64D
CGUE
C64F
C650
C651
C652
C653
C654
C657
C658
C65A
C65B
C65C
C65D

C65E
C65F

C661
C662
C663
C664

ÈS
2B
46
23
7D
90
6F
9F
84
67
E3
78
FEO7
2819
FE10

2804
FE16
200D
E5
2B
2B
7E
2B
6E
67
CDB8FF
E1
2004
EB
El
78
cg

E1
18D4

5E
23
56
23

push
dec
Id
inc
Id
SUD
1d
sbc
add
ld
ex
Für:
CP
jr

Jr
CP
jr
push
dec
dec
ld
dec
id
id
call
pop
Jr
ex
pop
ld
ret

pop
Jr

ld
inc
id
inc

CP

hl
hl
b, (h1)

(sp),h
a,b

07
z,C65E

z,C64D
16

nz, C65
hl

hl

hl

a, (hl)
hl
1,(h1)
h,a
FFB8
h]

nz, C65
de,hl
hl

a,b

hl
C635

e,(h1)
hl
d, (h1)
hl

BASIC 1.0

1

WHILE-WEND' ?

10 Integer
Real ’FOR-NEXT' ?
À
comparer hl <> de
E

-I11 26-

!FOR-NEXT"

?

C665
C667
C669
C66A
C66D
C66E
C66F
C672
C673
C676
C677
C679
C67A
C67B
CG7E
C67F
C680
C681
C682
C683
C684
C685
C688
C689
C68A
C68C
C68F
C690
C693
C694
C695

C696
C697
C698

FE10
282D
ES
010500

79

EB
CDUBFF
E1
3A26AC
B7
2810
E5

09
CDCCFC
ET

E5

2B

56

2B

SE

EB
CD62FF
E1

ES
0E05
CDO9FD
EI
010400
09

96

C9

E5
EB
5E

Cp
jr
push

id
ex
call
pop
ld
or
jr
push
add
call
pop
push
dec
Id
dec
ld
ex
call
pop
push
1d
call
pop
Id
add
sub
ret

push
ex
ld

BASIC

10
z,C696
h1
1d

a,C
de,hl
FF4B
h1

a, (AC26)
a
z,C689
h1
hl,bc
FCCC
hl

hl

hl

d, (h1l)
hl
e,(h1l)
de,hl
FF62
hl

hl
c,05
FDO9
h1

bc, 0004
hl,bc
(h1)

hl
de,hl
e,(hl)

1.0

Integer ?

bc, 0005 Type sur

accepter variable et type
flag pour premier parcours

oui, sauter addition

additionner valeur STEP

copier variable dans (hl)

comparaison arithmétique

10

-[11 27-

Real”

C699
CG9A
C69B
CGE
C69F
C6A1
C6A2
C6A3
C6A4
CG6AS
C6A6
C6A7
C6A8
C6A9
CGAC
CGAE

C6B1
C6B2
C6B3
C6B4
C6B5
C6B6
C6B7
C6B8
C6B9
C6BA
C6BB
C6BC
C6BD
CGBE
C6C1
C6C2
C6C5
C6C4
C6C5
C6C6

23

56
3A26AC
B7
2816
E3

E5

23

23

7E

25

66

6F
CDACBD
1E06
D294CA

EB
ET
E3
72
2B
75
ET
7E
23
ES
66
6F
EB
CDCuBD
ET
23
23
23
96
C9

inc
1d
ld
or
Jr
ex
push
inc
inc
ld
inc
ld
id
cal]
ld
Jp

ex
pop
ex
Id
dec
1d
Pop
Id
inc
push
Id
ld
ex
call
pop
inc
inc
inc
SUD
ret

BASIC

hl

d, (h1l)
a, (AC26)
a
z,C6B7
(sp),hl
hi

hl

hl

a, (hl)
hl

h, (h1)
1,a
BDAC
e,06
nc, CA94

de,hl
hl
(sp},h1
(h1),d
hl
(hl),e
pl
a,(hl)
hi

nl

h, (h1)
1,a
de,hl
BDC4
hl

hl

hl

hi
(hl)

UND DEEE DEEE DE DEEE DE EDEN DE DE DE HE DE HE DE DEEE RE DE NE

C6C7

CDFBCE

call

CEFB

1.0

premier parcours ?

oui, sauter addition

aller chercher valeur STEP dans h1

Integer-Addition hl := hl + de
'Overfliow'
sortir message d'erreur

comparer Integer

Instruction Basic IF
aller chercher expression

-]11 28-

CGCA
C6CC
C6CE
C6D1
C6D2
C6D3
C6D6
C6D7

C6DA
C6DB
C6DE
C6DF
C6E1
CGE3
C6E5

FEAO
2804
CD37DD
EB

E5
CDA3FD
Eî
CC9FES8

C8
CD51DD
D8
FETE
2805
FE1D
C2ABDD

CP
Jr
call
db
push
call
pop
call

ret
cal}
ret
Cp
Jr
CP
jp

BASIC

AO

z, C6D2
DD37
EB

hi
FDA3
h}
Z,E89F

Z

DD51

C

1E

zZ, C6E8
1D

nz, DDAB

HOHOHEHE DEN DE HEME HE EE DE DENON D DE DE DE HE DD DEHE DEN DEN

CGE8
CGEB

CGEC

CD67E7
EB

cg

call
ex

ret

E767
de,hi

EDEN DEEE EDEN DEMO MH DE DE HÉDE DE DE HE DH NE EE DEN

CGED
C6FO

C6F3
C6F4
C6F6
C6F7
C6F9
C6FC
CGFD
CGFE
CGFF
C700
C701
C702
C703

CD67E7
CDEFE8

EB
0E00
ES
3E06
CDBOF5
71

25

73

25

72

23

EB
CDD2DD

call

call

ex
1d
push
1d
call
id
inc
1d
inc
1d
inc
Ex
call

E767
E8EF

de,hl
c,00
hi

a, 06
F5BO
(h1),c
hl
(hl),e
hl
(h1),d
hi
de,hl
DDD2

1.0

GOTO"

Tester si encore un caractère
‘THEN’

aller chercher signe

chercher fin de ligne ou branchement
ELSE

fin de l'instruction ?

oui

Numéro de ligne ?

Oui, zum GOTO-Befehl

adresse de ligne ?

non, exécuter instruction Basic

Instruction Basic GOTO

aller chercher adresse de ligne
accepter adresse comme pointeur de

programme

Instruction Basic GOSUB

aller chercher adresse de ligne
instruction DATA, ignorer le reste

de la ligne d'instruction

marque pour ’GOSUB’ normal
ranger adresse du sous-programme
6 octets

réserver place dans pile Basic
zéro

Adresse instruction après ‘GOSUB'’
sur pile Basic

actuelle adresse de ligne dans h1

-III 29-

C706
C707
C708
C709
C70A
C70B
C70D

C70E

EB
73
23
72
23
3606
ET

C9

ex

ld

inc

ld

inc

ld
pop

ret

BASIC

de,hl
(hl),e
h]
(hl),d
hl
(h1),06
hi

HDI DEHHEDEHEDE DE DE ED DE ED DD DE HÉDEDE DE DE DEDE DE D

C70F
C710
C713
C716
C717
c718
C719
C71A
C71B
C71C
C71D
C71E
C71F
C720
C723
C724
C725
C727
C728
C72B

CO
CD2EC7
CDACFS
UE

23

5E

23

56

23

7E

23

66

6F
CDCEDD
EB

79
FEO1
D8
CAAUC8
C3B6C8

ret
call
call
ld
inc
1d
inc
Id
inc
id
inc
Id
ld
call
ex
Id
cp
ret
JP
JP

nz
C72E
FSAC
c,(hl)
hl
e,(hl)
hl
d,(h1)
hl

a, (hl)
hl

h, (h1)
1,a
DDCE
de,hl
a, C

01

C
z,C8A4
C8B6

HN DE HEDEDEHEHEDEHEHE DE MED DE DE DEEE DEEE NE DEEE

C72E
C731
C732

C733
C734
C735

2A8BB0
2B
7E

F5
7D
96

ld
dec
Id

push
ld
sub

hl, (BO8B)
hl
a, (h])

af
a,
(h1)

1.0

Adresse de ligne sur pile Basic

marque pour ‘GOSUB'
pointeur de programme sur sous-
programme

Instruction Basic RETURN

chercher GOSUB sur Pile Basic
réinitialiser pointeur de pile Basic
octet-marque

aller chercher adresse de
l'instruction après ‘GOSUB' dans de

Adresse de ligne dans h]

fixer numéro de ligne actuel
pointeur de programme actuel dans hl
octet distinctif

inférieur à 1 ?

oui, GOSUB normal

un, alors GOSUB après AFTER/EVERY

Pointeur de pile Basic

aller chercher marque das pile
Basic

-111 30-

BASIC

C736 6F ld l,a
C737 SF sbc a,à
C738 84 add a,h
C739 67 ld ha
C73À 23 inc hl

C73B F1 pop af
C73C FE06 CP
C/3E C8 ret Z

C73F B7 or a

C740 20FF jr nz,C731
C742 1E03 1d e,03
C744  C394CA JP CA94
DE HE DE DE HE HE HOME GE DEEE DEEE HEHOEHHMEEDE HE EEE
C747 ES push hi

C748 CDI8CA call CA18
C74B ES push pl

C74C EB ex de,hl
C74D 222EAC 1d (AC2E),hl
C750 CDB8C7 call C7B8
C753 CCACFS call Z,F5SAC
C756 3E07 id a,07
C758  CDBOFS5 call FSBO
C75B EB ex de,hl
C75C CDD2DD call DDD2
C75F EB ex de,hl
C760 73 1d (hl),e
C761 23 inc hl

C762 72 Id {hl1),d
C763 23 inc nl

C764 Di pop de
C765 73 Id (hl),e
C766 23 inc hl

C767 72 Id (h1),d
C768 23 inc nl

C769 EB ex de,hi
C/6A E3 ex (sp),hl
C76B EB ex de,hl
C76C 73 1d (hl),e
C/76D 23 inc hl

1.0

restaurer pointeur de pile Basic

06 GOSUB'

‘Unexpected RETURN’
Sortir message d'erreur

Instruction Basic WHILE

chercher WEND correspondant
ranger adresse

Adresse de ligne pour WHILE-WEND
fixer pointeur de pile Basic
7 octets

réserver place dans pile Basic

actuelle adresse de ligne dans hl

Adresse de ligne sur pile Basic

Adresse après ‘WEND’ sur pile Basi

Adresse de condition ‘’WHILE'

SILL31>

C76E
C76F
C770
C772
C773
C774

72
23
3607
EB
D1
182A

Id
inc
ld
ex
pop
jr

BASIC

(h1),d
nl
(h1),07
de,hl
de

C7A0

EOOEOHOHE DE DE HE DE DE DE EE DE DE DEEE HE HE HE EE HE EE EEE

C776
C777
C778
C77B
C77D

C780
C781
C784
C785
C788
C78B
C78E
C78F
C790
C791
C792
C793
C794
C797
C798
C799
C79A
C79B
C79C
C79D
C79Æ
C79F
C7A0
C7A1
C7A4
C7AS5

CO

EB
CDB8C7
1E1E
C294CA

ES
110700
19
CDACFS
CDD2DD
222EAC
E1
5E
23
56
25
EB
CDCEDD
EB
5E
25
56
23
7E
23
66
6F
D5
CDFBCE
ES
CDA3FD

ret
ex
call
1d
JP

push
ld
add
cal]
call
Id
pop
Id
inc
Id
inc
ex
call
ex
Id
inc
1d
inc
Id
inc
ld
Id
push
call
push
call

nz
de,hi
C7B8
e,1E
nz, CA94

hl

de, 0007
hl,de
FSAC
DDD2
(AC2E),h1
fl
e,(hl)
hl
d,(h1)
hl
de,hl
DDCE
de,hl
e,(hl)
hl
d,(h1l)
hi

a, (hl)
hi

h, (h1)
],a

de
CEFB
hl
FDA3

1.0

sur pile Basic

marque pour "WHILE'

tester condition “WHILE'

Instruction Basic WEND

‘Unexpected WEND'
Sortir message d'erreur

hl comme pointeur de pile Basic
actuelle adresse de ligne dans h1
Adresse de ligne pour WHILE-WEND

fixer actuelle adresse de ligne

aller chercher expression

aller chercher signe

-111 32-

C7A8
C7A9
C7AA
C7AB
C7AE

C7B1
C7B3
C7B6
C7B7

ET

D1

CO

2A2EAC
CDCEDD

3E07
CDAOF5
EB

C9

pop

pop

ret

ld
call

Id
call
ex
ret

BASIC

hl

de

nz

hi, (AC2E)
DDCE

a, 07
F5AO
de,hl

EHOHOHOHOHE HE HEH HE HE HE HE DE DEEE HE DEEE MED DE HE DE DE DENON HE

C7B8
C7BB
C7BC
C7BD
C/BE
C7BF
C7CO
C7C1
C702
C7C3
C7C4
C/C5
C7C6
C7C8
C7CA
C7CC
C/CE
C7D0
C7D2
C7D3
C7D4
C7D5
C7D6
C7D7
C/D8
C/D9
C7DC
C7DE

2A8BB0
2B
E5
7D
96
6F
9F
84
67
23
E3
7E
FE10
2816
FE16
2812
FEO7
200€
2B
28
2B
7E
2B
6E
67
CDB8FF
2002
E1

1d
dec
push
id
sub
ld
sbc
add
id
inc
ex
1d
CP
jr
Cp
Jr
cp
Jr
dec
dec
dec
Id
dec
1d
ld
call
jr
pop

h1, (BO8B)
hl

nl

a, |
(h1)
1,a

a,a

a,h

h,a

hl
(sp},hl
a, (hl)
10

Z, C7EO
16

Z, C7EO
07

nz, C7/DE
h!

hi

hl

a, (hl)
hl
1,(h1)
h,a
FFB8
nz, C7EO
hl

1.0

condition remplie ?

Adresse de ligne pour WHILE-WEND
fixer comme actuelle adresse de

ligne

libérer place dans pile Basic

Pointeur de pile Basic

Integer "’FOR-NEXT'
Real ‘’FOR-NEXT'

WHILE-WEND"

comparer hi <> de

-111 33-

BASIC 1.0

C7DF C9 ret

C/E0 El pop hl

C/E1 18D8 Jr C7BB

LÉLLLLLLSLLLILISSLSSSSSISISE ESS TSS Instruction Basic ON

C/ZE3 FE9C CP gC lERROR'

C7ES CAESCB jp z,CBES

C7E8 CD67CE call CE67 aller chercher valeur 8 bits
C/EB 4F id c,à comme compteur dans c

C7EC 46 ld b,(h1) aller chercher token

C/ED 78 id a,b

C7EE FEAO cp AO GOTO"

C7FO 2805 jr z,C7F7

C/F2 CD37DD call DD37 Tester si encore un caractère
C7F5S SF db 9F ! GOSUB'

C7F6 2B dec hl

C7F7 OD dec C diminuer compteur

C7F8 78 id a, D Token dans a

C/FQ9 CAABDD jp Z, DDAB exécuter instruction Basic
C/FC  CD3FDD call DD3F Ignorer les espaces

C/FF  CDEICE call CEE] aller chercher numéro de ligne dans
de

C802 FE2C cp 2C tt

C804 28F1 Jr Z, C7F7 prochain numéro de ligne
C806 C9 ret

LRLELLLLLSISS LS LLSLELSELSL LL LL LS) Traitement des Events (AFTER/EVERY)
C807 AF xor a

C808 3230AC 1d (AC30),a

C80B CDFBBC call BCFB KL NEXT SYNC

C80E 301D Jr nc, C82D y a-t-il un évènement ?

C810 47 ld b,a ranger priorité

C811 3A3OAC id a, (AC30)

C814 E67F and 7F annuler bit 7

C816 3230AC id (AC30),a

C819 C5 push bc

C81A ES push hl Adresse du bloc Event

C81B CDFEBC call BCFE KL DO SYNC

C81E E1 pop hi

-I11 34-

C81F
C820
C823
C824
C825
C826
C829
C82A
C82B
C82D
C830
C832
C835
C838
C83B
C83D
C83E
C83F
C842
C843
Cut

C1
3A30AC
17

F5

78
D4OTBD
F1

17
30DE
3A30AC
E604
C453C4
2ASUAE
3A30AC
E603
C8

1F
DAGBCB
23

F1
C393DD

POP
Id
rla
push
id
call
pop
rla
jr
ld
and
call
id
ld
and
ret
rra
jp
inc
pop
jp

BASIC

bc
a, (AC30)

af
a,b
nc, BD01
af

nc, C80B
a, (AC30)
04
nz,C453
h1, (AE34)
a, (AC30)
03

Z

c,CB6B
h1l

af
DDg3

HOHEHEOHDEHEHHE DE DEOHE HE DEEE DH HEHE DE OHEHE DEEE HE HE NEH

C8u7
C8uA
C8uc
C8UE
C851
C852
C853
C856
C858
C85A
C85D
C85F

2236AC
3E04
3050
2A3UAC
7C

B5
CuD6DD
3E4
3044
1131AC
0E02
1825

Id
Id
jr
ld
ld
or
call
ld
jr
ld
id
Jr

(CAC36),h]
a,04

nc, C8JE
h1, (AC34)
a,h

1

nz, DDD6
a,41

nc, C8
de, AC31
c,02

C886

EDEN DE DEN DE DEEE DD DEDEDEDE NEED DE DEEE D DE DE DE DE DE DE

C861
C862
C865

D5
CD37DD
SF

push
call
db

de
DD37
9F

1.0

KL DONE SYNC

prochain Event

autoriser interruption par ‘ESC’

Adresse de l'instruction actuelle

‘Break’

à la boucle de l'interpréteur

adresse ON-BREAK

Numéro de ligne dans hl

mode direct ?

Tester si encore un caractère

* GOSUB"

-111 35-

BASIC 1,0

C866 CD67E7 call E767 aller chercher adresse de ligne

C869 42 ld b,d

C86A 4B id c,e

C86B CD61DD call DD61 ignorer espace, TAB et LF

C86E D] pop de

C86F E5 push h}

C870 210400 ld h1,0004 10

C873 19 add hl,de

C874 71 ld (hl),c

C875 23 inc hl

C876 70 ld (h1),b

C877 El pop hl

C878 C9 ret

DEHEDEDEMEDEDEDE DE DEEE DE DD HE HE HN DD DEEE HE DEEE Routine Event

C879 23 inc hl

C87A 23 inc hl

C87B 23 inc hl

C87C EB ex de,hi

C8/7D CDD6DD call DDD6 aller chercher numéro de ligne mode
direct ?

C880 3E40 ld a,40

C882 301A jr nc, C89E oui

C884 0E01 ld c,01 octet distinctif pour AFTER/EVERY

GOSUB

C886 DS push de

C887 CDF6C6 call C6F6 instruction GOSUB

C88A 2A3UAE id hl, (AE34) Adresse de l'instruction actuelle

C88D EB ex de,hl

C88E El pop hl

C88F 70 ld (h1),b

C890 23 inc hl

C891 73 Id (h1l),e

C892 23 inc hl

C893 72 ld (h1),d

C894 23 inc hl

C895 5€ ld e,(hl)

C896 23 inc hl

C897 56 Id d,(h1)

C898 EB ex de,hl

-I11 36-

BASIC

C899 2234AE ld (CAE34),h1
C89C 3EC2 ld a, C2
C8 2130AC 1d h1, AC30
C8A1 B6 or (h1)
C8A2 77 ld (hl),a
C8A3 C9 ret

C8A4 7E ld a, (hl)
C8A5 23 inc hl

C8A6 GE Id e,(hl)
C8A7 23 inc hl

C8A8 56 ld d,(h1)
C8A9 D5 push de

C8AA O1F7FF ld bc,FFF7
C8AD 09 add h1,bc
C8AE CDO1BD call BDO1
C8B1 Ei pop h]

C8B2 F1 pop af

C8B3 C374DD Jp DD74
C8B6 7E 1d a, (hl)
C8B7 2A36AC ld Nh1, (AC36)
C8BA O1FCFF ld bc,FFFC
C8BD 09 add hl,bc

C8BE CDO1BD call BDO1
C8C1 CD53C4 call C453

C8C4 2A32AC Id h1, (AC32)
C8C7 F1 pop af

C8C8 C374DD Jp DD74
OUCHOHEOHODEONOMONE HE DE DE DEN DE DE MD DE HE DE NE DE HEH ÉD NE
C8CB FECE CP CE

C8CD 110000 ld de, 0000
C8D0 2808 jr z, C8DA
C8D2 CD37DD call DD37

C8D5 9F db 9F

C8D6 CD67E7 call E767

C8D9 2B dec hl

C8DA EDS334AC J]d (AC34),de
C8DE C33FDD jp DD3F

1,0

Adresse de l'instruction actuelle

KL DONE SYNC

à la boucle de l'interpréteur

KL DONE SYNC
autoriser interruption par ‘Break’

à la boucle de l'interpréteur

Instruction Basic ON BREAK
STOP'
valeur défaut O pour STOP

Tester si encore un caractère
! GOSUB"
aller chercher adresse de ligne

adresse ON-BREAK
Ignorer les espaces

-1II 37-

BASIC

DH NEED DE DEN DE DE DE DE DE DE DE DE HD HE DE HE DE DE DE DE EDEN NE

C8E1
C8E2
C8E5
C8E6

ES
CDO4BD
E1
C9

push
call
pop
ret

hl
BDO4
hl

LELRELE SELS ÉLLSISLS SSL LLLSLS LS LS S SE)

C8E7
C8E8
C8EB
C8EC

ES
CDO7BD
ET
C9

push
call
pop
ret

hl
BDO7
hl

MEHEDENEHE DE DD DE HE HÉDEHE DEEE DE DE DEEE DE HEDEHEDENEHE

C8ED
C8F0
C8F3
C8F5
C8F6
C8F9
C8FA
C8FD
C8FE
C900
C903
C906
c90g
C90c
C90F
cg12
cg15
c918
C91B
C9TE
cg21
C924
C925
C926
C928
C92B
C92E

CDA7BC
215CAC
0604
ES
CDECBC
E1
111200
19
10F5
CD48BB
CDF5BC
210000
2234AC
CD53C4
2138AC
110503
010008
CD24C9
2162AC
110B04
010102
C5

D5
OEFD
1179C8
CDEFBC
D1

call
1d
Id
push
call
pop
ld
add
djnz
call
call
1d
ld
call
1d
id
ld
call
ld
ld
1d
push
push
ld
ld
call
pop

BCA7
hl, AC5C
b,04

fl

BCEC

hl
de,0012
hl,de
C8F5
BB48
BCFS5
h1,0000
CAC34),h1
C453
hl,AC38
de,0305
bc, 0800
C924
h1, AC62
de, O40B
bc, 0201
bc

de

c,FD
de, C879
BCEF

de

1.0

Instruction Basic DI

KL EVENT DISABLE

Instruction Basic EI

KL EVENT ENABLE

Reset SOUND et Event
SOUND RESET

adresse de base du bloc Event

4 Timer

KL DEL TICKER

18

additionner
prochain Timer
KM DISARM BREAK
KL SYNC RESET

supprimer adresse ON-BREAK

autoriser interruption par ‘Break’

Adresse du bloc Event

sélect. BASIC-ROM
Adresse de routine Event
KL INIT EVENT

-III 38-

BASIC 1,0

C92F DS push de

C930 1600 ld d,00

C932 19 add hl,de

C933 D1 pop de

C934 C1 pop bc

C935 79 id a,C

C936 B7 or a

C937 2803 Jr z,C93C

C939 78 ld a,b

C93A 87 add a,a

C93B 47 ld b,a

C93C 15 dec d

C93D 20E5 jr nz,C924

C93F C9 ret

LÉESLÉELSSISSESESLSISLLLSEZLSLLLILSL.. Instruction Basic ON SQ

C940 CD37DD call DD37 Tester si encore un caractère
C9u3 28 db 28 Li de

C945 CD67CE call CE67 aller chercher valeur 8-bits
C947 F5 push hi

C948 CDSDC9 call C95D calculer adresse de Sound-Queue
C94B B7 or a supérieur à 4 ?

C94C 201E jr nz, C96C ‘Improper argument"

CQUE CD37DD call DD37 Tester si encore un caractère
C951 29 db 29 He KL

C952 CD61C8 call C861 aller chercher ‘’GOSUB’ et adresse
C955 F1 pop af

C956 ES push hl

C957 EB ex de,hl

C958 CDBOBC call BCBO SOUND ARM EVENT

C95B E1 pop hl

C95C cg ret

EEE DE HOMME HONOHEOHEME DEEE HUE ME GEHEHEGEHE EME GE calculer adresse de Sound-Queue
C95D 1F rra Bit O mis ?

CISE 1138AC Id de, AC38

C961 D8 ret C

C962 1F rra Bit 1 mis ?

C963 11UUAC id de, AC4U

C966 D8 ret C

-I1I1 39-

BASIC 1.0

C967 1F rra Bit 2 mis ?

C968 1150AC ld de, AC50

C96B D8 ret re

C96C 1E05 ld e,05 ’Improper argument’

CI6E C39UCA jp CA94 Sortir message d'erreur

REDON DE DEDE DEN DE DH DE ME EEDEHHE HEEEDEDE DEEE Instruction Basic AFTER

C971 CD/CCE call CE7C aller chercher valeur 16-bits O -
32767

C974 010000 ld bc,0000 Recharge Count sur zéro

C977 1805 jr C97E

LÉLÉLSLÉISSLLSLLLLSLLLLSLSLLLLLISSS. Instruction Basic EVERY

C979 CD7/CCE call CE7C aller chercher valeur 16 bits 0 -
32767

C97C 42 id b,d comme Count et

C97D 4B ld c,e Recharge Count

C97E DS push de

C97F C5 push bc

C980 CD55DD call DD55 suit virgule ?

C983 110000 ld de, 0000 valeur par défaut zéro

C986  DC86CE call c,CE86 oui, aller chercher valeur entière
avec signe

C989 EB ex de,hl

C98A  CDB1C9 call C9B1 aller chercher dans timer# adresse
du bloc Event

C98D ES push hi

C98E 010600 ld bc, 0006 additionner 6 octets pour bloc
ticker

C991 09 add h],bc

-111 40-

BASIC

C992 FEB ex de,hl
C993 CD61C8 call C861
C996 D1 DOP de

C997 c1 pop bc

C998 E3 ex (sp),hl
C999 EB ex de,hl
C99A CDE9BC call BCE9
C99D E1 pop h]

C9JŒ C9 ret

HORONREOHOHOHOHONHE HE HE MEHEHE DEMO DE DEOMME DE HEHE DEEE

C99F  CD8DFE call FE8D
CSA2 CDB1C9 call C9B1
C9A5 CDECBC call BCEC

C9A8 3803 jr c,C9AD
CSAA 110000 ld de, 0000
C9AD EB ex de,hl
COAE C3ODFF Jp FFOD
MH HÉ EH HE DE DE ÉD HE DE DE DE HÉME DE DEDE DE EME EU DEEE NE
C9B1 7C Id a,h
C9B2 B7 or a

C9B3 20B7 Jr nz, C96C
C9B5 7D 1d a, 1
C9B6 FEOb CP 04

C9B8 30B2 Jr nc, C96C
CIBA 87 add a, a
C9BB 87 add a,a
C9BC 87 add a, à
C9BD 85 add a, 1
CJBE 87 add a,a
C9BF 6F ld ],a
C9CO O15CAC 1d bc, ACSC
C9C3 09 add hl,bc
C9c4 C9 ret

EDR DEDEHE IE HE DE ED DE DE DE DE DE DE DE DD DE DE DE DE DE HE DE DEEE

C9C5 EB ex de,hl
C9C6 CDD2DD call DDD2
C9C9 EB ex de,hl

1,0

aller chercher ’GOSUB' et adresse

KL ADD TICKER

fonction BASIC REMAIN

CINT

aller chercher adresse du bloc EVENT
KL DEL TICKER

trouvé ?

non, zéro

accepter nombre entier dans hl
calculer adresse du bloc EVENT

Hi-Byte différent de zéro ?
oui, ‘Improper argument’

supérieur égal à 4 ?
oui, ‘Improper argument’

* 18

adresse de base table EVENT
plus Offset

chercher NEXT correspondant

adresse de ligne act. dans hl

-[Il 41-

C9CA
C9CB
C9CD
C9CF
C9D2
C9D3
C9D6
C9D8
C9DA
C9DB
C9DD
CODF
CSEO

C9E2
C3
CSE4
CSÆS
CJE8
CSS
CŒC
CSED
CIE
C9FO
C9F3
C9F5
C9F6
C9F7
C9FA
C9FB
C9FC
C9FF
CAO1

CAO3
CAO4
CAOS
CA06
CAO8
CAO9

2B
0601
OETA
CD23E9
ES
CD3FDD
FEBO
2808
ET
FESE
20EE
04
18EB

F1

EB

ES
CDD2DD
E5
CDCEDD
EB

05
2824
CD3FDD
280€
C5

DS
CD86D6
D1

C1
CD55DD
3002

10F2

2B

78

B7
280€
EB
CDD2DD

dec
ld
ld
cal]
push
call
CP
jr
pop
cp
jr
inc
jr

pop
ex
push
call
ex
call
ex
dec
Jr
cal]
Jr
push
push
cal]
pop
pop
call
Jr
dJnz

dec
ld
or
Jr
ex
call

BASIC 1.0

hl
b,01 compteur pour imbrication
c,1À numéro d’erreur pour ‘NEXT missing’
E923
hi
DD3F ignorer les espaces
BO "NEXT'’
z,C9E2
hl
JE FOR"
nz, C9CD
b augmenter imbrication
C9CD continuer à chercher
af
de,hl
hl
DDD2 adresse de ligne actuelle dans hl
(sp),hl
DDCE fixer adresse de ligne actuelle
de,hl
D diminuer imbrication
z, CA14 trouvé ’NEXT’ correspondant ?
DD3F ignorer les espaces
z,CAO3 fin de ligne ?
bc
de
D686 chercher variable
de
bc
DD55 virgule suit ?
nc, CAD3 non
C9F5 sinon prochaine variable après
INEXT'
h1
a, D trouvé ’NEXT' correspondant ?
a
z,CA14 oui
de,hl
DDD2 adresse de ligne actuelle dans hl

-11] 42-

BASIC

CAOC E3 ex (sp),hl
CAOD CDCEDD call DDCE
CA10 El pop hl

CA11 EB ex de,hi
CA12 18B9 jr C9CD
CA14 D1 pop de

CA15 C33FDD jp DD3F
PTTITLIILIITIIIILLLILLILSLSSSLSIEENÉEESS
CA18 2B dec hl

CA19 EB ex de,hl
CATA  CDD2DD call DDD2
CA1D EB ex de,hl
CAE 0600 1d b,00
CA20 O4 inc b

CA21 OE1D id c,1D
CA23 CD23E9 call E923
CA26 E5 push hl

CA27 CD3FDD call DD3F
CA2A El pop hl

CA2B FED6 CP D6

CA2D 28F1 jr Z, CA20
CA2F FED5 CP D5

CAS1 20FE jr nz, CA21
CA33 J0EC dJnz CA21
CA35 CD3FDD call DD3F
CA38 C33FDD jp DD3F
PPTTLILILILILILILILLLLLLSSSLL SSL S SEE)
CA3B 21AUAC ld hl,ACAu
CA3E 3600 ld (h1),00
CAO C33ABD Jp BD3A
DE HE ME DE DE DE DE DE DE DE DE HE DE DE DE SECHE DE DEN JEU EEE HN
CA43 21ALAC ld hl,ACA4

CA46 CD3ABD call BD3A
CAU9  C3HEC3 JP C3UE

CELL DELL EL LS ELLES SSSR SES.

1.0

fixer adresse de ligne actuelle

continuer à chercher

ignorer les espaces

chercher WEND correspondant

adresse de ligne actuelle dans h]

compteur pour imbrication

numéro d'erreur pour ‘WEND missing’

ignorer les espaces

WHHILE’
augmenter imbrication
*WEND'

diminuer imbrication
ignorer les espaces
ignorer les espaces

aller chercher ligne d'entrée
pointeur sur buffer d'entrée

vider contenu buffer

aller chercher ligne d'entrée

éditer ligne

pointeur sur buffer d'entrée
éditer ligne

sortir LF

aller chercher ligne d'entrée dans cassette

-I11 43-

BASIC 1.0

CAUC CS push bc

CA4D DS push de

CAUE  21A4AC Id hl,ACA4 pointeur sur buffer d'entrée
CAS1 ES5 push hl

CAS2 0601 ld b,01

CAS4  0E00 id c,00

CA5S6  CD80BC cal] BC80 CAS IN CHAR
CAS9 CAG6BCB Jp z, CB6B

CASC 3022 jr nc, CA80

CASE 77 ld (hl),a

CASF  FEOD CP OD CR

CA61 2817 Jr Z, CA7A

CA63 0E00 Id c,00

CA65 FEOA Cp OA LF

CA67 2006 jr nz, CAG6F

CA69 78 ld a,b £

CA6A 3D dec a

CA6B 28E7 jr z, CAS

CAGD OEFF Id c,FF

CA6F 78 ld a,b

CA70 B7 or a

CA71 1E17 ld e,17 ‘Line too long’
CA73 CA9UCA Jp z, CA94 sortir message d'erreur
CA76 23 inc hl

CA77 Où inc 0)

CA78 18DC Jr CA56

CA7A 79 1d a,C

CA7B B7 or a

CA7C 20D8 Jr nz, CAS6

CA7E 77 ld (hl),a

CA7F 37 scf

CA80 E1 pop hl

CA81 D1 pop de

CA82 C1 pop bc

CA83 C9 ret

RSS LESSSLS ESS SSSIRSRSSSLRSSSLSSSLLSSS] supprimer numéro d'erreur
CA84 AF xor a

-IIT 44-

BASIC

HDEHDED DD DENE HE DE DE DE DE DE DE HE DE DE DE DEC DE DE DE HE DE DEEE NE

CA85
CA88
CA8B
CASE

32AAAD
CDD2DD
22A6AD
C9

id
call
ld
ret

CADAA),a
DDD2
(ADA6),h1

DEHHONMDDEDEDEDE DE HE DEN DE DE DE DE DE MED DE DE DE DE DE DE DEN

CA8F

CA92
CA93

CD6DCE

CO
SF

call

ret
id

CE6D

nz
e,a

DEN DE HE DE DEN DEEE DE DE DEEE DEN DEEE D DE HD DEEE

CA94
CA97
CA98
CA9B
CASE
CAAI

CAAU
CAA7
CAAA
CAAD
CABO
CAB3

CAB6
CAB9
CABA
CABD
CABF
CACO
CAC]
CAC3
CAC4
CAC6
CAC7
CAC8

CDO4AC
7B

CD85CA
2A3UAE
22A8AD
CDBOCB

3100C0
2A32AE
CDACF5
CDB3FB
CDFDD9
CDDFCA

2AAFAD
EB
21B1AD
300€
7A

B3
2808
A6
2005
35

EB
C393DD

call

Id

call

Id

ld
call

1d
1d
cal]
call
call
call

ld
ex
id
Jr
ld
or
Jr
and
jr
dec
ex
jp

ACO4

a,e

CA85

hl, (AE34)
CADA8),h1
CBBO

sp, CO00
hl, (AE32)
FSAC
FBB3
D9FD
CADF

h1, CADAF)
de,hl
h1,ADB1
nc, CACB
a,d

e

Z, CACB
Ch1)
nz, CACB
(h1)
de,hl
DD93

1.0

fixer numéro d'erreur

numéro d'erreur

adresse de ligne actuelle dans h1l
ERROR-Line

instruction Basic ERROR
aller chercher valeur 8 bits non
nulle

numéro d'erreur dans e

sortir message d'erreur
ret

ranger numéro et ligne d'erreur

Adresse de l'instruction actuelle

pointeur de programme dans ERROR
ranger adresse de ligne et pointeur

de programme

pointeur de pile sur CO00

mémoire pour pointeur de pile Basic
réinitialiser pointeur de pile Basic
initialiser pile du descripteur

vider AE29 et AE2B

aller chercher numéro de ligne de la
ligne d'erreur

Adresse de la routine ON-ERROR

Flag pour traitement d'erreur

à la boucle de l’interpréteur

-JII 45-

CACB
CACD
CADO
CAD3
CAD6
CAD9
CADC

3600

3AAAAD
CD4SCC
2AAGAD
CDCEDD
CD36CB
C364C0

ld
ld
call
ld
call
call
JP

BASIC

(h1),00
a, (ADAA)
CC4s

hl, CADA6)
DDCE
CB36
CO64

MENU HEHE HE DEDEDE DE DEN DD DE DE HE HE DE DE DE DEEE DE DEEE

CADF
CAE2

CAES
CAE6
CAE9

2AAG6AD

CDD9DD

D8
210000
C9

1d

call

ret
1d
ret

hl, CADAG6)
DDD9

C
h1,0000

HE DE DEEE DD DEN DEDE DEEE DEN DE DEEE DEN DEEE NN

CAEA
CAEB
CAEC
CAEF
CAF1

CAF3
CAFH
CAF5
CAF8
CAFA
CAFB
CAFC
CAFF
CBO0
CBO1
CBO2

CBOS
CB06
CB09
CBOA
CBOD

D5

ES
2113CD
1E0B
1807

D5

ES
21B9CC
1E06
F5

ÉS
2AAFAD
7C

B5

ET
C294CA

AF
CDA2C1
F2
CD41C3
CDUEC3

push
push
ld
ld
Jr

push
push
id
ld
push
push
ld
id
or
pop
Jp

xor
call
push
call
call

de

hi
h1,CD13
e,0B
CAFA

de

h]
h1,CCB9
e,06

af

hl

hl, CADAF)
a,h

l

hi

nz, CA94

C1A2
af

C341
C3UE

1.0

numéro ERROR

fixer pointeur sur message d'erreur

Adresse de la ligne ERROR

fixer adresse de ligne actuelle

au mode READY

Adresse de la ligne ERROR

aller chercher numéro de ligne dans

hl

pointeur sur
Division by zero’

pointeur sur
’Overflon'

Adresse de la routine ON-ERROR

sortir message d'erreur

sortir chaîne
sortir LF

-I1I 46-

CB10
CB11
CB14
CB15
CB16
CB17

CB18
CB1B
CBIE
CB21

F1
CDA2C1
F1
ET
D1
C9

CD86C3
2123CB
CD48CB
181D

pop
call
POP
POP
pop
ret

call
1d
call
Jr

af
C1A2
af
hl
de

C386

BASIC 1.0

initialiser écran

hl,CB23 sortir ’Undefined line”

CB48
CB4O

sortir numéro de ligne
sortir ‘in numéro de ligne’

HD NEHODEH DD HE HUE DEHME HE DEEE EEE EEE

CB23 55 6E 64 65 66 69 6E 65
CB2B 64 20 6C 69 6E 65 20 00

‘Undefined line ‘

HEOROMOHOHE MEME HE ME DE HE HE HHE HE HE EE HE HE DEEE EOH DE

CB33 114FCB ld de, CBUF pointeur sur ‘Break’
CB36 CD9DC1 call C19D

CB39 CD86C3 call C386 initialiser écran

CB3C EB ex de,hl

CB3D CD41C3 cal] C341 sortir

CB4O  CDD6DD call DDD6 aller chercher No de ligne dans hl
CB43 DO ret nc mode direct ?

CB4L EB ex de,hl

CB4S 2155CB id h1,CB55 pointeur sur ‘ in ‘
CB48 CD41C3 call C341 sortir

CBUB EB ex de,hl

CB4C C379%EE jp EE79 sortir numéro de ligne
AEEODMHEHE DE HE DE HE NEED HE EH MEME EE HE DE DE DEN HE HE EN

CBUF 42 72 65 61 6B 00 Break”

CB55 20 69 6E 20 00 "in
CTTLLLILILLILSLSSISLLLLSSISSILSS SES EE ES instruction Basic STOP
CB5SA CO ret nz

CBSB ES push h]

CB5C CD33CB call CB33 ‘Break in numéro de ligne”
CB5F El pop hl

CB60 CD93CB call CB93

-111 47-

BASIC

CB63 182B jr CB90
CERESSSSSSSRERSLLSISSLSELSSLLESLSLSESS
CB65 CO ret nz

CB66 CD93CB cal] CB93
CB69 181C jr CB87
CB6B CD33CB call CB33
CBGE 2A3UAE Id h1,(AE34)
CB71 CDBOCB call CBBO
CB74 181A Jr CB90
CB76 CDD6DD call DDD6
CB79 3012 Jr nc, CB8D
CB7B CDABCB call CBAB
CB7E 3AB1AD ld a, (ADB1)
CB81 B7 or a

CB82 1E13 Id e,13
CB84 C294CA JP nz, CA94
CB87 CD98D2 call D298
CB8A CDA1D2 call D2A1
CB8D CDCBDD call DDCB
CB90 C364C0 Jp CO64
LÉLELL RL SSL SR SSL SSLRSLLLÉRLLSESSSL ESS
CB93 EB ex de,hl
CB94 CDD6DD call DDD6
CB97 EB ex de,hl
CB98 DO ret nc

CB99 7E ld a,(hl)
CB9A FEO1 cp 01

CB9C  280B Jr z, CBA9
CBJŒ 23 inc hl

CB9F 7E ld a, (hl)
CBAO 23 inc hi

CBA1 B6 or (h1)
CBA2 2807 Jr z, CBAB
CBA4 23 inc hl

CBA5 CDCEDD call DDCE

1.0

au mode READY

instruction Basic END

Adresse de l'instruction actuelle

mode direct ?
oui

encore en traitement d'erreur ?

"RESUME missing”
oui, sortir message d'erreur

adresse de ligne actuelle sur zéro
au mode READY

aller chercher No de ligne dans hl

mode direct ?

fixer adresse de ligne actuelle

-[11 48-

CBAB
CBA9

CBAB
CBAE

CBBO
CBB1

CBB4
CBB5

CBB8
CBBB
CBBC

CBBF

ROMEO NE DE DEN HE HD DE DE MEME DE DE DE DE DE DEEE

CBCO
CBC1

CBC4
CBC5
CBC6
CBC8

CBCB
CBCC
CBCF
CBD2
CBD5
CBD6

23
1805

210000
180€

EB
CDD6DD
DO
CDD2DD

22ADAD
EB
22ABAD

cg

CO
2AABAD

7C

B5
1E11
CA9ACA

ES
2AADAD
CDCEDD
CDB9BC
ET
C374DD

inc
jr

ld
jr

ex
call
ret

call

id
ex
Id

ret

ret
Id

id
or
id
JP

push
ld
call
call
pop
JP

BASIC 1,0

LELLLLLE SL LS LLL EL ES LS SLI LLLLLL LL)

CBD9
CBDA

CBDD
CBEO
CBE4

AF
32B1AD

110000
ED53AFAD
C9

xor
id

1d
ld
ret

hl
CBBO
h1,0000
CBBC
de,hl
DDD6 mode direct ?
nc oui
DDD2 adresse de ligne dans hl
(ADAD),h1 adresse de ligne après interruption
de,hl
(ADAB),hl pointeur de programme après
interruption
instruction Basic CONT
nz
h1l, CADAB) pointeur de programme après
interruption
a,h
l
e,i1 ‘Cannot CONTinue'
Z, CA94 sortir message d'erreur
hl
h1, (ADAD) adresse de ligne après interruption
DDCE fixer adresse de ligne actuelle
BCB9 SOUND CONTINUE
hl
DD/4 à la boucle de l'interpréteur
a
(ADB1),a supprimer flag pour en traitement
d'erreur
de, 0000
CADAF), de Adresse de la routine ON-ERROR

-III 49-

BASIC

EE HD DE DE ME DEEE DE HEDEDE DE DE DEEE DEHEDEDEDEDEDE DE HE DEEE

CBES CD3FDD call DD3F
CBE8 CD37DD call DD37
CBEB AO db A0

CBEC CDEICE call CEE1
CBEF ES push hl

CBFO CD9AE7 call E79A
CBF3 22AFAD id CADAF),h]
CBF6 E1 pop hl

CBF7 C9 ret

HONOR DD DEN DEHE HE DEMI HE DE HD HE DE HE DEEE DEN DE NE
CBF8 CDDDCB call CBDD
CBFB 3AB1AD ld a, (ADB1)
CBFE B7 or a

CBFF C8 ret Z

CCO0 C3AUCA Jp CAAU
LÉSESLISLLLLLLSSSLSLLSSLSISLLSSSLSRLSS XL]
CCO3 2814 jr z,CC19
CCO5 FEBO CP BO

CCO7 2817 Jr z, CC20
CCO9 CD67E7 call E767
CCOC  CDAADD call DD4A
CCOF DS push de

CC10 CD2BCC call CC2B
CC13 E1 pop hl

CC14 23 inc hl

CC15 F1 pop af

CC16 C393DD Jp DD93
CC19 CD2BCC call CC2B
CCIC F1 pop af

CC1D C374DD jp DD74
CC20 CD3FDD call DD3F
CC23 C0 ret nz

CC24 CD2BCC call CC2B
CC27 23 inc hl

1.0

ON ERROR

ignorer les espaces

Tester si encore un caractère
"GOTO"’

aller chercher No de ligne dans de

chercher ligne Basic de
Adresse de la routine ON-ERROR

instruction Basic ON ERROR GOTO 0
adresse de ligne actuelle sur zéro
en traitement d'erreur ?

non

instruction Basic RESUME
'NEXT'
allier chercher adresse de ligne
fin de l'instruction, sinon ’Syntax

error’

supprimer flags ERROR

à la boucle de l'interpréteur
supprimer flags ERROR

à la boucle de l'interpréteur
ignorer les espaces

supprimer flags ERROR

-JII 50-

CC28

CC2B
CC2E
CC2F
CC31

CC34
CC35
CC38

CC3B
CC3E
CCH
CCuu

C3EFES8

3ABTAD
B7
1E14
CASHCA

ÂF
32AAAD
32B1AD

2AAGAD
CDCEDD
2AA8AD
cg

jp

Id
or
1d
jp

xor
ld
ld

ld
call
id
ret

BASIC

E8EF

a, (ADB1)
a

e,14

z, CA94

a
CADAA), a
CADB1),a

h1, (ADAG6)
DDCE
hl, (ADA8)

LESLÉSS ELLES ETS LL SELS ELLE), )L,),),),),)

CCAS

CCu8
CC4A
CCUB
CC4C
CCHD
CCUE
CCHF
CC50
CC51
CC53
CC54
CC56
CC57
CC58
CC5A

115BCC

FETF
DO
B7
C8
47
1A
13
B7
20FB
05
20F8
1A
B7
28EB
C9

ld

CR
ret
or
ret
id
ld
inc
or
Jr
dec
Jr
id
or
jr
ret

de, CC5B

1F

nc

a

Z

b,a

a, (de)
de

a

nz, CCHE
b

nz, CCHE
a, (de)
a
z,CCHS

HOHOROHOMOHOHOHEOHONNEOHEHEE NME DEEE DEHEDEMEGEONNEE

CC5B 55 GE 6B 6E 6F 77 6E 20 65
CC64 72 72 6F 72 00
CC69 55 6E 65 78 70 65 63 74 65
CC72 64 20 LE 45 58 54 O0

1.0

ignorer reste de la ligne
en traitement d'erreur ?

‘Unexpected RESUME"
non, sortir message d'erreur

annuler numéro ERROR

supprimer flag pour en traitement
d'erreur
Adresse de la ligne ERROR
comme adresse de ligne actuelle
pointeur de programme après ERROR

fixer pointeur sur message d'erreur
adresse de base des messages

d'erreur

31, numéro d'erreur maxi +1

> =, alors 0 ‘Unknown error’

0, alors terminé
numéro d'erreur dans b

lire caractère
jusqu'à 0, fin d'un message

prochain message
pas encore message voulu ?

messages d'erreur
O Unknown error

1 Unexpected NEXT

STE Sie

CC79
CC82
CC86
CC8F

cc98
CCAI

CCA7
CCBO
CCB9
CCC2
CCCB
CCCE
CCD7
CCEO
CCE2
CCEB
CCF4
CCF9
CDO2
CDOB
CD13
CD1C
CD24
CD2D
CD35
CD3B
CD4H
CD4g
CD52
CD5B
CD64
CD6B
CD/4
CD7D
CD86
CD89
CD92
CD99
CDA2
CDAB

53
72
55
64
uy
75
yg
61
UF
UD
6C
yC
20
74
53
20
61
y
65
6E
y
62
49
69
6D
54
61
53
61
53
6F
53
70
74
65
43
LE
55
73
69

79
6F
6E
20
ul
73
6D
72
76
65
6c
69
6E
00
75
6F
6E
72
61
73
69
79
6E
72
61
79
7ù
74
63
74
20
74
72
6F
78
61
54
6E
65
6F

6E
72
65
52
54
74
70
67
65
6D
00
6E
6F

62
75
67
72
64
69
76
20
76
65
6E
70
63
72
65
72
6C
72
65
6F
00
6E
69
6B
72
6E

74
00
78

45

1
65
72
75
72
6F

65
74

73
74
65
61
79
6F
69
7A
61
63
64
65
68
69
20
69
6F
69
73
20

6E
6E
6E
20
00

61

70
54
20
64
6F
6D
66
72

20
20

63
20
00
79
20
6E
75
65
6C
74
00
20
00
6E
66
6E
6E
6E
73
63

6F
75
6F
66

78

65
55
65
00
70
65
6C
79

64
65

72
6F

20
64
65
69
72
69
20

6D

67
75
67
67
67
69
6F

74
65
77
75

20

63

52
78

65
6E
6F
20

6F
78

69
66

61
69
64
6F
6F
64
63

69

20
6C
20
00
20
6F
6D

20
00
6E
6E

65

74

LE
68

72
74
77
66

65
69

70
20

6C
6D
00
6E
00
20
6F

73
73
6C
74
65
6E
70
43

20
63

BASIC 1.0

72

2
65
00 3
61

4
20
00 5
00 6
75

7
73
73

8
74
72 ù

9
72
65

10
20

11
64
6D

12
6D

13
70
00 14
6F

15
78
20
6C

16
4F

17
75
74

18

-111 52-

Syntax error

Unexpected RETURN

. DATA exhausted

Improper argument
Overflon

Memory full

Line does not exist

Subscript out of range

Array already dimensioned

Division by zero

Invalid direct command
Type mismatch
String space full

String too long

String expression too complex

Cannot CONTinue

Unknown user function

BASIC 1-0

CDAF 52 45 53 55 UD 45 20 6D 69

CDB8 73 73 69 6E 67 O0 19 RESUME missing
CDBE 55 GE 65 78 70 65 63 74 65
CDC7 64 20 52 45 53 55 4D 45 00 20 Unexpected RESUME

CDDO 44 69 72 65 63 74 20 63 6F
CDD9 6D 6D 61 6E 64 20 66 6F 75

CDE2 6€ 64 00 21 Direct command found

CDES 4F 70 65 72 61 6E 64 20 6D

CDEE 69 73 73 69 6E 67 00 22 Operand missing

CDFS 4C 69 6E 65 20 74 6F 6F 20

CDFE 6C 6F 6E 67 00 23 Line too long

CEO3 45 4F 46 20 6D 65 74 O0 24 EOF met

CEOB 46 69 6C 65 20 74 79 70 65

CE14 20 65 72 72 6F 72 00 25 File type error

CE1B 4E 45 58 54 20 6D 69 73 73

CE24 69 6E 67 00 26 NEXT missing

CE28 46 69 6C 65 20 61 6C 72 65

CE31 61 64 79 20 6F 70 65 6E 00 27 File already open

CE3A 55 6E 6B 6E 6F 77 6E 20 63

CE43 6F 6D 6D 61 6E 64 O0 28 Unknown command

CEUA 57 US LE 4h 20 6D 69 73 73

CES3 69 6E 67 00 29 WEND missing

CES7 55 6E 65 78 70 65 63 74 65

CE60 64 20 57 45 UE 44 OO 30 Unexpected WEND

LÉELLSLLÉESLLSLLLSLSSSRSSSLSLLLLSLSS LISE) aller chercher valeur 8 bits

CE67  CD86CE call CE86 aller chercher valeur entière avec
signe

CE6A F5 push af

CE6B 1808 jr CE75

SÉNATENERMEENARRER aller chercher valeur 8 bits différente de zéro

CE6D CD86CE call CE86 aller chercher valeur entière avec
signe

CE70 F5 push af

CE71 7A 1d a,d

CE72 B3 or e zéro ?

CE73 2836 jr Z, CEAB alors ’Improper argument’

CE75 7A ld a,d

CE76 B7 or a Hi-Byte différent de zéro ?

<FTT.53-

BASIC 1.0

CE77 2032 Jr nz, CEAB alors ‘’Improper argument’

CE79 F1 pop af

CE7A 7B ld a,e

CE7B C9 ret

PORPOMENPNEEERRRANNERANNENR aller chercher valeur 16 bits 0 à 32767

CE7C  CD86CE call CE86 aller chercher valeur entière avec
signe

CE7F F5 push af

CE80 7A Id a,d

CE81 17 rla Bit 15 mis ?

CE82 3827 jr c,CEAB alors ‘Improper argument”

CE84 F1 pop af

CE85 C9 ret

RE ER AR AE aller chercher valeur entière avec signe

CE86 CDFBCE call CEFB aller chercher expression

CE89 F5 push af

CE8A EB ex de,hl

CE8B CD8DFE call FE8D CINT

CES8E EB ex de,hl

CE8F F1 pop af

CE90O C9 ret

ONE REEREHHEX A]ler chercher valeur 16 bits expression adresse

CE91 CDFBCE call CEFB aller chercher expression
CE94 F5 push af

CE95 C5 push bc

CE96 ES push hl

CE97 CDC2FE call FEC2 UNT

CESA EB ex de,hl

CE9B E1 pop hl

CE9C C1 pop bc

CE9D F1 pop af

CESE C9 ret

RAOOOOONREEEREEEEEEEXX j]ler chercher expression chaîne et paramètres
CE9F  CDFBCE call CEFB aller chercher expression
CEA2 C3DAFB JP FBDA aller chercher paramètres chaîne

-H11 54-

BASIC 1,0

LÉLLLÉELSSSSÉSSLESLSLISLSLSSLSLS SX ETS) aller chercher expression chaîne
CEAS CDFBCE call CEFB aller chercher expression

CEA8S C33CFF jp FF3C Type ‘chaîne’, sinon ‘Type mismatch'
CEAB 1E05 id e,05 ’Improper argument’

CEAD C394CA jp CA94 sortir message d'erreur

RERO ERA aller chercher zone de numéros de ligne
CEBO 010100 ld bc, 0001 1

CEB3 11FFFF ld de,FFFF et 65535 par défaut

CEB6 CDSSDD call DD55 tester si virgule

TEL "55-

BASIC 1.0

CEBS D4S1DD call nc, DD51 fin de l'instruction ?

CEBC D8 ret C oui

CEBD FE23 cp 23 ’#' (numéro de canal) ?

CEBF C8 ret Z

CECO FEFS CP F5 et

CEC2 280A jr z,CECE

CEC4 CDEICE call CEE1 aller chercher No de ligne dans de
CEC7 42 1d b,d

CEC8 4UB ld c,e copier dans bc

CEC9 C8 ret Z

CECA CDS5DD call DD55 virgule suit ?

CECD D8 ret (e oui

CECE CD37DD call DD37 Tester si encore un caractère
CEDT F5 db F5 Fe

CED2 11FFFF id de,FFFF 65535 comme valeur finale par défaut
CEDS C8 ret Z

CED6 CDS5DD call DD55 tester si virgule

CED9 D8 ret C

CEDA CDEICE call CEE] prendre numéro de ligne dans de
CEDD C4SSDD call nz, DD55 tester si virgule

CEEO C9 ret

EODEDUEDE HE DE DE DEEE DE DE DEEE D EDEN DE DD DEEE EEE prendre numéro de ligne dans de
CEET 7E ld a, (hl) type constante

CEE2 23 inc hl

CEE3 SE Id e,(hl)

CEEU 23 inc hl valeur dans de

CEES 56 ld d,(h1)

CEEG FEIE CP 1E numéro de ligne ?

CEE8 280E jr z, CEF8 oui, terminé

CEEA FEID CP 1D adresse de ligne ?

CEEC C27BD0O jp nz, DO7B non, ‘Syntax error’

CEEF ES push hl

CEFO EB ex de,hi hl désigne début de ligne

CEF1 23 inc hl

CEF2 23 inc hl

CEF3 23 inc hl

CEF4 SE Id e,(hl)

CEFS 23 inc hl numéro de ligne dans de

CEF6 56 ld d,(hl)

-111 55-

BASIC 1.0

CEF7 El pop hl

CEF8 C33FDD jp DD3F ignorer espaces

CTELILILILLILSLIILLLLLLLLLISLLSILLSS EX) aller chercher expression

CEFB C5 push bc

CEFC 2B dec hi

CEFD 0600 id b,00 code de hiérarchie

CEFF  CDO/CF call CFO7 aller chercher terme

CFO2 C1 pop bc

CFO3 2B dec hl

CFO C33FDD jp DD3F ignorer espaces

LELLLLLLLLLLLLILLSSILLSLLLLSÉES LS S aller chercher terme

CFO7 C5 push bc

CFO8  CDCBCF cal! CFCB aller chercher terme

CFOB E5 push hl

CFOC El pop hi

CFOD C1 pop bc

CFO 7E Id a,(hl)

CFOF FEEE CP EE ">!

CF11 D8 ret C inférieur ?

CF12 FEFE cp FE NOT"

CF14 DO ret nc supérieur égal ?

CF15S FEF4 CP F4 +!

CF17 3840 jr c,CF59 inférieur, alors opérateur de
comparaison

CF19 CCASFF call z,FF45 ‘+, alors tester si chaîne

CF1C 2012 jr nz, CF30 pas chaîne

CFIE C5 push bc

CFIF ES push hi

CF20 2AC2B0 1d h1, (BOC2) Stringdescriptor

CF23 E3 ex (sp),hi sur pile

CF24  CDCBCF call CFCB aller chercher terme suivant

CF27 CD3CFF call FF3C Type ‘chaîne’, sinon ‘Type mismatch”

CF2A E3 ex (sp),hl

CF2B CD63F8 call F863 addition de chaînes

CF2E 18DC jr CFOC traiter terme suivant

PTLLLLLSLILLLLLLS LS ÉESLLSLSSLS SSL) opérateurs arithmétiques

CF30 7E ld a, (hl)

-111 56-

BASIC 1.0

CF31 D6F4 sub F4 moins F4

CF33 87 add a,à

CF34 87 add a, à fois 4

CF35 C681 add a,81

CF37 SF ld e,a plus CF81, adresse de table

CF38 CECF adc a, CF

CF3A 93 SUD e

CF3B 57 Id d,a

CF3C EB ex de,hl

CF3D 78 id a,b

CF3E BE cp (h1)

CF3F EB ex. de,hl

CF4O DO ret nc

CF4T CS push bc

CF42  CD53FF call FF53 placer résultat sur la pile Basic

CF4S DS push de

CF46 C5 push bc

CF47 A ld a, (de) code de hiérarchie

CFU8 47 Id b,a

CF49  CDO7CF call CFO7 aller chercher terme

CF4C C1 pop bc

CF4D E3 ex (sp),hl

CFUE 23 inc hl

CFUF EB ex de,hl

CF50 79 Id a, C

CF51  CDAOFS call F5AO libérer place dans la pile Basic

CF54  CDFBFF call FFFB Jp (de), exécuter opération

CF57 18B3 Jr CFOC traiter terme suivant

LRRLESÉELLLSLLSL ESS LELSLLSLSLLLES ES opérateurs de comparaison

CF59 78 ld a,D

CFSA FEOA cp OA

CF5C DO ret nc

CF5D C5 push bc

CFSE 7E ld a, (hl) Token

CF5F  D6ED sub ED moins Offset

CF61 47 ld b,a

CF62 CD4SFF call FF45 Tester si chaîne

CF65  11A9CF ld de, CFA9 Adresse pour comparaisons
arithmétiques

-II1 57-

BASIC 1,0

CF68 20D8 jr nz, CF42 pas chaîne

CF6A E5 push hl

CF6B 2AC2B0 ld h1, (BOC2) Stringdescriptor

CF6E E3 ex (sp),hl sur pile

CF6F C5 push bc

CF70 O6OA ld b,0A code de hiérarchie

CF72 CDO/CF call CFO7 aller chercher terme

CF75 C1 pop bc

CF76 E3 ex (sp),hl

CF77 C5 push bc

CF78 CD97F8 call F897 comparaison de chaînes

CF7B C1 pop bc

CF7C  CDAFCF call CFAF aller chercher résultat de
comparaison

CF7F 188B jr CFOC traiter terme suivant

RO EOPEEREXE codes de hiérarchie des opérateurs Basic + adresses

CF81 OC db OC F4, ‘+!
CF82 C3CCFC jp FCCC

CF85 OC db OC RES
CF86 C3EIFC Jp FCE1

CF89 12 db 12 FETE
CF8A C3F5FC JP FCFS

CF8D 12 db 12 F7, ‘/!
CF8& C312FD jp FD12

CF91 16 db 16 F8, °°
CF92 C3F4D4 Jp D4F4

CF95 10 db 10 F9, ‘Backslash'
CF96 C337FD Jp FD37

CF99 06 db 06 FA, "AND'
CF9A C358FD jp FD58

CF9D OE db 0E FB, ‘MOD’
CFSE C349FD Jp FD49

CFAT O4 db 04 FC, OR’
CFA2 C363FD jp FD63

CFA5S 02 db 02 FD, "XOR'
CFA6 C36DFD Jp FD6D

CELL LLLL ELLES LS LL LS L SL SSL SSL LL.) Compa ra Î son ar Î thmét ique
CFA9 OA ld a, (bc)

-III 58-

CFAA C5 push bc

CFAB CDO9FD call FDO9 comparaison arithmétique
CFAE C1 pop bc

CFAF  C601 add a,01

CFB1 8F adc a,a

CFB2 AO and D

CFB3 C6FF add a,FF

CFBS 9F sbc a,à

CFB6 C3O5FF jp FF05 accepter signe

CELL LILLILLLLLLLLLIILLLRSSSLSSISR ES: 4? signe négatif
CFB9 2B dec h1

CFBA 0614 ld b,14 code de hiérarchie
CFBC CDO7CF call CFO7 aller chercher terme
CFBF  C389FD Jp FD89 changer signe

BASIC 1.0

‘

CELLES SLSLSS RES LSRELESR SES SSSR EL RS)

opérateur Basic NOT

CFC2 2B dec hl
CFC3 0608 ld b,08 code de hiérarchie
CFCS CDO7CF call CF07 aller chercher terme

CFC8 C377FD JP FD77 opérateur NOT
HONDA DEDE HE DEEE DE DE DE DEEE DE HE DE DEEE DE DE HE EEE al ler chercher expression
CFCB CD3FDD call DD3F ignorer espaces

HORDE DE DEEE DEN HEDE DE DEEE DEDE DEEE HE DE DENON DEEE

aller chercher expression

CFCE 281D jr Z, CFED 'Operand missing”

CFDO FEOE CP 0E

CFD2 3839 jr c,DOOD aller chercher variable
CFD4 FE20 Cp 20

CFD6 3854 jr c,DO2C aller chercher valeur numérique
CFD8 FE22 CP 22 PE

CFDA CACBF7 jp z,F7CB au traitement de la chaîne
CFDD FEFF Cp FF fonction ?

CFDF  CA80ODO jp z, DO80 au calcul de fonction

CFE2 ES push hl

CFEZ 21F2CF ld hl,CFF2 adresse de base de la table
CFE6 CD93FF call FF93 rechercher dans la table
CFE9 E3 ex (sp},hi

-II1 59-

CFEA

CFED
CFEF

DD DE DE DE DD DE DD DH DEN NE DE DD DE HE DE DE NEED DE DE DE DEN

CFF2
CFF3
CFFS
CFF6
CFF8
CFF9
CFFB
CFFC
CFFE
CFFF
DO0!
D002
DOO!
D005
D007
D008
DOOA
D00B

MERDE DE DIE DEEE DE HE DE DD DEN DEN DE DEEE DEN DEEE DE DEEE HE

DOOD
D010
D012
D014
D016
D017
D018
DO1B
DO1C

DO1D
DO1F
DO22
D0O25

C33FDD

1E16
C394CA

08
78D0
F5
BQCF
F4
CECF
28
70D0
FE
C2CF
E3
EEDO
EU
30D1
AC
4BF9
40
FADO

CD90D6
300B
FE03
280F
E5

EB
CDUBFF
E1

C9

FE03
C2F3FE
112BD0
EB

Jp

ld
Jp

db
du
db
dw
dw
du
db
dw
db
dy
db
dw
db
dy
db
du
db
dy

call
Jr
(oo)
Jr
push
ex
call
pop
ret

cp
Jp
Id
ex

BASIC

DD3F

e,16
CA94

08
DO78
F5
CFB9
F4
CFCE
28
DO70
FE
CFC2
E3
DOEE
Eu
D130
AC
F9uB
40
DOFA

D690
nc,DO1D
03

z, DO25
hl
de,h1
FF4B

hl

03
nz,FEF3
de,DO2B
de,hl

1,0

ignorer espaces

‘Operand missing”
sortir message d'erreur

fonctions spéciales

nombre d'entrées dans la table

pas trouvé, ‘’Syntax error’

4

1

NOT”

"ERL'

EN‘

*MID$'

4j"

aller chercher variable
aller chercher adresse de
pas encore initialisée ?

type de variable
chaine ?

chaine ?
supprimer variable
fixer valeur à zéro

-I11 60-

variable

BASIC 1.0

D0O26 22C2B0 id (BOC2),hl
DO29 EB ex de,hl
DO2A C9 ret

OKON À ON OÙ KE NO OÙ À NM OK OK NO OX KO KO OK X M NOK XX M OX XX +

DO2B 00 db 00 zéro

LÉLLES SLI LL RSS ESS LL LSSLSLSLLILSLSS] aller chercher valeur numér ique

DO2C D60E sub 0E ôter offset

DO2E FEOA CP OÀ inférieur 10 ?

DO30 381D Jr c,DO4F oui, aller chercher chiffre

DO32 23 inc hl

DO33 FEOB Cp OB valeur sur un octet ?

DO35 2817 Jr z,DOUE

DO37 FEOF cp OF valeur sur deux octets (déc, hex,
bin) ?

DO39 380€ Jr c,D049

DO3B FE11 cp 11

DO3D 381A Jr c,D059 valeur à virgule flottante ?

DO3F 203A Jr nz, DO7B "Syntax error’

DO41 3E05 ld a,05 Real”

DO43 CD4BFF call FFUB

DOU6  2B dec hl

DO47 1824 jr DO6D ignorer espaces

LÉLLLSSSSLSLLLSSSSSSLLSRRS SSL LSSSS LT] aller chercher valeur deux octets

DO49 GE 1d e,(h1)

DOUA 23 inc hi

DOUB 56 ld d,(h1l)

DO4C 1804 jr DO52 accepter nombre entier dans de

LÉLÉSSSSSLLSSLSSRSLLLLLRSSLLISSSS TS. aller chercher valeur sur un octet

DOUE 7E Id a,(hl)

DOUF SF ld e,a

DO50 1600 ld d,00 fixer octet fort sur zéro

DO52 EB ex de,hl

DO53 CDODFF call FFOD accepter nombre entier dans hl

DO56 EB ex de,hl

DO57 1814 jr DO6D ignorer espaces

-I11 61-

BASIC 1.0

FARRPENEPENEERANENAENTERY aller chercher valeur à virgule flottante
DO59 5€ ld e,(h1)

DOSA 23 inc hl

DOSB 56 ld d,(h1)

DOSC E5 push hl

DOSD FEOF cp OF

DO5F 2007 Jr nz, DO68

D061 13 inc de

D062 EB ex de,hl

D063 23 inc hl

DC64 23 inc nl

D065 GE ld e,(hl)

DO66 23 inc hl

D067 56 id d,(hl)

D068 EB ex de,hl

D06S CD6OFE call FE60 type de variable sur ‘'Real'
DO6C El pop hl

DO6D C33FDD jp DD3F ignorer espaces
AENASARENRENESEANERANANE ‘(aller chercher valeur entre parenthèses
DO70 CDFBCE call CEFB aller chercher expression
DO73 CD3/DD call DD37 tester si encore un caractère
DO76 29 db 29 je

DO77 C9 ret

LÉLLSÉ SL LLLSSLSLSL SSL LS SSL LSLS LS)

DO78 CDODAC call ACOD ret

DO7B 1E02 1d e,02 ’Syntax error’

DO/D C394CA JP CA94 sortir message d'erreur
LÉELSSSLLSIESRS SSSR RSLSERSLSESRSSLSE ST) ca Icul de fonct i on

DO80 23 inc hl augmenter pointeur de programme
DO81 LE ld c,(hl) aller chercher token

DO82 CD3FDD call DD3F ignorer espaces

D085 79 id a,C tester token

DO86 FELO cp 40

DO88 3805 jr c,DO8F 40 - 48, variable réservée
DO8A FE49 cp yg

DO8C DABBDO jp c,DOBB

DO8F CD37DD call DD37 tester si encore un caractère

-I11 62-

DO92
D093
DO94
DO95
DO97
DO98
DO9A
DO9C
DOSE
DOAO

DOA3
DOA4
DOA7
DOA8

DOA9
DOAC

DOAE
DOAF
DOB1
DOB4
DOB5
DOB6
DOB7
DOB8
DOB9
DOBA

DOBB
DOBC
DOBD
DOBF
DOC2
DOC3
DOC4
DOCS
DOC6
DOC7

28
79
87
C61E
LF
FE59
300D
FE1D
380€
CD70D0

ES
CDAEDO
ET
cg

CDOAAC
18CD

ES
0600
2190D1
09

7E

23

66

6F

E3

C9

ES

UF
0600
214AD0
09

09

7E

23

66

6F

db
id
add
add
ld
cp
jr
Cp
jr
call

push
call
POP
ret

call
jr

push
1d
1d
add
1d
inc
id
ld
ex
ret

push
1d
1d
ld
add
add
ld
inc
ld
ld

BASIC 1,0

28 RC
a,C
a, à
a,1E
C,a
59
nc, DOAS ‘Syntax error’
1D
c,DOAE calculer fonction
DO70 aller chercher argument de la
fonction entre parenthèses
hi
DOAE calculer fonction
hl

ACOA ret
DO7B ’Syntax error’

hl

b,00

h1,D190 Adresses des fonctions
h1,bc

a,(hl)

hl

h,(h1l)

1,a

(sp),hl

hl

C,a

b, 00
hl,DO4UA
h1,bc
hl,bc
a,(hl)
hl

h, (h1)
1,a

-111 63-

BASIC 1.0

DOC8 E3 ex (sp),hl

DOC9 C9 ret

CESSE LLLIRLSSSSSSLRSSSSLLSSSSS SSL] Adresses des variables réservées

DOCA 17C4 dw C417 UO, EOF

DOCC DCDO dw DODC 41, ERR

DOCE F4DO dw DOF4 42, HIMEM

DODO 24FA dw FA24 43, INKEY$

DOD2 DBD4 dw DUDB 4, PI

DOD4 84D5 dW D584 45, RND

DOD6 ESDO dw DOES 46, TIME

DOD8 G7D1 du D107 47, XPOS

DODA OEDI dw D10E 48, YPOS

LÉRÉELLLELELL SSL LS LLS SSL SSL SSS SLT] ERR

DODC ES push hl

DODD 3AAAAD id a, (ADAA) numéro ERROR

DOEO  CDOAFF call FFOA accepter contenu accu comme nombre
entier

DOE3 EI pop hl

DCE4 C9 ret

LÉELSLLELLSISSLLLLLLÉESLLLISLLSISSLS ST: TIME

DOS ES push hi

DOE6 CDODBD call BDOD KL TIME PLEASE

DOEQ  CD/CFE call FE7C convertir valeur 4 octets en valeur
à virgule flottante

DOŒEC E1 pop hl

DOED C9 ret

LÉLLELLIL LL ESS LES LLSI LL LL L LL. LLL.); ERL

DOŒE ES push h]

DOEF  CDDFCA call CADF aller chercher numéro de ligne ERROR

DOF2 180E jr D102

CÉLELLSLLLLLSLLSLLLLSL LILI LL ÉELLS SEE) HIMEM

DOF4 ES push hl

DOF5S 2A7BAE ld h1, (AE7B) HIMEM

DOF8 1808 jr D102

-11I 64-

BASIC

REED DE DE ME DE DEEE DE DE DE HD DE HE DE DE EEE NÉ DE NE

DOFA CDS9OD6 call D690
DOFD D2ABCE jp nc, CEAB
D100 ES5 push hl

D101 EB ex de,hl
D102 CD6OFE call FE60
D105 EI pop hl

D106 C9 ret
LÉLLLILÉELSLLLSLELLLRLSLLLLLSILLZLSSS:
D107 ES5 push hl

D108 CDC6BB call -  BBC6
D10B EB ex de,hl
D10C 1804 Jr D112
DOMODOHEDEDEDEHEDE MEN HE DEEE DEEE
D10E ES push hl

D10F CDC6BB call BBC6
D112 CDODFF call FFOD
D115 El pop hl

D116 C9 ret

HOME DD HD DD DD D DEEE DEEE DE
D117 CD37DD call DD37
D11A Eu db E4

D11B EB ex de,hl
D11C CDD6DD call DDD6
D11F EB ex de,hl
D120 1E0C id e,0C
D122 D294CA Jp nc, CA94
D125 CDA2D6 call D6A2
D128 EB ex de,hl
D129 73 1d (hl),e
D12A 23 inc hl

D12B 72 ld (hl),d
D12C EB ex de,hl
D12D C3EFES8 JP E8EF

LÉELES ES LLELS LL SS LE LES LL ELLES LS LE)

1.0

‘4j', pointeur de variable
aller chercher adresse de variable
absent, ‘’Improper argument’

accepter valeur

XPOS

GRA ASK CURSOR

YPOS

GRA ASK CURSOR
accepter nombre entier dans hl

instruction Basic DEF

tester si encore un caractère
‘FN’

prendre numéro de ligne dans hl

‘Invalid direct command’
mode direct, sortir message d'erreur

chercher fonction

ignorer reste de l'instruction

fonction Basic FN

-III 65-

BASIC 1,0

D130 CDA2D6 call D64A2 chercher fonction

D133 C5 push bc

D134 ES push pl

D135 EB ex de,hl

D136 SE id e,(hl)

D137 23 inc hl

D138 56 1d d,(h1)

D139 EB ex de,hl

D13A 7C ld a,h

D13B B5 or 1

D13C 1E12 ld e,12 ‘Unknown user function’
D13E CA9ACA jp z, CA94 sortir message d'erreur
D141 CDO/DA call DAO7

Di44 7E ld a,(hl)

D145 FE28 Cp 28 Aou

D147 202C jr nz,D175

D149 CD3FDD call DD3F ignorer espaces

D14C E3 ex (sp),hl

Di4D CD37DD cal] DD37 tester si encore un caractère
D150 28 db 28 (?

D151 E3 ex (sp),hl

D152 CDABDA call DAUB

D155 E3 ex (sp),hl

D156 DS push de

D157 CDFBCE call CEFB aller chercher expression
DISA E3 ex (sp),hl

D15B 78 ld a,b

D15C CD66D6 call D666

D15F E1 pop hl

D160 CDS5SDD call DD55 virgule suit ?

D163 3007 Jr nc,D16C non

D165 E3 ex (sp),hl

D166 CD3/7DD call DD37 tester si encore un caractère
D169 2C db 2C Eu

D16A 18E6 Jr D152

D16C CD3/DD call DD37 tester si encore un caractère
D16F 29 db 29 1}

D170 E3 ex (sp),hl

-1I1 66-

D171
D174
D175
D178
D17B
D17C
D17F
D182
D185
D188
D18B
D18C
D18D

CD37DD
29
CD27DA
CD37DD
EF
CDFBCE
C27BD0
CD30DA
CDHSFF
CC49FB
ET
F1
C3D7FE

cal]
db
call
call
db
call
jp
call
call
call
pop
pop
jp

DD37
29
DA27
DD37
EF
CEFB
nz, DO7
DA30
FF45
z,FB49
hl

af
FED7

BASIC 1,0

tester si encore un caractère
ou

tester si encore un caractère
lof

aller chercher expression
B *Syntax error’

Tester si chaîne
oui

OO EEREEEEX Fonctions Basic à plusieurs arguments

D190 BAF8 dn F8BA 71, BIN$
D192 EAF8 du F8EA 72, DEC$
D194 C4F8 dw F8C4 73, HEX$
D196 A1FA dw FAAI 74, INSTR
D198 3CF9 dw F93C 75, LEFT$
D19A EEDI dy DIEE 76, MAX
D19C EADI dw DIEA 77, MIN
DIJŒ 76C2 dw C276 78, POS
D1AO 43F9 dw F943 79, RIGHTS
D1A2 19D2 dw D219 7A, ROUND
D1A4 36FA dw FA36 7B, STRINGS
D1A6 E9C4 dw CUE9 7C, TEST
D1A8 EEC4 dw CUEE 7D, TESTR
DIAA ABCE dW CEAB JE, ‘Improper argument”
DIAC 62C2 dw C262 7F, VPOS
LRSRLSSÉELSRESSSRES SSSR ÉESE SSSR SSLER SZ) Adresses des fonctions Basic
DIAE  8SFD dw FD85 00, ABS
D1BO 10FA du FA10 01, ASC
D1B2 3EDS5 dy D53E 02, ATN
D1B4  16FA dw FA16 03, CHR$
D1B6  S8DFE dw FE8D O4, CINT
D1B8 34D5 dy D534 05, COS
DIBA ECFE dy FEEC 06, CREAL
D1BC 20D5 dw D520 07, EXP

“111 67-

DIBE
D1C0
D1C2
D1C4
D1C6
D1C8
D1CA
D1CC
DICE
D1D0
D1D2
D1D4
D1D6
D1D8
D1DA
DiDC
DIDE
D1EO
D1E2
D1E4
D1E6
D1E8

E8FD
2DFC
O9D4
6DF1
EDFD
23D4
OAFA
2AD5
25D5
34F8
58F1
gFC9
O2FF
2FD5
57FA
29D3
EFD4
1EF9
39D5
C2FE
42F8
77FA

dn
dn
dn
dn
dw
dw
dw
dw
dw
dw
dw
dw
dn
dw
dw
dw
dn
dw
dw
dy
dw
dw

FDE8
FC2D
D409
F16D
FDED
D423
FAOA
D52A
D525
F834
F158
C99F
FFO2
D52F
FA57
D329
DUEF
F91E
D539
FEC2
F842
FA77

BASIC 1.0

08,
09,
OA,
OB,
OC,
OD,
OE,
OF,
10,
11,
12,
13,
14,
15,
16,
17,
18,
19,
1A,
1B,
ic,
1D,

-I11 68-

FIX
FRE
INKEY
INP
INT
JOY
LEN
LOG
LOG610
LOWER$
PEEK
REMAIN
SGN
SIN
SPACES
sq

SGR
STR$
TAN
UNT
UPPER$
VAL

BASIC 1.0

LÉLRÉSSLSLSSÉLESSSLESLSLLLLLLLLLLLLS SZ] fonction Basic MIN

D1EA O6FF ld b,FF Flag pour MIN

DIEC 1802 jr D1F0

LÉLLLLLESLÉE LES SSSLLSISSSLISLLSLSLSLS fonction Basic MAX

DIEE 0601 Id b,01 Flag pour MAX

D1FO CDFBCE call CEFB aller chercher expression

D1F3 CDSSDD call DD55 virgule suit ?

D1F6 301€ Jr nc,D214 non, terminé

D1F8 CDS3FF call FF53 placer variable sur la pile Basic

D1FB CDFBCE call CEFB aller chercher expression

DIFE ES push hl

DIFF 79 id a, C

D200 CDAOFS call F5A0 libérer place dans la pile Basic

D203 C5 push bc

D204 ES push hl

D205 CDO9FD call FDO9 comparaison arithmétique

D208 EI pop hl

D209 C1 pop bc

D20A B7 or a

D20B 2804 jr z,D211

D20D B8 Cp )

D20E  C4UUEFF call nz,FFUE aller chercher résultat de la
comparaison

D211 EI pop h]

D212 18DF jr D1F3 argument suivant

D214 CD3/7DD cal] DD37 tester si encore un caractère

D217 29 db 29 4

D218 C9 ret

HEC DEEE DD EEE HE DEEE ÉD HE EEE HE fonction Basic ROUND

D219 CDFBCE call CEFB aller chercher expression

D21C CDS3FF call FES et placer sur la pile Basic

D21F  CDSSDD call DD55 virgule suit ?

D222 110000 ld de, 0000 valeur par défaut 0

D225  DC86CE call c,CE86 oui, aller chercher valeur entière
avec signe

D228 CD3/DD call DD37 tester si encore un caractère

D22B 29 db 29 60

-IIT 69-

BASIC 1.0

D22C ES push hi

D22D DS push de

D22E 212700 ld h1,0027 39

D231 19 add hl,de additionner

D232 114F00 ld de,004F 79

D235 CDB8FF cal} FFB8 comparer hl <> de

D238 D2ABCE Jp nc, CEAB supérieur, ‘Improper argument’
D23B D1 pop de

D23C 79 ld a,C

D23D CDAOFS call F5SAO libérer place dans la pile Basic
D240 43 1d b,e nombre d'arrondissement dans b
D241  CDAFFD call FDAF arrondir le nombre

D244 E] pop hi

D245 C9 ret

LELLLE SSL RS ÉES SSID ESTLISSISLISLSLSSSS) instruction Basic CAT

D246 CO ret nz

D247 ES push hl

D248  CDADD2 call D2AD interrompre 1/0 cassette

D24B CD37F6 call F637 mettre en place buffer de sortie
D24E  CD9BBC call BCOB CAS CATALOG

D251 CD/71F6 call F671 libérer buffer de sortie

D254 E1 pop hl

D255 C9 ret

HORMONE DE DEMO HE MERE HOME EME HE EH HE HE EN instruction Basic OPENOUT
D256 CD/73D2 call D273 aller chercher nom de fichier
D259 CD3/7F6 call F637 organiser buffer de sortie
D25C C38CBC jp BC8C CAS OUT OPEN

HOECHOROHOHOEHOHOHOHOE NE NE DE DE HE EE HE EH HE HE GE EEE instruction Basic OPENIN

D25F  CD6AD2 call D26A aller chercher nom, ouvrir fichier
D262 FE16 CP 16 fichier ASCII ?

D264 C8 ret Z

D265 1E19 ld e,19 ‘File type error’

D267 C39uCA jp CA94 sortir message d'erreur

ROC EEK HEXEE  A]]er chercher nom, ouvrir fichier d'entrée
D264 CD/3D2 cal] D273 aller chercher nom de fichier
D26D CD32F6 call F632 mettre en place buffer d'entrée

-I11 70-

BASIC

D270 C377BC jp BC77
LELÉELLLÉESSLLSLLSSSLS SSSR SSSSÉRSSSRS ESS]
D273 CDSFCE call CE9F
D276 E3 ex (sp},hl
D277 EB ex de,hl
D278 CD85D2 call D285
D27B CAG6BCB JP z, CB6B
D27E EI] pop hl

D27F D8 ret C

D280 1E1B 1d e,1B
D282 C394CA Jp CA94
CELELLLLLLLLLLLLSSSLLRSSLSLSLLLSS LS]
D285 DS push de

D286 0E00 ld c,00
D288 78 1d a,b

D289 B7 or a

D28A 2808 jr z,D294
D28C 7E Id a,(hl)
D28D FE21 Cp 21

D28F 2003 Jr nz, D294
D291 23 inc hi

D292 05 dec D

D293 OD dec C

D294 79 ld a,C

D295 C36BBC Jp BC6B

CELL LLLLLLLLLLLLSLLLSLSLLLELSISELE LS)
D298 ES push hl

D299 CD7ABC call BC7A
D29C CD6DF6 call F66D

D29F Ei pop hl

D2A0 C9 ret

DD DE DE DE DE DE ADDED DE DE DEEE DEEE EE EEE
D2A1 E5 push hl

D2A2 CD8FBC call BC8F
D2A5 CAG6BCB JP z, CB6B

1.0

CAS IN OPEN

aller chercher paramètres et
expression de chaîne

tester si messages du système
interruption par ESC’

‘File already open’
sortir message d'erreur

sortir flag pour messages
longueur du nom de fichier

pas de nom de fichier ?

premier caractère

11!

non

fixer pointeur sur second caractère
diminuer longueur

interdire flag pour messages

CAS NOISY
instruction Basic CLOSEIN

CAS IN CLOSE
libérer buffer d'entrée

instruction Basic CLOSEOUT

CAS OUT CLOSE
‘Break in numéro de ligne’

-III 71-

D2A8
D2AB
D2AC

CD71F6
ET
C9

call
POP
ret

BASIC

F671
hl

HIER DE HE DE DE DEN DE DE DH DE HE HEDEDE DEEE DEN EEE

D2AD
D2AE
D2AF
D2B0
D2B3
D2B6
D2B9
D2BC
D2BD
D2BE
D2BF

C5
D5
ES
CD7DBC
CD6DF6
CD92BC
CD71F6
ET
D1
C1
cg

push
push
push
call
call
call
call
pop
pop
pop
ret

pc
de
hl
BC7D
F66D
BC92
F671
hl
de
bc

LELLL ESC ELLES LLLLL ELLE LL ELLES TSX)

D2C0
D2C3
D2C6
D2C9
D2CA
D2CD
D2D1
D2D4
D2D7

D2DA
D2DE
D2E1

D2E4
D2E7
D2E9

D2EC
D2EF

D2F2

CD67CE
32B2AD
CD37DD
2C
CDFFD3
ED53B5SAD
CD55DD
111400
DC86CE

ED53B9AD
010010
CDODD3

32B8AD
0E00
CDODD3

32B3AD
CDODD3

32B4AD

call
id
cal]
db
call
Id
call
ld
call

ld
ld
call

Id
1d
call

id
call

id

CE67

(ADB2),a

DD37

2C

D3FF

(ADB5), de

DD55

de,0014
c, CE86

CADB9), de
bc, 100€
D30D

(ADB8),a
c,00
D30D

CADB3),a
D30D

CADB4), a

1.0

libérer buffer de sortie

interrompre I/0 cassette

CAS IN ABANDON
libérer buffer d'entrée
CAS OUT ABANDON
libérer buffer de sortie

instruction Basic  SOUND
aller chercher valeur 8 bits
état canal

tester si encore un caractère

1 ot
,

aller chercher argument 0 à 4095
période du ton
virgule suit ?
valeur par défaut 20

oui, aller chercher valeur entière
avec signe
Durée
max, 15, défaut 12

aller chercher argument s’il y en a
un
volume
max. 15, défaut O0

aller chercher argument s'il y en a
un
courbe d'enveloppe de volume

aller chercher argument s’il y en a
un
Courbe d'enveloppe du ton

-III 72-

D2F5
D2F7

D2FA
D2FD

D300
D301
D304
D307
D308
D309
D30A

0620
CDODD3

32B7AD
CDHADD

ES
21B2AD
CDAABC
ET
D8
F1
C371DD

ld
call

ld
call

push
ld
call
pop
ret
pop
Jp

BASIC

b, 20
D30D

(ADB/),a
DDHA

hl
h1,ADB2
BCAA

hl

(o

af

DD71

HEOMOMDEONEDEE DE HD DE DD DE DD DEEE DE HE DE DM HÉEHE DE DE N

D30D
D310
D311
D312
D315
D315
D316
D317
D31A
D31B
D31C

CD55DD
79

DO

7E
FE2C
79

C8
CD67CE
B8

D8
182B

call
ld
ret
id
cp
ld
ret
call
cp
ret
Jr

DD55
a,C
nc
a, (hl)
2C
a,C
Z
CE67
b

C
D349

HOHNE DEN DE DE HEHONE DEN DE DE DEN DEN DD DEN EE DEEE NE

D31E
D320
D323
D324
D327
D328

0608
CDIi7D3
ES
CDB3BC
E1

C9

ld
call
push
call
pop
ret

b,08
D317
hl
BCB3
hl

LÉLÉRSL SSL SSL LLLLLLLLLLLLSLLLLLS SE)

D329
D32C
D32D

CD8DFE
7D
B7

call
ld
or

FE8D
a, |
a

1.0

max, 31, défaut O

aller chercher argument s'il y en a
un

période bruit

fin de l'instruction, sinon ‘Syntax
error’

Adesse bloc paramètres Sound
SOUND QUEUE

à la boucle de l’interpréteur

aller chercher valeur 8 bits s’il y
en a une

virgule suit ?

charger valeur défaut

pas virgule, terminé

L OE
,

aller chercher valeur 8 bits holen
supérieur égal D ?

non

‘Improper argument’

instruction Basic RELEASE
8
aller chercher valeur 8 bits < 8

SOUND RELEASE

fonction Basic SQ
CINT

- III 73-

BASIC

D32E 1F rra

D32F 3806 jr c,D337
D331 1F rra

D332 3803 jr c,D337
D334 1F rra

D335 3012 Je nc, D349
D337 B4 or h

D338 200F jr nz, D349
D33A 7D id a, |
D33B CDADBC call BCAD
D33E C3OAFF jp FFOA
LÉSSLLLSLLLSSLLLSSLISLISLLILLLILISLSS.
D341 CD86CE call CE86
D344 7B ld a,e
D345 87 add a,à
D346 SF sbc a,a
D347 BA CP d

D348 C8 ret Z

D349 1E05 1d e,05
D34B C39ACA Jp CA94
HORMONE MEME ME DEMO HEHE MED HE MED MEME ME EEE
D34E  CD6DCE call CE6D
D351 FE10 CP 10

D353 30F4 Jr nc, D349
D355 F5 push af

D356 116/7D3 ld de, D367
D359 CDD8D3 call D3D8
D35C F1 pop af

D35D E5 push hl

D35E 21BBAD ld h1,ADBB
D361 71 ld (hl),c
D362 CDBCBC call BCBC
D365 El pop hl

D366 C9 ret

1.0

’Improper argument’

‘Improper argument’

SOUND CHECK
accepter contenu accu comme nombre
entier

aller chercher argument -128 à +127
aller chercher valeur entière avec
signe

‘Improper argument’
sortir message d'erreur

instruction Basic ENV
aller chercher valeur 8 bits non
nulle
supérieur égal 16 ?
"Improper argument”

SOUND AMPL ENVELOPE

-111 74-

BASIC 1,0

D367 7/E ld a, (hl)

D368 FEEF CP EF "=!

D36A 2012 Jr nz, D37E

D36C CD3FDD call DD3F ignorer espaces

D36F 0610 Id b,10 16

D371 CD1/D3 call D317 aller chercher valeur 8 bits < 16
D374 F680 or 80 mettre bit 7

D376 4F ld C,a

D377 CD3/DD call DD37 tester si encore un caractère
D37A 2C db 2C Pot

D37B C391CE Jp CE91 aller chercher valeur 16 bits
D3/E 0680 ld b, 80 128

D380 CD1/D3 call D317 aller chercher valeur 8 bits < 128
D383 1840 jr D3C5

LÉDÉLSÉEELLLLLERSLLLSLSLLLESLSLLELLLES ST: instruction Basic ENT

D385 CD41D3 call D341 aller chercher argument -128 à +127
D388 7A id a,d

D389 B7 or a

D38A 7B id a,e

D38B 2802 jr z,D38F zéro ?

D38D 2F cpl a

D38E 3C inc a

D38F 5F Id e,a

D390 B7 or a zéro ?

D391 28B6 jr z,D349 "Improper argument’

D393 FE10 cp 10 supérieur égal 16 ?

D395 30B2 jr nc, D349 "Improper argument’

D397 DS push de

D398 11AED3 ld de, D3AE

D39B CDD8D3 call D3D8

D3SŒ Di pop de

D39F ES push hl

D3A0 21BBAD ld h1,ADBB

D343 7/A id a,d

D3A4 E680 and 80

D3A6 B1 or C

D3A7 77 1d {hl),a

D3A8 7B ld a,e

=I-785

D3A9
D3AC
D3AD

CDBFBC
EI
C9

call
POP
ret

BASIC 1.0

BCBF SOUND TONE ENVELOPE
hl

JE HRDEHDEDEHDHDEDEDEDENEDEDEHEUECH HE DEEE DEEE DE

D3AE
D3AF
D3B1
D3B3
D3B6
D3B9
D3BA
D3BC
D3BD
D3BE

D3C0
D3C2
D3C5
D3C6
D3C9
D3CA
D3CD
D3CE
D3D1
D3D2
D3D5
D3D6
D3D7

D3D8
D3DB
D3DE
D3E0
D5E1
D3E2
D3E5
D3E6
D3E7
D3E8

7E
FEEF
200D
CD3FDD
CDFFD3
7A
C6FO
UF

43
180€

06FO
CD17D3
UF
CD37DD
20
CD41D3
43
CD37DD
2C
CD67CE
57
58
cg

010005
CDS5DD
301€
D5

C5
CDFBFF
79

C1

C5

ES

ld
cp
jr
call
call
1d
add
ld
ld
Jr

1d
call
ld
call
db
call
id
call
db
call
id
ld
ret

id
call
jr
push
push
call
ld
POP
push
push

a,(hl)

EF "=!

nz, D3C0

DD3F ignorer espaces

D3FF aller chercher valeur de O0 à 4095
a,d

a,F0

C,a

b,e

D3CE

b,FO 240

D317 aller chercher valeur 8 bits < 240
c,a

DD37 tester si encore un caractère

2C Ryie ;

D341 aller chercher argument -128 à +127
b,e

DD37 tester si encore un caractère

2C 5

CE67 aller chercher valeur 8 bits

d,a

e,b

bc, 0500

DD55 tester si virgule
nc, D3FC

de

bc

FFFB jp (de)

a,C

bc

bc

hl

-I11 76-

BASIC

D3E9 21BCAD id h1, ADBC
D3EC 0600 ld b,00
D3EE 09 add hl,bc
D3EF 09 add hl,bc
D3F0 09 add hl,bc
D3F1 77 ld (hl),a
D3F2 23 inc hl
D3F3 73 ld (hl),e
D3F4 23 inc hl
D3F5 72 ld (hl),d
D3F6 El pop hl
D3F7 C1 pop bc
D3F8 OC inc C

D3F9 D1 pop de
D3FA 1ODF djnz D3DB
D3FC C34ADD jp DD4A

HHEDEME DE DE DH HD MED DE DEEE DD EH HÉDE DEEE DE DE GENE

D3FF  CD86CE call CE86

D402 7A Id a,d
D403 E6FO and FO

D4O5S C249pD3 JP nz,D349
D4O8 C9 ret

HEOEHDEDE DE DE HEHEHEHEDEDE DEEE DEEE ED DE DEEE DEEE HE HE

D409  CD8DFE call FE8D
D40OC 115000 id de, 0050
DUOF  CDB8FF call FFB8
D412 3022 Ir nc,D436
D414 JD Id a, 1
D415 CDIEBB call BBIE

D418 21FFFF 1d h1,FFFF
D41B 2803 Jr z,D420
D41D 69 id 1,C
DH1E 2600 ld h, 00
D420 C3ODFF Jp FFOD

LÉLSSSELELLEL SSL SL LSEL LLLELELLLL ELLE)

1.0

fin de l'instruction, sinon ’Syntax
error’

aller chercher argument 0 à 4095
aller chercher valeur entière avec

signe

Hi-Byte

Bits 12-15 mis ?

oui, ‘’Improper argument

fonction Basic INKEY
CINT

80

comparer hl <> de
’Improper argument’

KM TEST KEY
-1, si pas enfoncée

accepter nombre entier dans h]

fonction Basic JOY

III 77-

BASIC

D423 CD24BB call BB24

D426 EB ex de,hl
D427 CDS8DFE call FE8D
D42A 7C ld a,h

D42B B5 or Il

D42C 2802 jr z,D430
D42E 53 ld d,e

D42F 2B dec h1

D430 7C ld ah

D431 B5 or 1

D432 7A ld a,d

D433  CAOAFF jp Z,FFOA
D436 C349D3 jp D349
LÉLLLILLLLLLLSLSLES SLR LSLSLSLS STE
D439 FE8D CP 8D

D43B 2819 jr z,D456
D43SD 3F20 Id a, 20

D43F  CD17D3 call D317

D442 F5 push af

D443 CD3/7DD call DD37
D4u6 2C db 2C

D447  CD9FCE call CE9F
DAUA 48 id C,D
DB F1 POP af

DuuC 47 id b,a
D4uD E5 push pl

DUUE EB ex de,hl
DHUF  CDOFBB call BBOF
D452 E1 POP hi

D453 30E1 jr nc,D436
D455 C9 ret

SECHE HHOHEHEOHOHODEHHEEOHE EE HE HE DE EE SEE DEEE EE NE EH

D456  CD3FDD call DD3F
D459  CD67CE call CE67
Tastennnummer

1.0

KM GET JOYSTICK

CINT

accepter contenu accu comme nombre
entier
‘Improper argument’

instruction Basic KEY
DEF’

aller chercher argument, valeur 8
bits
ranger numéro de touche
tester si encore un caractère
aller chercher expression et
paramètres de chaîne
longueur de chaîne dans c

numéro de touche dans D

adresse de chaîne dans hl
KM SET EXPAND

‘Improper argument’
KEY DEF

ignorer espaces
aller chercher valeur 8 bits,

-111 78-

BASIC

D4SC UF ld c,a
D4SD FES cp 50

D4SF  30D5 jr nc, D436
D461 CD37DD call DD37
Du64 2C db 2C

D465 0602 ld b,02
Du67 CD17D3 call D317
DUG6GA  1F rra

D46B JF sbc a,a
DU6C 47 Id b,a
D46D C5 push bc

DU6E ES push hl

D46F 79 1d a,C
D470 CD39BB call BB39
D473 E1 pop hl

Du74 C1 pop bc

D475 1127BB Id de,BB27
D478 CD84D4 call D4gù
D47B 112DBB ld de,BB2D
D47E CD84D4 call D484
D481 1133BB Id de, BB33
D484  CDSSDD call DD55
D487 DO ret nc

D488 DS push de

D489 CD67/CE call CE67
D48C 47 ld b,a
Du8D E3 ex (sp),hl
DU8E 79 ld a,C
D48F  CDF8FF call FFF8
D492 E] pop hl

D493 C9 ret
LLLRELLSSLLLLSLSELLLÉESSLSLRSLLELLLSEL.
D494 FEA4 cp A4

D496 013FBB Id bc,BB3F
D499 2810 jr Z, DHAB
D49B FEA2 cp A2

D49SD 013EBC ld bc, BC3E
D4AO 2809 jr Z,D4AB
D4A2 FED9 cp Dg

1.0

80
supérieur égal, ‘’Improper argument’
tester si encore un caractère

11
,

2
aller chercher argument < 2

KM SET REPEAT

KM SET
tester
KM SET
tester si encore un argument
KM SET CONTROL

virgule suit ?

non, terminé

TRANSLATE
si encore un argument
SHIFT

aller chercher valeur 8 bits

Jp (hl)

instruction Basic SPEED
KEY”
KM SET DELAY

"INK
SCR SET FLASHING

"WRITE"

-111 79-

D4Au
DHAG
D4AS

281D
1E02
C394CA

jr
Id
jp

BASIC

z,DaC3
e,02
CA94

EE HERMIONE HE ME D EEEÉDEJÉEEDE EE DE EN

DUAB
DHAC
D4AF

D4B2
D4B3
D4B6
D4B7

DABA
DABB
D4BC
DABD
DUBE
D4C1
D4C2

C5
CD3FDD
CD6DCE

BF
CD37DD
2C
CD6DCE

5F
51
C1
EB
CDF9FF
EB
cg

push
call
cal]

1d

call

db
call

id
1d
pop
ex
call
ex
ret

bc
DD3F
CE6D

C,a

DD37

2C
CE6D

e,a
d,c
bc
de,hl
FFF9
de,hl

HD DE DEN NH DE OHECHE HE DEEE HE DH EEE

D4C3
D4C6
D4C8
DACB
D4CC
D4CF
D4DO
DuD2
D4D4
D4DS
D4D6
D4Dg
DADA

CD3FDD
0602
CD17D3
ES
214700
3D
3E32
2802
29

OF
CD68BC
E1

C9

call
1d
call
push
id
dec
ld
jr
add
rrca
call
pop
ret

DD3F

b, 02
D317

nl
hl,00A7
à

a, 32

Z, D4D6
hLhi

BC68
hl

HO N HE MEDENEDE DE DEEE NME DEN DE HE DE DE DEMO DEN DEEE

D&DB
D4DC

ES
CDI9FF

push
call

hl
FF19

1.0

’Syntax error’
sortir message d'erreur

SPEED KEY & INK

ignorer espaces

aller chercher valeur 8 bits non

nulle

tester si encore un caractère

1!
,

aller chercher valeur 8 bits non

nulle

Jp (bc)

SPEED WRITE
ignorer espaces

aller chercher argument < 2

167

zéro, ?

non, doubler constante de temps

CAS SET SPEED

PI

fixer type sur ‘Real’

—111 80-

D4DF

DHE2
DES
D4E6

EH DE DEEE D DEN HD DE DE DE HD DE DE DE HE HEDEDE DEEE DEN

D4E7
D4E9

HEHOHEDEHEDEDE HD DEN DE DE DE HD HE DE DE DE EDEN

DUEB
DUEC

HEROHEODEDEDEDEHE HD DD HEDE HE DE HE DEN DE HD DE DE DEEE DE DE DE HE HE

DUEF
D4F2

EDEDEDEHEHEDE HD DE DE DH HE DE DEEE DE DE DE DE HE DE DE DEEE DE ME HEN

D4F4
D4F5
D4F6
D4F9
DFA

D4FD
D500
D501
D502
D503
D506
D507
D50A
D50D
D50E
D511
D514
D516

CD1DFF

CD/6BD
ET
C9

3EFF
1801

AF
C373BD

0179BD
1816

ES

C5

CDECFE

EB
21CBAD

CD3DBD
C1

E3

79
CD4BFF
D1
01/CBD
CD19D5
D8
CAEACA
FAF3CA
1E05
C394CA

call

call
pop
ret

1d
jr

XOr -

JP

ld
Jr

push
push
call
ex
1d

call
pop
ex
ld
call
pop
ld
call
ret
JP
JP
ld
JP

BASIC 1

.0

FF1D type de variable dans c, hl sur

variable

BD76 aller chercher 4S

hl
instruction Basic DEG

a,FF FF = DEG

DUEC
instruction Basic RAD

a 0 = RAD

BD/3 fixer mode DEG/RAD
fonction Basic SGR

bc,BD/79 fonction SGR

DSOA
opérateur Basic ‘°'

hi

bc

FEEC CREAL

de,hl

h1,ADCB mémoire provisoire pour nombre à

virgule flottante

BD3D copier variable de (de) dans (h1)

bc

(sp),hl

a,C

FFUB

de

bc,BD7C élévation à la puissance

D519 exécuter fonction

C sans erreur ?

Z, CAEA ‘Division by zero’

m, CAF3 ’Overflon’

e,05 ’Improper argument’

CA94 sortir message d'erreur

HOMO OH OK ON ON OK KM KO NOK ON WOK NM M M OK NM M OK NON OX HO NO X OX X

-111 81-

D519
D51A
D51B
DS1E

C5
D5
CDECFE
D1

push
push
call
POP

BASIC 1.0

bc
de
FEEC CREAL
de

-JII 82-

BASIC 1.0

D51F C9 ret exécuter fonction
ÉTESSSSLLLLLLLSSRSSLLSLSSSÉRSRSLSLSLS SES fonction Basic EXP
D520 0O185BD Id bc,BD85 EXP (fonction)

D523 18E5 jr DSOA

MERDE DE DEN DE DE DD DE DE DEEE DE DE DE D DE DE DE DEEE DE DE D fonction Basic L0610
D525 0182BD 1d bc,BD82 L0610 (fonction)
D528 18E0 jr DSOA
LÉLRÉRRLLLLLÉERSSLSLSSLLLÉESSLSLSLSERSS SZ] fonction Basic LOG
D52A 017FBD ld bc,BD/F LOG (fonction)

D52D 18DB Jr DSOA

OHMODEDEMEDEDEDEDE DE DEEE DE DEN DE HE NE DEDDEDEHUNE fonction Basic SIN
D52F 0188BD id bc,BD88 “SIN (fonction)

D532 18D6 jr D50A

OHEDEODEOMEUEDEDEMEHE DEEE DE DD HE DE DD fonction Basic COS
D534 O18BBD ld bc, BD8B COS (fonction)

D537 18D1 jr D5OA
LÉSRLLLLLLLSSSSLLSLLLRSLSSSIRSRSLLE SE. fonction Basic TAN
D539 O18EBD ld bc, BD8E TAN (fonction)

D53C 18CC jr DSOA

DOME DEEE DE DE DE DE DE HD D DE DEEE DE DE DE DEHEDEHE DE EE fonction Basic ATN
D53E 0191BD ld bc, BD91 ATN (fonction)

D541 18C7 jr DSOA

LÉLLELLL IIS S ESS SLLISL ESS SSL SL SELS)

D543 652 61 6E 64 6F 6D 20 6E ‘Random number seed ?’

D54B 75 6D 62 65 72 20 73 65
D553 65 64 20 3F 20 00

HEDDDEDEDE DE DEEE DE DE DE DE DE DE DE NE DE DE DE DE HE D HE NÉ JE DE EE instruction Basic RANDOMIZE
D559 2806 jr z,D561

D5S5B CDFBCE call CEFB aller chercher expression
D55E ES push h]

D55F  181B jr D57C

-]11 82-

D561
D562
D565
D568
D56B
D56E
D571
D574
D576
D579
D57A
D57C
D57F
D582
D583

ES
2145D5
CD41C3
CD3BCA
D26BCB
CD4EC3
CDA3EC
30EC
CD61DD
B7
20E6
CDECFE
CDSABD
ET

C9

push
ld
call
call
JP
call
call
jr
call
or
Jr
call
call
pop
ret

BASIC

hl
h1,D543
c341
CA3B
nc, CB6B
C3HE
ECA3
nc, D562
DD61

a

nz, D562
FEEC
BD9A

hl

ROROMONOHONHEHEHEHEDE DEEE DEEE DE MMM DEEE ED DEEE

D584
D585
D587
D589
D58C
D58F
D592
D593
D594
D597
D59A
D59C
D59F
D5AO

D5A1
DSA4
D5A5
D5A6
D5A9
D5SAC
D5AD

7E
FE28
201C
CD3FDD
CDFBCE
CD37DD
29

ES
CDECFE
CD/CBD
2005
CDAOBD
E

cg

FCSABD
ET
ES
CD16FF
CD9DBD
EI
C9

ld
CP
Jr
call
cal]
call
db
push
call
call
jr
call
pop
ret

call
pop
push
cal]
call
pop
ret

a,(h1)
28

nz, DSAS
DD3F
CEFB
DD37
29

hl

FEEC
BD70
nz, D541
BDAO
hi

m, BD9A
hi

nl
FF16
BD9D
h1

1.0

‘Random number seed ?'

sortir

aller chercher ligne d'entrée
touche ESC enfoncée ?

sortir LF

lire entrée

non valable, recommencer
ignorer espace, TAB et LF

CREAL
Set Random Seed

RND

CN QE

ignorer espaces

aller chercher expression
tester si encore un caractère
Fr

CREAL

SGN

différent zéro ?

aller chercher dernière valeur RND

Set Random Seed

fixer type sur virgule flottante
RND

-III 83-

BASIC

DENON DH DEDE MED DHEA DE DE DE DE JE DE

DSAE  CDBEDS call DSBE

1.0

réinitialiser pointeur de variable
vider table

D5B1 2A83AE ld h]l, (AE83) fin du programme

D5B4 2285AE 1d (AE85),h1 début des variables

DSB7 2287AE ld (CAE87),h1l début des tableaux

DSBA 2289AE ld (AE89),hi fin des tableaux

DSBD C9 ret

EOHOHN DEN DE DD DE D DE DE DE DE DE HE DE DE DE DE DE DE DEDEÉNE DÉEEN vider tables

DSBE  21DOAD ld h1,ADDO

D5C1 3E36 id a, 36 54 = 2*27, variables + fonctions
D5C3 CDCBDS call D5CB supprimer de ADDO à AEOS5
DSC6 2106AE ld h1,AE06

D5C9 3E06 1d a, 06 supprimer de AE06 à AEOB
DSCB 3600 id (h1),00 tableaux

DSCD 23 inc hl

DSCE 3D dec a

DSCF 20FA jr nz, DSCB

D5D1 C9 ret

EDEN DD DEDE DE DE DE DE HD HE DE DE DEEE HE DE DEEE DEEE HE DE DEEE supprimer flag pour EN
D5D2 210000 id h1,0000

DSDS 22O04AE Id CAEOG),h1

DSD8 C9 ret

HEOUODOMONDEDEDEDE DE DE DE DE DE HE DEEE HE DE HE DEHE EE DE calculer adresse de table
DSD9 3E5B 1d a, 5B /1'+1, FN

D5SDB 2A85AE ld h1, (AE85) début des variables

DSDE 2B dec hl moins 1

DSDF 44 Id b,h dans bc

DSEO 4D ld CA

DSE1 87 add a,a fois 2

D5E2 CGLUE add a, HE

DSEU 6F ld l,a plus AD4HE

DSES CEAD adc a, AD donne ADDO - AEO2 pour ‘A’ - 7!
DSE7 95 SUD 1

DES 67 Id ha

D5E9 C9 ret

EEE DEEHEDE DE DE DE HD HE DEN DE DE DE DE JE JE JE DE

calculer adresse de table pour tableaux

-III 84-

BASIC 1.0

DSEA 2A87AE ld hl, (AE87) début des tableaux

D5SED 2B dec hl moins 1

DSEE 44 id b,h

DSEF 4D id C1 dans bc

DSFO E603 and 03

D5SF2 3D dec a

DSF3 87 add a,à

D5F4 C606 add a,06

D5F6 6F ld 1,a plus AE06

DSF7 CEAE adc a, AE

D5F9 95 sub ]

DSFA 67 ld ha

DSFB C9 ret

ACIER REA AO EE A EEE toutes les variables sur le type REAL
DSFC 015441 ld bc, 415A "AZ!

DSFF  1E05 ld e,05 Real”

D601 79 ld a,C

D602 90 sub b nombre dans a

D603 383D jr c,D642 inférieur 1, ’Syntax error’
D605 E5 push hl

D606 3C inc a

D607 21CBAD ld hl, ADCB Base de la table = ADCB+'A'
D60A 0600 1d b, 00

D60C 09 add hl,bc lettre pointeur dans table
D60D 73 ld (hl),e sauvegarder type

D60E 2B dec hl

D60F 3D dec a toutes les lettres

D610 20FB Jr nz, D60D

D612 El pop hi

D613 C9 ret

HOROHOROHOHOHOUOHMOHODEONEOMOMEOHEMEHE DEEE EE DEEE HE EE EEE instruction Basic DEFSTR
D614 1E03 Id e,03 chaîne”

D616 1806 jr D61E

LÉLLLSLLLLSSILSILÉESSLRSLLLSSEL TELL LS instruction Basic DEFINT
D618 1E02 id e,02 ‘Integer’

D61A 1802 jr D61E

-111 85-

BASIC 1.0

HORDE EH HE DE DEN DE DE DE DE DE DEEE DE DE DE DE ME DEEE DEEE JE instruction Basic DEFREAL
D61C 1E05 1d e,05 Real’

D61E 7E ld a,(hl) aller chercher lettre

D61F CD/1FF call FF71 tester si lettre

D622 3OI1E jr nc, D642 'Syntax error’

D624 UF ld c,a dans bc (de - à)

D625 47 id b,a

D626 CD3FDD call DD3F ignorer espaces

D629 FE2D CP 2D ie

D62B 200€ jr nz,D639

D62D CD3FDD call DD3F ignorer espaces

D630 CD71FF call FF71 tester si lettre

D633 300D jr nc, D642 ’Syntax error’

D635 4F ld C,a à

D636 CD3FDD call DD3F ignorer espaces

D639 CDO1D6 call D601 fixer type de variable
D63C CDS5DD call DD55 virgule suit ?

D63F 38DD jr c,D6ÎE oui, traiter variable suivante
D641 C9 ret

D642 1E02 id e,02 ’Syntax error’

D644 1806 jr D64C

D646 1E09 id e,09 Subscript out of range’
D648 1802 jr D64C

D64A 1EOA ld e,OA ‘Array already dimensioned'
D64C C39uCA Jp CA94 sortir message d'erreur
CELLLLLSSLSSLSLLSSLSLLISSSELS RS LES LS SE)

D6UF FEF8 CP F8

D651 CAAOF] JP z,F1A0 extension d'instruction
ÉTILLELLLLLLLLLSLISLSSLS RSS SLSLSL ES instruction Basic LET
D654  CD86D6 cal] D686 aller chercher variable
D657 DS push de

D658 CD37DD call DD37 tester si encore un caractère
D65B EF db EF =!

D65C CDFBCE call CEFB aller chercher expression
D65F 78 ld a,b

-I11 86-

BASIC

D660 E3 ex (sp),hi
D661 CD66D6 call  D666
D664 E1 pop hl

D665 C9 ret

HERO HE DE HE DEMO EE HE EME DE HE ME EEE HE M EME
D666 47 id b,a
D667 CD23FF call  FF23
D66A B8 cp b

D66B 78 Id a,b
D66C C4D7FE call  nz,FED7
D66F CDUSFF call  FF45
D672 C262FF jp nz, FF62
D675 E5 push hl

D676 CD59FB call  FB59
D679 D pop de

D67A C366FF Jp FF66
EEE DE DE DEEE DEEE DE HE HE ME DEH HE EDEN HE DÉ EE
D67D CDB5D7 call  D7B5
D680 CD55DD call  DD55
D683 38F8 ir c,D67D
D685 C9 ret

OH HEME ED DEN NEED DEEE DEEE HE DEEE EH DEEE HE
D686 CDO6D9 call  D906
D689 CDDBD7 call  D7DB
D68C 3842 jr c,D6DO
D68E 1828 LÉ D6B8
HOHENEONE HD HE MEME DE DE DE DE CHE HE HE DE DE DE HE HE DE DEEE DENON NN
D690 CDO6D9 call  D906
D693 CDDBD7 call  D7DB
D696 3838 Jr c,D6D0
D698 E5 push hl

D699 79 id a,c
D69A CDDBDS call  D5DB
D69D CDDED6 call  D6DE
D6AO 182D jé D6CF

1.0

affecter valeur à variable

affecter valeur à variable
comparer type de variable
et type de résultat

adapter type, sinon ‘Type mismatch'
tester si chaîne
non, copier variable dans (hl)

gestion de chaîne
accepteur pointeur sur chaine

instruction Basic DIM
dimensionnement
virgule suit ?
oui, variable suivante

chercher variable

lire nom de variable

tester si variable dimensionnée
aller chercher type de variable

aller chercher adresse de variable
lire nom de variable

tester si variable dimensionnée
aller chercher type de variable

première lettre
calculer position de table

-]11 87-

BASIC

EHOEHEDE DE DEEE DE DE DE DE HE DE DE DE HE DE EN DEEE DEEE EH

D6A2 CDO6D9
D6AS 3821
D6A7 E5
D6A8 CDD9D5
D6AB CDDED6
D6AE D43DD/
D6B1 181C

call
jr

push
call
call
call
jr

D906
c,D6C8
hl

D5D9
D6DE
nc, D73D
D6CF

REOROHEODEHEHE DEEE DE DE DE D DE DE HE HE DEEE HÉ EM DE DEEE NEED

D6B3 CDO6D9
D6B6 3810
D6B8 E5
D6B9 79
D6BA CDDBDS5
D6BD CDDED6
D6CO 3AC1BO
D6C3 D4u4gD7
D6C6 1807

D6C8 E5
D6C9 2A85ÂE
D6CC 2B
D6CD 19
D6CE EB
D6CF El
D6DO 3AC1BO
D6D3 47
D6D4 UF
D6D5 C9

call
frs
push
id
call
call
id
call
Jr

push
id
dec
add
ex
pop
1d
1d
Id
ret

D906
c,D6C8
hl

a, C
DSDB
D6DE

a, (BOC1)
nc,D749
D6CF

hl

hl, (AE85)
hl

hl,de
de,hl

hl

a, (BOC1)
b,a

C,a

LRLS SL SL LS SL LS SEL LS LS LL LL LL LL.)

D6D6  CDO6D9
D6D9 CDCIE8
D6DC 18F2

call
call
jr

D906
E8C1
D6D0

ARE HDE EDR HE EE DE DE HEHEDE DE DEEE DE DEEE DEEE EDEN HE DEN

D6DE D5
D6DF EB
D6EO 2A2BAE

push
ex
Id

de
de,h1
h1, (AE2B)

1.0

chercher fonction
lire nom de variable

calculer position de table pour FN

mettre fonction en place

lire nom de variable

première lettre

calculer position de table

type de variable

début des variables

type de variable

lire nom de variable
tester si variable indicée
aller chercher type de variable

-I11 88-

D6E3
D6E4
D6E5
D6E7
D6E8
D6E9
DGEA
D6EB
DGEE
D6F1
D6F2
D6F4
D6F5
D6F6
D6F7
D6FA
D6FC
D6FD
D6FE

D6FF
D700
D701

D704
D705
D706
D707

HORMONE DE DE DE DE HHEDEDE HE DEEE DE

D708
D709
D7OA
D70B
D70C
D70D
D/0E
D70F
D710
D711

7C

B5
280E
D5

23

23

C5
010000
CDO8D7
C1
3810
D1

EB

E5
CDO8D7
3803
E1

D1

C9

F1
E
C36DD7

F1
F1
37
C9

7E
23
66
6F
B4
C8
09
E5
23
23

ld
or
Jr
push
inc
inc
push
ld
call
pop
jr
pop
ex
push
call
jr
pop
pop
ret

pop
pop
JP

pop
Pop
scf
ret

ld
inc
ld
1d
or
ret
add
push
inc
inc

a,h

l
Z,D6FS
de

h1

h1

bc
bc,000
D708
DC
c,D704
de
de,hl
hl
D708
c,D6FF
hl

de

af
hl
D76D

af
af

a, (h1)
hl

h, (h1)
l,a

h

Z
hl,bc
hl

h1

hl

BASIC 1,0

0
chercher tableau

trouvé ?

chercher tableau
trouvé ?

chercher tableau

-[11 89-

D712
D713
D716
D717
D718
D71A
D71B
D71C
D71D
D71F
D720
D723
D724
D725
D727
D729
D72A
D72B
D72C
D72D

D72E
D72F

EB
2A27AE
TA

BE
2014
23

15

17
30F7
EB
3AC1B0
3D

AE
E607
2005
EB

13

EI

37

cg

ET
18D7

ex
1d
ld
cp
Jr
inc
inc
rla
Jr
ex
id
dec
xor
and
jr
ex
inc
pop
scf
ret

pop
Jr

BASIC

de,hl

h1, (AE27)
a, (de)
(h1)
nz,D/2E
hl

de

nc, D/716
de,h1

a, (BOC1)
a

(h1)

07
nz,D72E
de,hl

de

hl

hl
D/08

DOHOMEHEODEDEDE DE DH DEEE DE DEEE DE DE DEEE DEEE DE HE DEN D

D731
D732
D733
D734
D735
D/36
D737
D/38
D739
D/3B
D73C

F5
54
SD
23
23
7E
23
17
30FB
F1
C9

push
1d
ld
inc
inc
Id
inc
rla
Jr
pop
ret

af
d,h
e, 1

hl

hi

a, (hl)
hl

nc, D736
af

DEEE DE DE DEEE DE DE DE DE NE DE DE DE ED DE D DE DEEE DE DE DEN DEEE

D73D
D73F

3E02
CD49D7

ld
call

a, 02
D749

1.0

type de variable

chercher tableau

—III 90-

D742
D743
D744
D746
D747
D748

1B
1A
F640
12
13
C9

dec
ld
or
ld
inc
ret

BASIC

de
a, (de)
40
(de),a
de

CELLES REZ LEE SEL LS LS LLLL ES LIST)

D749
D7LHA
D74B
D74C
D74D
D750
D751
D754
D755

D758
D75B
D75C
D75F
D760
D761
D763
D764
D766
D/67
D768
D76B
D76C
D76D
D76E
D76F
D770
D771
D772
D773
D774
D775

D5
ES
C5
F5
CD77D7
F5
2A87AE
EB
CDF8F5

CD3AF5
F1
CD8AD7
F1

2B
3600
3D
20FA
C1

E3
CDASD/
D1

ET

23

7B

91

77

23

7A

98

77

37

push
push
push
push
call
push
id

ex

cal]

call
pop
cal]
pop
dec
1d
dec
Jr
pop
ex
call
pop
pop
inc
ld
sub
id
inc
ld
sbc
ld
scf

de
hi
bc
af

D777

af

h1, (AE87)

de,hl
F5F8

F53A

af

D78A

af

hl
(h1),00
a
nz,D/60
pc
(sp),hl
D7A5

de

hl

hl

a,e

C
(hl),a
hl

a,d

a,b
(h1),a

1.0

mettre bit 6, ‘FN’

début des tableaux

réserver place dans zone des
variables
augmenter pointeur pour zone

-II1 91-

D776

cg

ret

BASIC 1.0

CELLLELELL LL ELLES SL LS LL LS SSL LS LS)

D777
D779
D77A
D77D
D77F
D780
D781
D782
D783
D784
D786
D787
D789

C603
UF
2A27AE
0600
oc
o4
7E
23
17
30F9
78
0600
cg

add
ld
ld
ld
inc
inc
ld
inc
rla
Jr
1d
ld
ret

a,03

c,a

h1, (AE27)
b, 00

C

b

a, (hl)

hl

nc, D77F
a,b
b,00

HOHMOUEDEDÉHEHEHE DE DEN HEDE DE DEEE DE DEDEN EDEN

D78A
D78B
D78C
D78D
D78E
D790
D791
D792
D793
D794
D797
D79A
D79D
D7SE
D79F
D7A0
D7A1
D7A2
D7A3
D7A4

62
6B

09

UF
0600
ES

D5

13

13
2A27AE
CDF2FF
3AC1B0
3D

12

13

42

uB

D1

E1

co

ld
id
add
ld
ld
push
push
inc
inc
1d
call
ld
dec
Id
inc
id
id
pop
pop
ret

h,d

l,e
h1,bc
c,a
b,00

hl

de

de

de

h1, (AE27)
FF2

a, (BOC1)
a

(de),a
de

b,d

c,e

de

hl

MED HE HD DEEE EE MEME DE DEEE DD DE HN

idir
type de variable

-111 92-

D7A5
D7A6
D7A7
D7A8
D7A9
D7AA
D7AB
D7AC
D7AD
D7AE
D7AF
D7BO
D7B1
D7B2
D7B3
D/B4

7E
12
7B
91
77
23
7E
F5
7A
98
77
F1
13
12
13
C9

1d
1d
1d
SUD
1d
inc
ld
push
ld
sbc
1d
pop
inc
id
inc
ret

BASIC

a, (hi)
(de),a
a,e

C
(hl1),a
hl

a, (h1l)
af

a,d
a,b
(h1),a
af

de
(de),a
de

HOME HOMME DEMO DEN DEEE DE EEE DEEE RE HUE

D7B5
D/B8
D7B9
D/BB
D/BD
D/BF
D7C2
D/C5
D7C6
D/C7
D/CA
D7CD
D/DO
D/D3
D7D4
D/D6
D7D9
D/DA

CDO6D9
7E
FE28
2805
EESB
C242D6
CDSAD8
E5

C5
3AC1BO
CDEADS
CDO8D7
DAUAD6
C1
3EFF
CD8AD8
ET

cg

call
ld
Cp
jr
xor
jp
call
push
push
1d
call
call
jp
pop
ld
call
pop
ret

D906

a, (hl)
28
z,D/C2
CB

nz, D642
D85A

nl

bc

a, (B0OC1)
DSEA
D/708
C,D64UA
bc

a, FF
D88A

hl

CÉLELLEST SELS STE SES ESS SSL SSSR SS

D/DB
D/DC

F5
7E

push
1d

af
a, (hl)

1.0

Dimens ionnement
aller chercher nom variable

LE

'ET'
"Syntax error’

type de variable

calculer position table pour tableau
chercher tableau

trouvé, ‘Array already dimensioned’

tester si variable dimensionnée

-IIT 93-

BASIC 1,0

D7DD FE28 cp 28 re

D/DF 2810 jr Z,D7F1

D/E1 EESB xor SB ET’

D/E3 280C jr z,D/F1

D/ES F1 pop af

D/E6 DO ret nc

D/E7 ES push hl

D/E8 2A8GSAE id h!, (AE85) début des variables
D/EB 2B dec hi

D/EC 19 add hi,de

D/ED EB ex de,h]l

D/EE Ei pop h1

D/EF 37 scf

D7FO C9 ret

CELLLLLLLLLILLLLLLLLLLLLLLISLRSEL ES) variable dimensionnée
D7F1 CDSAD8 call D85A

D7F4 F1 pop af

D/F5 ES push hl

D/7F6 3007 Jr nc, D7FF

D/F8 2AS87AE ld h1, (AE87) début des tableaux
D7FB 2B dec hl

D7FC 19 add hl,de

D7FD 1815 Jr D814

D7FF C5 push bc

D800 DS push de

D801 3AC1BO ld a, (BOC1) type de variable
D804 CDEADS call DSEA calculer position table pour tableau
D807 CDO8D/ call D708 chercher tableau
D80A 300F jr nc, D81B pas trouvé ?

D80C 13 inc de

D80D 13 inc de

D80E EI pop hl

D80F CD6DD7 call D76D

D812 C1 pop bc

D813 EB ex de,hl

D814 78 ld a,b

D815 96 sub (h1)

D816 C246D6 Jp nz, D646 ’Subscript out of range’

-111 94-

D819

D81B
D81C
D81D
D81E
D821
D824
D825
D828
D829
D82A
D82B
D82C
D82D
D82E
D82F
D83i
D834
D835
D836
D837
D838
D83B
D83E
D83F
D842
D843
D844
D845
D846
D847
D848
D849
D84B
D84C
D84F
D851
D854
D855

180A

E1

C1

AF
CD8AD8
CD6DD7
EB
110000
46

23

ES

D5

5E

23

56
3E02
CDAOFS
7E

23

66

6F
CDB8FF
D246D6
E3
CDBEBD
D1

19

EB

ET

23

23

05
20DF
ES
2AC1B0
2600
CDBEBD
D1

19

Jr

pop
POP
xor
call
call
ex
ld
ld
inc
push
push
1d
inc
1d
1d
call
1d
inc
ld
ld
call
JP
ex
call
pop
add
ex
pop
inc
inc
dec
jr
push
ld
ld
call
pop
add

BASIC 1.0

D825

hl

bc

a

D88A

D76D

de,hl

de, 0000

b,(h1) nombre de dimensions

hl

hl

de

e,(h1l)

hl limite tableau dans de
d,(h1)

a, 02

FSAO libérer place dans pile Basic
a, (hl)

hl aller chercher index dans h1
h,(h1)

1,a

FFB8 comparer hl <> de

nc, D646 Subscript out of range’
(sp),hl

BDBE multiplication entiers sans signe
de

hl,de

de,hl

hl

hi

hl

b prochain index

nz,D82A

hl

h1, (B0C1) type de variable

h,00

BDBE multiplication entiers sans signe
de

hl,de

-1IT 95-

BASIC 1.0

D856 EB ex de,hl

D857 El pop hl

D858 37 scf

D859 C9 ret

LÉRSLSISLÉESSSSSLLSRSLLLSSLSSLLSLILLSSS ST: ] ire indices

D85A DS push de

D85B CD3FDD call DD3F ignorer espaces

D85E  3AC1B0 ld a, (BOC1) type de variable

D861 F5 push af

D862 0600 id b,00

D864 CD/CCE call CE7C aller chercher valeur 16 bits 0 -
32767 , index

D867 ES push hl

D868 3E02 ld a,02

D86A CDBOFS call F5SBO réserver place dans pile Basic

D86D 73 1d (hl),e

D86E 23 inc hl index sur pile Basic

D86F 72 ld (hl),d

D870 El pop hl

D871 O4 inc D

D872 CDS5DD call DD55 virgule suit ?

D875 38ED jr c,D864 oui, index suivant

D877 7E ld a,(h1) ;

D878 FE29 CP 29 13

D87A 2805 Jr z,D881

D87C FESD CP SD EU’

D87E C242D6 Jp nz, D642 ’Syntax error’

D881 CD3FDD call DD3F ignorer espaces

D884 F1 pop af

D885 32C1B0 id (BOC1),a type de variable

D888 D1 pop de

D889 C9 ret

LÉLEELLE LD ES ESS LSLLLLLLILLILILILL.:

D88A ES push hl

D88B 3226AE ld (AE26),a

D88E C5 push bc

D88F 78 ld a, D

D890 87 add a,à

-IIT 96-

D891
D893
D896
D897
D89A
D89B
D8SE
D89F
D8A2
D8A3
D8A4
D8A5
D8A6
D8A7
D848
D8AB
D8AC
D8AE
D8AF
D8B0
D8B1
D8B2
D8B5
D8B6
D8B9
D8BB
D8BC
D8BE
D8C1
D8c2
D8C3
D8C4
D8C5
D8C6
D8C7
D8c8
D8C9
D8CA
D8CB
D8CE

C603
CD77D7
F5
2A89AE
EB
CDF8F5
F1
CD8AD7
60

69

C1

D5

23

23
3AC1B0
SF
1600
70

ES

23

D5
3A26ÂE
B7
110B00
280B
ES
3E02
CDAOF5
SE

23

56

13

E1

73

23

72

23

E3
CDBEBD
DA46D6

add
call
push
ld
ex
call
pop
call
1d
ld
Pop
push
inc
inc
1d
ld
id
ld
push
inc
push
ld
or
ld
Jr
push
ld
call
ld
inc
ld
inc
Pop
ld
inc
ld
inc
ex
call
Jp

BASIC 1,0

a,03

D777

af

hl, (AE89) fin des tableaux

de,hl

F5SF8 réserver place dans zone variables
af

D78A

h,b

1,c

bc

de

hl

hl

a, (B0OC1) type de variable

e,a

d,00

(h1),b

hl

hl

de

a, (AE26)

a

de, 000B 11, valeur défaut pour index
z,D8C6

hl

a,02

F5AO libérer place dans pile Basic
e,(hl)

hi

d,(h1l)

de

hi

(hl),e

hi

(hl),d

hl

(sp),hl

BDBE multiplication entiers sans signe
c,D646 ‘Subscript out of range”

-III 97-

BASIC 1,0

D8D1 EB ex de,hl

D8D2 E1 pop hl

D8D3 10DC djnz D8B1 prochain index
D8D5 42 id b,d

D8D6 4B ld c,e

D8D7 54 ld d,h

D8D8 SD ld e,l

D8D9 CDFBFS call FSFB réserver place en mémoire
D8DC 2289AE ld (AE89),h1 fin des tableaux
D8DF C5 push bc

D8E0 2B dec hl

D8&E1 3600 id (h1),00

D8E3 OB dec bc

D8E4 78 1d a,b

D8&S5 B1 or C

D8E6 20F8 Jr nz,D8EO

D8E8 C1 pop bc

D8&9 EI pop hl

D&A GE ld e,(hl)

D8EB 1600 1d d,00

D8ED EB ex de,hl

D&ÆE 29 add hl,hl

D&F 23 inc hl

D8FO 09 add h1,bc

D8F1 EB ex de,hl

D8F2 2B dec hl

D8F3 2B dec hl

D8F4 73 ld (hl),e

D8FS 23 inc hl

D8F6 72 ld (h1),d

D8F7 23 inc hl

D8F8 E3 ex (sp),hl

D8F9 EB ex de,hl

D8FA 3AC1B0O id a, (B0C1) type de variable
D8FD CDEADS cal} DSEA calculer position table pour tableau
D900 CDASD7 call D7AS

D903 D] pop de

D904 E1 pop hl

D905 C9 ret

-[11 98-

BASIC

RER DENE DE EME EDEN DE EDEN DEEE DEEE NE NUE

D906 CD/FD9 call D97F
D909 23 inc hl

DSCA GE ld e,(hl)
D90B 23 inc hl

D90C 56 id d,(h1)
D90D 7A ld a,d

D90E B3 or e

D9OF  280A Jr z,D91B
D911 23 inc hi

D912 7E ld a,(hl)
D913 17 rla

D914 30FB Jr nc,D911
D916 CD3FDD call DD3F
D919 37 scf

D91A C9 ret

HEHOHEHEHEDEDE M DEEE HÉE DE HE DE DEEE EDEN DEHE EDEN HE EEE HE
D91B 2B dec hl

D91C 2B dec hl

D91D EB ex de,hl
D91E C1 pop bc

D91F 2A27AE ld h1, (AE27)
D922 ES push hl

D923 212BD9 ld h1,D92B
D926 ES push hi

D927 C5 push bc

D928 EB ex de,hl
D929 180E Jr D939
D92B E5 push hi

D92C 2A27AE 1d hl, (AE27)
D92F  CDACFS cal] F5AC
D932 E1 pop hl

D933 E3 ex (sp),hl
D934 2227AE ld (AE27),hl
D937 El pop h1

D938 C9 ret

D939 ES push hl

1.0

aller chercher nom de variable
déterminer type de variable

variable déjà initialisée ?
non

ignorer lettres du nom
tester bit 7

dernière lettre ?
ignorer espaces

fixer pointeur sur type de variable

fixer pointeur de pile Basic

-III 99-

BASIC 1,0

D93A 7E 1d a, (hl)

D93B 23 inc hl

D93C 23 inc hl

D93D 23 inc h1

D93E 4E Id c,(h1)

D93F CBA9 res 5,C

D941 FEOB cp 0B

D943 3819 Jr c,D9SE

D945 79 ld a,C

D946 E61F and 1F

D948 C60B add a,0B

D94A SF Id e,a plus AEOB

D94B CEAE adc a, AE

D94D 93 Sub e

DSUE 57 ld d,a

D9uF A ld a, (de)

D950 32C1B0 ld (B0OC1),a type de variable

D953 E3 ex (sp),hl

D954 360D ld (h1),0D

D956 FEOS5 CP 05

D958 2803 Jr z, D95D

D95A C609 add a,09 05 + 09 => OD

D95C 77 1d (h1),a

D95D E3 ex (sp),hl

D9SE EB ex de,hl

D95F  3E28 ld a, 28 40

D961 CDBOFS call F5BO réserver place dans pile Basic

D964 2227AE ld (AE27),h1l

D967 0629 1d D, 29 41

D969 O5 dec b déjà 40 caractères ?

D96A CA42D6 JP z,D642 oui, ‘Syntax error’

D96D 14 Id a, (de) aller chercher prochain caractère du
nom

DIE 13 inc de

D96F EG6DF and DF convertir minuscules en majuscules

D971 77 Id (h1),a sauvegarder dans pile Basic

D972 23 inc hl

D973 17 rla dernier caractère ?

D974 30F3 jr nc, D969 non

D976 CDACFS call F5AC fixer pointeur de pile Basic

-111 100-

BASIC 1.0

D979 EB ex de,hl

D97A 2B dec hl

D97B D1 pop de

D97C C33FDD jp DD3F ignorer espaces
LÉLLELSELSELSSSISSLSSLSLLSLLILLLSSS. déterminer type de variable
D97F 7E 1d a,(h1)

D980 FEOB cp 0B

D982 3802 Jr c,D986 inférieur 0B ?

D984 C6F7 add a,F7 -9, OD => 05

D986 FEO4 Cp 04 "1", Real-Variable ?

D988 2809 jr z,D993 fixer type sur ‘real
D98A 3004 jr nc, D990 "Syntax error’

D98C FEO2 CP 02 /%', Integer-Variable ?
D98E 3005 jr nc, D995 ou ’$’, chaîne ?

D990 C342D6 jp D642 ’Syntax error’

D993 3E05 1d a,05 Real”

D995 32C1B0 ld (BOC1),a ranger type de variable
D998 cg ret

LÉLLELLEL LS SLLSS SL SSLLLSLLSSLLSL. actualiser table des tableaux
D999 CDC6DS call D5C6 vider table pour tableaux
D99C 2A89AE ld h1, (AE89) fin des tableaux

D99F EB ex de,hl

D9AO 2A87AE ld hl, (AE87) début des tableaux

DSA3 CDB8FF call FFB8& comparer hl <> de

D9A6 C8 ret zZ

D9gA7 DS push de

D9SA8 CD31D7 call D731

DSAB 7E id a, (hl)

D9AC 23 inc hl

DSAD E607 and 07

DSAF 3C inc a

D9BO ES push hi

D9B1 CDEADS call DSEA calculer position table pour tableau
D9B4 CDASD7 call D7A5

D9B7 El pop hl

D9B8 5E id e,(h1l)

D9B9 23 inc hl

-111 101-

BASIC 1.0

D9BA 56 ld d,(hl)
D9BB 23 inc hl
D9BC 19 add hl,de
D9BD Di pop de
D9BE 18E3 jr DSA3
HOME DEEE DEN DE DE DEMO DE DEEE DE inst
D9CO CD8dEg cal] E989
D9C3 CDCCD9 call D9CC SUPP
D9C6 CDSSDD call DD55 virg
D9CS 38F8 Jr c,D9C3 oui,
D9CB C9 ret
ELLE LS ELSLSE LES SLSSLLSLSSLILLLLLSLLSSS] SUPP
D9CC CDO6D9 call D906 alle
D9CF ES push hl
D9DO 3AC1B0 ld a, (BOC1) type
D9D3 CDEADS call DSEA calc
D9D6 CDO8D/ call D708 cher
D9D9 ES push hl
D9DA EB ex de,hl
D9DB 1E05 ld e,05 "Imp
DSDD  D294CA JP nc, CA94

d'er
DSŒO 5E Id e,(hl)
DST 23 inc hl adre
DSŒÆ2 56 1d d,(h1)
DSÆ3 23 inc hl
DSŒ4 19 add hl,de
DŒS EB ex de,hl
DSÆ6 2AS89AE 1d h1, (AE89) fin
DS  CDCFFF call FFCF hl
DŒC E3 ex (sp),hl
DŒD C1 pop bc
DŒE EB ex de,hl
DŒF 78 ld a,b
D9FO B1 or C
D9F1 C4F2FF call nz,FFF2 ldir
D9F4 EB ex de,hl

-111 102-

ruction Basic ERASE

rimer tableau
ule suit ?
tableau suivant

rimer tableau
r chercher nom variable
de variable

uler position table pour tableau
cher tableau

roper argument’
pas initialisé, sortir message
reur

sse du tableau dans de

des tableaux

‘= hl - de

D9F5
D9F8
D9FB
D9FC

2289AE
CD99p9
E1
C9

id
call
pop
ret

BASIC 1.0

(AE89),h1 fin des tableaux
D999 actualiser table des tableaux
hl

MEN DE DEMO EE HE HE DEEE NE GENE EE

D9FD
DAOO
DAO3
DA06

210000
222BAE
2229AE
C9

ld
1d
ld
ret

h1,0000
CAE2B),h]
CAE29),h1

LELLLL ELLES ESS SSL SL LSS ESS RES ES SE)

DAO7
DA08
DAOB
DAOC
DAOF
DA10
DA12
DA15
DA18
DA19
DATA
DA1B
DAIC
DA1D
DATE
DA1F
DA20
DA21
DA22
DA23
DA24
DA25
DA26

ES
2A2BAE
E5
2A29AE
EB
3E06
CDBOF5
2229ÂE
73

23

72

23

AF

77

23

77

23

D1

73

23

72

E1

C9

push
1d
push
ld
ex
ld
call
ld
Id
inc
ld
inc
xor
Id
inc
id
inc
pop
ld
inc
id
pop
ret

hl

hl, (AE2B)
hi

h1, (AE29)
de,hl
a,06

F5BO réserver place dans pile Basic
CAE29),h1
(hl),e

hl

(hl),d

hl

a

(hi),a

hl

(hi),a

hl

de

(hl),e

hl

(hl),d

h]

CILLLLLL ELLE ESS LS SES SL LS SELLES LE)

DA27
DA28
DA2B

E5
2A29AE
222BAE

push
1d
id

hl
h1, (AE29)
CAE2B),h1

-II1 103-

DA2E
DA2F

ET
C9

Pop
ret

BASIC 1.0

nl

EEE DE HEDEDEMEDE DE DEEE DE DE DEEE EE DEEE EME EE HO NS

DA30
DA31
DA34
DA37
DA38
DA39
DAGA
DA3B
DA3C
DA3F
DA4O
DA41
DA42
DAU3
DAU4
DA4S
DA4G
DA49
DAUA

E5
2A29AE
CDACFS
5E
23
56
23
EB
2229AE
EB
22
23
SE
23
56
EB
222BAE
E1
C9

push
1d
call
ld
inc
id
inc
ex
ld
ex
inc
inc
Id
inc
ld
ex
1d
POP
ret

nl

h1, (AE29)
F5SAC fixer pointeur de pile Basic
e,(hl)

hl

d,(hl)

hl

de,hl
(AE29),h1
de,hl

hl

hl

e,(hi)

hl

d,(hl)
de,hl
(AE2B),hl
fl

DEHHEDHEDE DEEE DE DE DD DD HE HD DEEE DÉNEÉOHEEDEHEDEDE H

DAUB
DAUC
DAUE
DA51
DA52
DAS5
DAS8
DAS9
DASA
DASD
DASE
DASF
DA62
DA65
DA68

ES
3E02
CDBOFS
E3
CD/FD9
CD39D9
E3

EB
2A29AE
23

23
010000
CDA5D7
3AC1B0
47

push
Id
call
ex
call
call
ex
ex
ld
inc
inc
1d
call
ld
1d

hi

a, 02

F5BO réserver place dans pile Basic
{sp),hl

D97F déterminer type de variable
D939

(sp),h1

de,hl

h1, (AE29)

h1

hl

bc, 0000

D7AS

a, (BOC1) type de variable

b,a

-III 104-

19
5À
6D
\6E
AGF
A70
)A71
JA72
DA73

3C
CDBOF5
78
3D
77
23
EB
ET
C9

inc
call
id
dec
1d
inc
ex
pop
ret

BASIC 1.0

a
F5BO réserver place dans pile Basic
a,D

(hl),a
nl
de,hl
hi

DDR HEEDEHEHHEHEORGEHEGEHEEHEEGEHEDE GED JEHE ÉE E

DA74
DA77
DA78
DA79
DA7B
DA7/C
DA7D
DA7E
DA7F
DA80
DA83
DA86

2A29AE
7C

B5
280E
LE

23

46

23

C5
010000
CDCEDA
EI

DAB87

HEHEHOHODEDEHHDEME MEME DE HE DD DENON EEE HE

DA89
DA8C
DA8D
DA8E
DA91
DA94
DA95
DA96
DA97
DA99
DASB
DASE
DASF
DAAO
DAAI

OTHTTA
C5

19
CDDBDS
CDCEDA
C1

OC

05
20F3
3E03
CDEADS
LE

23

46

78

1d
1d
or
jr
1d
inc
ld
inc
push
ld
call
pop

iL'8 "EE

id
push
ld
call
call
pop
inc
dec
Jr
ld
call
id
inc
1d
ld

h1, (AE29)
a,h
1
z,DA89
c,(h1)
hi
b,(hl)
hl
bc
bc, 0000
DACE
hl
jr DA 77

bc, 1A41 26 lettres, ‘A’

bc

a,C première lettre du nom
D5DB calculer position de table
DACE

bc

C lettre suivante

b déjà toutes les lettres ?
nz, DA8C

a, 03

DSEA calculer position table pour tableau
c,(h1l)

hl

b,(h1)

a,bD

-111 105-

DAA2
DAA3
DAAU
DAA7
DAA8
DAA9
DAAA
DAAB
DAAE
DAAF
DABO
DAB1
DAB2
DAB3
DAB4
DAB5
DAB6
DAB7
DAB8
DAB9
DABB
DABC
DABD
DABE
DAC
DAC3
DAC6
DAC7
DAC8
DAC9

DACB
DACC

DACE
DACF
DADO
DAD1
DAD2
DAD3

BI

C8
2A87AE
2B

09

ES

D5
CD31D7
Di

23

UE

23

46

23

ES

09

E3

LE

23
0600
09

09

C1
CDBEFF
2808
CDE7DA
25

23

23
18F3

ET
18D0

7E
23
66
6F
B4
c8

or
ret
ld
dec
add
push
push
call
pop
inc
Id
inc
ld
inc
push
add
ex
ld
inc
1d
add
add
pop
call
jr
call
inc
inc
inc
jr

pop
jr

id
inc
1d
ld
or
ret

BASIC 1.0

C

Z

hl, CAE87) début des tableaux
h1
hi,bc
hl

de

D731

de

hl
c,(h1)
hl
b,(h1)
hl

hl
hl,bc
(sp},hl
c,(hl)
hl

b,00
hi,bc
hl,bc
bc
FFBE comparer hl <> bc
z,DACB
DAE7

hi

hl

nl

DABE

hi
DASE

a, (hl)
hi

h, (h1)
},a

h

Z

-111 106-

BASIC 1.0

DAD4 09 add hl,bc

DADS ES push hl

DAD6 DS push de

DAD7 CD31D7 call D731

DADA D1 POP de

DADB 7E 1d a, (h1l)

DADC 23 inc hl

DADD E607 and 07

DADF FEO2 CP 02

DAET CCE7DA call Z,DAE7

DAEU El pop hl

DAES 18E7 ir DACE

DAE7 C5 push bc

DAE8S DS push de

DAE9 ES push hl

DAEA 7E 1d a, (h1l)

DAEB 23 inc hl

DAEC 4E ld c,(hl)

DAED 23 inc hl

DAEE 46 1d b,(hl)

DAEF EB ex de,hl

DAFO B7 or a

DAFT CUFS8FF call nz,FFF8 jp (h1l)

DAF4 E1 pop hl

DAFS Di pop de

DAF6 C1 pop bc

DAF7 C9 ret

OEM EDEN HE EEE EME EME DE DEEE ME EH EH HN instruction Basic LINE

DAF8 CD3/DD call DD37 tester si encore un caractère
DAFB A3 db A3 "INPUT"

DAFC CDCBC1 call C1CB aller chercher numéro de canal
DAFF F5 push af

DBCO CD89DB cal} DB89 sortir éventuelle chaîne dialogue
DBO3 CD86D6 call D686 chercher variable

DBO6 CD3CFF call FF3C type ‘chaîne’, sinon ‘Type mismatch'’
DBO9 ES push hl

DBOA DS push de

DBOB CD1ADB call DB1A aller chercher entrée dans appareil

-I11 107-

DBOE

DB11
DB12
DB15
DB16
DB17

CDDCF7

El
CD6FD6
ET
F1
C3AFC1

call

pop
call
POP
pop
JP

B

F7

hl
D66F
hl
af
C1AF

LELLÉELSLESSEL SELS SL SL SSL LS)

DB1A
DB1D
DB20
DB23
DB24
DB27
DB28

CDCOC1
D266DC
CDA2C1
F5

CDADDB
F1

C3A2C1

call
JP
call
push
call
pop
JP

C1CO
nc, DC66
C1A2

af

DBAD

af

C1A2

ASIC

DC

aller

LES ELELSE LEE DEL TEL SELLE ELLES TEE EX)

DB2B
DB2E
DB2F
DB32
DB33
DB36
DB37
DB39
DB3C
DB5D
DB4O
DB42
DB43
DB44

CDCBC1
F5
CD47DB
D5
CD86D6
E3
3E00
CDBCDB
E3
CDS5DD
38F1
Di

F1
C3AFC1

call
push
call
push
call
ex
ld
call
ex
call
jr
POP
pop
Jp

C1CB
af
DB47
de
D686
(sp),hl
a,00
DBBC
(sp},hl
DD55
c,DB33
de

af
C1AF

LÉRÉRLLE SSSR SSL LS ESS LE LLLESLLLL IL LL.)

DB47
DB4A
DB4C
DB4F
DB50

CDCOC1
303D
CDA2C1
F5

ES

call
jr

call
push
push

C1C0
nc, DB89
C1A2

af

hi

1.0

actif
entrer chaîne dans pile du
descripteur

affecter résultat à variable

réinitialiser numéro canal
chercher entrée dans appareil actif

aller chercher entrée de cassette

aller chercher entrée du clavier

instruction Basic INPUT
aller chercher numéro de canal

aller chercher entrée et convertir

aller chercher variable

tester si virgule

aller chercher entrée et convertir

sortir éventuelle chaîne dialogue

-I11 108-

DB51
DB54
DB56
DB59
DB5B
DBSE
DB5F
DB62
DB63
DB64
DB67
DB69
DB6C
DB6F
DB70

DB72
DB73
DB74

CD89DB
3E3F
D456C3
3E20
D456C3
E5
CDADDB
EB

ET
CDD3DB
3809
2177DB
CD41C3
ET
18DE

F1
Fi
C3A2C1

call
ld
call
ld
call
push
call
ex
pop
call
jr
id
call
pop
jr

pop
pop
JP

BASIC 1.0

DB89 sortir éventuelle chaîne dialogue
a, 3F ri

nc, C356

a, 20 FD?

nc, C356

nl

DBAD aller chercher entrée du clavier
de,hl

hl

DBD3

c,DB72

h1,DB77 ’?Redo from start’

C341 sortir

hl

DB50

af
af
C1A2

HN DE HE HDMI HE DE HN HE D DH DEHE DE GE DEMO HE DE EH

DB77 3F 52 65 64 6F 20 66 72

’?Redo from start’

DB/F 6F 6D 20 73 74 61 72 74

DB87 OA 00

CÉELLLLÉELLLSLSLSLSLLLSLSSSSÉESSSSSS] sortir éventuelle chaîne dialogue
DB89 7E 1d a, (hl)

DB8A FE3B Cp 3B 1e

DB8C 322DAE ld (AE2D),a ranger signe séparation

DB8F CC3FDD call Z, DD3F ignorer espaces

DB92 EE22 xor 22 FRS

DBS4 C0 ret nz

DB95 CDCBF7 call F7CB lire chaîne dialogue

DB98 CDCOC1 call C1C0

DB9B F5 push af

DB9C DC28F8 call c,F828 sortir chaîne

DB9F F1 pop af

DBAO D4DAFB call nc, FBDA aller chercher paramètres chaîne
DBA3 CDS5DD call DD55 virgule suit ?

DBA6 D8 ret C oui

-I11 109-

BASIC 1.0

DBA7 CD37DD call DD37 tester si encore un caractère

DBAA 3B db 3B ts

DBAB B7 or a

DBAC Cg ret

LÉLSES LS SSI SSISLRSLSSLSLLLSLLSSLLLSSS: aller chercher entrée du clavier

DBAD CD3BCA call CA3B aller chercher ligne d'entrée

DBBO D26BCB Jp nc, CB6B ESC enfoncée ?

DBB3 3A2DAE Id a, (AE2D) signe de séparation

DBB6 FE3B cp 3B DrEU

DBB8 C44EC3 call nz,C34E pas ‘;’, nouvelle ligne (sortir LF)

DBBB C9 ret

AND DEN DE D D DE DE DE DE DE DE DE D DE DE DEEE DE D

DBBC DS push de

DBBD CDO2DC call DCO2

DBCO 300€ jr nc, DBCE

DBC2 E3 ex (sp),hl

DBC3 CD66D6 call D666 affecter valeur à variable

DBC6 E1 pop hl

DBC7 7E Id a,(hl)

DBC8 23 inc hl

DBC9 B7 or a

DBCA C8 ret Z

DBCB EE2C Xor 2C A

DBCD C9 ret

DBCE 1EO0D Id e,OD "Type mismatch'

DBDO C394CA Jp CA94 sortir message d'erreur

LEEDS LLEILSILSRLLLÉELLLLLSLLLLLL SI LLLSS,;

DBD3 DS push de

DBD4 ES push hl

DBDS D5 push de

DBD6  CDD6D6 cal] D6D6 aller chercher nom et type de
variable

DBD9 E3 ex (sp),hl

DBDA AF xor a

DBDB CDO2DC call DCO2

DBDE 301E Jr nc, DBFE

-111 110-

DBEO
DBE?

DBES
DBEG6
DBES
DBEA
DBEC
DBEF
DBF1
DBF3
DBF4
DBFS

DBF7
DBFA
DBFB
DBFD
DBFE
DBFF
DCO0
DCO1

DCO2
DCO3
DCO6
DCO7
DCO8
DCOA
DCOD
DCOE

DC10
DC13
DC16
DC19
DC1A
DC1D
DCIE
DC1F

FE03
CCDAFB

E3
CDS5DD
E3
300B
CD61DD
EE2C
200B
23

E3
18DF

CD61DD
B7
2001
37

ET

ET

D1

C9

5F
CD4SFF
57

D5
2006
CD21DC
37
1809

CDCOC1
D438DC
CDASEC
F5
DC61DD
F1
D1
7A

CP
call

ex
call
ex
jr
call
xor
jr
inc
ex
jr

call
or
jr
scf
pop
pop
pop
ret

ld
call
1d
push
jr
call
scf
Jr

call
call
call
push
cal]
POP
pop
ld

BASIC 1.0

03 chaine’
Z,FBDA oui, aller chercher paramètre de
chaîne
(sp),hl
DD55 virgule suit ?
(sp),h]l
nc, DBF7 non
DD61 ignorer espace, TAB et LF
2C tu
nz, DBFE
h]l
(sp),hl
DBD6

DD61 ignorer espace, TAB et LF
a
nz, DBFE

hl
hl
de

e,a

FF45 tester si chaîne
d,a

de

nz,DC10

DC21

DC19

C1C0

nc, DC38

ECA3

af

c,DD61 ignorer espace, TAB et LF
af

de

a,d

-111 111-

DC20

DC21
DC24
DC26
DC29

DC2C
DC2F
DC31
DC34
DC35

DC38
DC3B
DC3D
DC40

DC42
DCHH

DC47
DC4A
DC4C
DC4E
DC50
DC53

DC55
DC58
DC5SB
DC5D
DC60
DC62

DC63
DC65

DC66

C9

CDCOC1
3806
CD47DC
C3DCF7

CD61DD
FE22
CACBF7
7B
C3E6F7

CD9DDC
3005
11C6DC
182C

1E18
C394CA

CD9DDC
30F6
FE22
2805
11CADC
1819

CDA8DC
1163DC
3811
21AUAC
3600
C9

FE22
C9

CDA8DC

ret

call

jr

call
jp

call
Cp
Jp

JP

call
jr
ld
Jr

ld
JP

call
Jr
CP
Jr
Id
Jr

call
id
Jr
ld
ld
ret

CP
ret

call

BASIC 1.0

C1C0
c,DC2C
DC47
F7DC entrer chaîne dans descripteur de
chaîne

DD61 ignorer espace, TAB et LF
22 HUE

z,F7CB lire chaîne

a,e

F7E6

DC9D

nc, DC42 'EOF met’
de, DCC6

DC6E

e,18 'EOF met’
CA94 sortir message d'erreur

DC9D

nc, DC42 EOF met”
22 ut
Z,DC55

de, DCCA

DC6E

DCA8
de, DC63
c, DC6E

h1,ACA4 début du buffer d'entrée
(h1),00 premier caractère égale 00

22 sut

DCA8

-111 112-

DC69
DC6B
DC6E
DC71
DC72
DC74
DC77
DC79
DC7A
DC7B
DC7C
DCYE
DC81
DC83
DC85
DC87
DC88
DC89
DC8B
DC8C
DC8E
DC91
DC92
DC95
DC96
DC9g
DC9C

DC9D
DCAO
DCAI
DCAL
DCA6
DCA7

DCA8
DCAB
DCAC
DCAD
DCAF

30D7
41CDDC
21A4AC
E5
O6FF
CDFBFF
280C
77

23

05
2805
CDA8DC
38F1
F6FF
3600
ET

CO
FEOD
C8
FE22
C4DODC
CO
CD9DDC
DO
CDCADC
CH14C4
C9

CDA8DC
DO
CDDODC
28F7
37

cg

CD24C4
DO

C5
FEOD
O60A

Jr
ld
1d
push
1d
call
Jr
1d
inc
dec
jr
call
jr
or
id
pop
ret
CP
ret
cp
call
ret
call
ret
cal]
call
ret

call
ret
call
Jr
scf
ret

call
ret
push
cp
1d

BASIC 1.0

nc, DC42 "EOF met‘
de,DCCD

h1,ACA4 début du buffer d'entrée
hl

D,FF

FFFB jp (de)
z,DC85

(h1),a

hl

z,DC83

DCA8

c,DC74

FF

(h1),00

nl

nz

OD CR

Z

22 ER,

nz, DCDO

nz

DC9D

nc

DCCA

nz,C414 CAS RETURN

DCA8
nc
DCDO
z, DC9D

C424 lire et entrer un caractère
nc

bc

0D CR

b, OA LF

-I11 113-

BASIC 1.0

DCB1 2805 ir z, DCB8

DCB3 B8 cp e)

DCB4 200D Jr nz, DCC3

DCB6 O60D ld b,0D CR

DCB8 4F ld c,a

DCB9 CD24C4 call cu24 lire et entrer un caractère
DCBC 3004 Jr nc, DCC2

DCBE B8 CP D

DCBF C414C4 call nz,C414 CAS RETURN

DCC2 79 ld a,C

DCC3 C1 pop bc

DCCH 37 scf

DCC5 C9 ret

DCC6 CDDODC call DCDO

DCC9 C8 ret Z

DCCA FE2C cp 2C vr

DCCC C8 ret Z

DCCD FEOD cp OD CR

DCCF C9 ret

DCDO FE20 cp 20 D:

DCD2 C8 ret Z

DCD3 FEO9 CP 09 TAB

DCDS C8 ret Z

DCD6 FEOA cp OA LF

DCD8 C9 ret

ORDONNÉ D DEEE DE DE DE DE DE DE HE DE D EE EH HE HE instruction Basic RESTORE
DCD9 2804 jr Z,DCES

DCDB CDE1CE call CEE1 aller chercher numéro ligne dans de
DCDE ES push hl

DCDF CDS9AE7 call E79A BASIC-Zeile de suchen
DCE2 2B dec hl

DCE3 182D ir DD12 fixer pointeur de DATA
DCES ES push hi

DCEG 2A81AE ld hl,CAE81) début de programme
DCE9 1827 jr DD12 comme pointeur de DATA

=HIT 114-

BASIC
CELLES LS SISLLS LS LLSLSSESLISLSLLLSLSLSS:
DCEB ES push hl
DCEC 2A30AE ld h1, (AE30)
DCEF CD17DD call DD17
DCF2 E3 ex (sp},hl
DCF3 CD86D6 call D686
DCF6 E3 ex (sp),hl
DCF7 23 inc hi
DCF8 3E3A ld a, 3A
DCFA CDBCDB call DBBC
DCFD 2B dec hi
DCFE  280B jr z,DDOB
DDOO 2A2EAE ld hl, (AE2E)
DDO3 CDCEDD call DDCE
DD06 1E02 1d e,02
DDO8 C394CA Jp CA94
DDOB E3 ex (sp),hi
DDOC CDS55DD call DD55
DDOF E3 ex (sp),hl
DD10 38DD jr c,DCEF
DD12 2230AE ld (CAE30),h1
DD15 El pop hl
DD16 C9 ret
DD17 7E ld a, (h1)
DD18 FE2C CP 2C

1.0

instruction Basic READ

pointeur de DATA
aller chercher prochain élément DATA

chercher variable

1,1
‘

adresse ligne pendant instruction
READ
fixer adresse ligne actuelle
"Syntax error’
sortir message d'erreur

virgule suit ?

oui
pointeur DATA

-111 115-

DD1A
DD1B
DDIE
DD1F
DD21
DD22
DD23
DD24
DD25
DD26
DD28

DD2B

DD2E
DD2F
DD32
DD34
DD36

C8
CDEFE8
B7
200E
25

JE

23

B6

25
1E04
CA94CA

222EAE

23
CD3FDD
FE8C
20E5
C9

ret
call
or
Jr
inc
ld
inc
or
inc
ld
Jp

ld

inc
call
CP
jr
ret

BASIC

Z

E8EF

a

nz, DD2F
hl
a,(hl)
hl
(h1)
hi
e,04
z, CA94

CAE2E),h1

hi

DD3F

8C

nz, DD1B

LÉLLDES LS SSL EEE S SEL ES SSL ESS SL LL EE)

DD37
DD38
DD39
DD3A
DD3B

DD3C

E3
7E
23
E3
BE

C2C6DD

ex
1d
inc
ex

Cp

Jp

(sp),hl

a, (h1)

hl

(sp},hl
(h1)

nz,DDC6

ECHEC DE DE EME MEME DE DE OH DEHHEDE DEEE JE DENON NE

DD3F
DD4O
DD41
DD43
DD45
DD47
DD48
DD49

LÉLÉLLLL SELLES SSSL LES LL LL LS)

25
TE
FE20
28FA
FEO1
DO
B7
C9

inc
Id
CP
jr
cp
ret
or
ret

pl
a, (h1)
20
Z,DD3F
01
nc
a

1.0

ignorer reste de la ligne
fin de ligne ?
non

longueur de ligne
zéro, fin de programme ?

DATA exhausted'
sortir message d'erreur

adresse de ligne pendant instruction
READ

ignorer espaces
DATA’

tester si encore un caractère
pointeur sur après instruction CALL
aller chercher caractère suivant

augmenter adresse de retour
comparer caractère avec texte
programme
différent, ‘Syntax error’
ignorer espaces

Ep
ignorer espaces

fin de ligne, Z=1

fin de l'instruction, sinon ‘Syntax error’

-111 115-

DD4A
DD4B
DD4D
DD4HE

7E
FE02
D8
C3C6DD

1d

cp

ret
"Jp

BASIC

a, (nl)
02

€
DDC6

RHONE DEEE DE GENE HE DE DE DEEE DE ÉD JM H HE HEEHN NE

DD51
DD52
DD54

HER HEMOMEDEOHOHEEHEHEHEHEONE JE HE DE DE DE NE HE DE DE DE DE

DD55
DD56
DD59
DD5B
DD5C
DD5F
DD60

7E
FEO2
cg

2B
CD3FDD
EE2C
Co
CD3FDD
37

cg

1d
CP
ret

dec
call
xor
ret
call
scf
ret

a, (hl)
02

hi
DD3F
2C
nz
DD3F

LRRELLLSIELTS SELS LL LL LS LL LS, LL)

DD61
DD62
DD63
DD65
DD67
DD69
DD6B
DD6D
DD6F
DD70

7E
23
FE20
28FA
FEO9
28F6
FEOA
28F2
2B
C9

ld
inc
CP
jr
cp
jr
cp
jr
dec
ret

a, (hl)
hl
20
z, DD61
09
Z, DD61
OA
z,DD61
hl

HOMME DE DE HE MEME DEEE DE MED MED DE DEEE JE DEN EH

DD71
DD/4
DD75
DD78
DD/B
DD7C
DD7F
DD82

2A3UAE
EB

2A8BBO
2232AE
EB

2234AE
CD21B9
DCO7C8

1d
ex
1d
Id
ex
1d
call
call

h1, (AE34)
de,hl

hl1, (BO8B)
CAE32),h]
de,h1
(AE34),h1
B921
c,C807

1.0

caractère actuel
inférieur 2 ?

ok

sinon ‘’Syntax error’

tester si fin d'instruction
caractère actuel
inférieur 2 ?

tester si prochain caractère = virgule

ignorer espaces
L L

pas trouvé, c=0
ignorer espaces
trouvé, c=1

ignorer espace, TAB et LF

15!
TAB

LE

boucle de l'interpréteur

adresse de l'instruction actuelle
ranger pointeur de programme
pointeur de pile Basic

mémoire pour pointeur de pile Basic
pointeur de programme

comme adresse de l'instruction act.
KL POLL SYNCHRONOUS

traitement Event AFTER/EVERY

-111 116-

DD85
DD88
DD8B
DD8C
DD8E
DD90
DD92
DD93
DD94
DD95
DDS6
DD97
DD99
DD9C
DDSD
DDAO
DDAI
DDA3
DDAG6

DDA8

CD3FDD
CHABDD
7E
FEO
28E4
3034
23

7E

25

B6

23
280F
2236AE
23
3A38AE
B7
28D1
CDEBDD
18CC

C376CB

call
call
ld
cp
jr
jr
inc
ld
inc
or
inc
jr
ld
inc
id
or
jr
cal]
jr

JP

BASIC

DD3F
nz,DDAB
a, (h])
01
z,DD74
nc, DDC6
pl
a,(h})
hl

(h1)

hl

z, DDA8
(AE36),h1
hl

a, (AE38)
a
z,DD74
DDEB
DD/4

CB76

LR RSSS LS S SELS S SSL LES LS ESSSLLLEE,)

DDAB
DDAC
DDAF
DDB1
DDB3
DDB4
DDB6
DDB7
DDB9
DDBA
DDBB
DDBC
DDBD
DDBE
DDBF
DDCO

87
D24FD6
FEB9
3010
EB
C601
6F
CEDE
95

67

LE

23

46

C5

EB
C33FDD

add
jp
CP
Jr
ex
add
ld
adc
SUD
ld
ld
inc
ld
push
ex
Jp

a,a
nc, D64F
B9
nc,DDC3
de,hl
a,01
l,a

a, DE

1

ha
c,(h1)
hl
b,(h1)
bc
de,hl
DD3F

1.0

ignorer espaces

exécuter instruction Basic
lire texte programme

:!, fin de l'instruction ?
oui

’Syntax error’

longueur de ligne
égale zéro ?

oui, à l'instruction END
ranger adresse de ligne actuelle

flag TRAC mis ?

non

routine TRACE

à la boucle de l'interpréteur
à l'instruction END

exécuter instruction Basic
token par 2

tester si extension d'instruction

token non valable, ‘’Syntax error’

plus DEO1 (adresse de table)

adresse de l'instruction sur pile

ignorer espaces, saut à instruction

-111 117-

BASIC

CLESLES ELLES ES ETES SSI SL SL LSLLLL.;

DDC3 CDO7AC call ACO7
DDC6 1E02 1d e,02
DDC8 C394CA JP CA94

HORDE NMDE HE EH HE DE DE DE DE EN HE HE EE HE EE ENE

DDCB 210000 id h1,0000
DDCE 22364AE ld (AE36),h1
DDD1 C9 ret
LÉLSLSLSSLSLSLLLSLSSELSSLLLSLLSSSLARSS SES:
DDD2 2A36AE Id hi, (AE36)

DDD5 C9 ret

LÉSLSSSESRSS SES SES SES S TS)

DDD6 2A364AE 1d h1, (AE36)
DDD9 7C ld a,h

DDDA B5 or 1

DDDB C8 ret Z

DDDC 7E ld a, (hl)
DDDD 23 inc hl

DDDE 66 ld h, (h1)
DDDF 6F 1d 1,a

DDEO 37 scf

DDET C9 ret

DDE2 3EFF jd a,FF
DDEH 1801 Jr DDE7
LÉLLE SSL ILLLLLLLLLLLLLLELLLLLLL EL)
DDE6 AF xor a

DDE7 3238AE 1d (AE38),a
DDEA C9 ret
CLLLLLLLSLLLLLELL LL EL LLLLL LL SLE XL)
DDEB 3E5B id a, 5B
DDED CD56C3 call C356
DDFO E5 push hl

DDF1 2A36AE ld h1, (AE36)

1.0

ret

’Syntax error’

sortir message d'erreur

adresse de ligne actuelle sur zéro
adresse de ligne actuelle

charger adresse de ligne actuelle
adresse de ligne actuelle

aller chercher test mode direct / numéro de ligne

adresse de ligne actuelle

zéro, mode direct

numéro de ligne dans h]

instruction Basic TRON

instruction Basic TROFF
flag TRACE

Routine TRACE

‘ET!

sortir

adresse de ligne actuelle

-[11 118-

BASIC 1.0

DDF4 7E id a,(h1)

DDFS 23 inc hl numéro de ligne dans hl
DDF6 66 Id h, (h1)

DDF7 6F 1d 1,a

DDF8 CD/%ÆE call EE79 sortir numéro de ligne
DDFB El pop hl

DDFC 3ESD ld a, 5D "EU

DDFE C356C3 jp C356 sortir

REMOHOHEHDEHHEDE DE DE ED HE DE DE DE DE DE DE DE HE DE NEED DE DE DEEE adresses des instructions Basic

DEO1T 7109 dw C971 80 AFTER
DEO3 DFCO dw CODF 81 AUTO
DEOS 2102 dw C221 82 BORDER
DEO7 BAF1 dw FIBA 83  CALL
DEO9  46D2 dw D246 84 CAT
DEOB 3CEA dw EASC 85  CHAIN
DEOD 3201 dw C132 86  CLEAR
DEOF B5C4 dw C4B5 87 CLG
DE11 98D2 dw D298 88 CLOSEIN
DE13 A1D2 dw D2A1 89  CLOSEOUT
DE15 SAC2 dw C25A 8A CLS
DE17 COCB dw CBCO 8B  CONT
DE19 EFE8 dw E8EF 8C DATA
DE1B 17D1 on D117 8D DEF
DE1D 18D6 ow D618 8E  DEFINT
DEF  1CD6 dw D61C 8F  DEFREAL
DE21 14D6 dw D614 90  DEFSTR
DE23 E/D4 dW D4UE7 91  DEG
DE25 28E7 dw E728 92  DELETE
DE27 7DD6 dw D6/D 93 DIM
DE29 C6C4 dw C4C6 94 DRAW
DE2B CBC4 dw C4CB 95  DRAWR
DE2D 52C0 dw CO52 96 EDIT
DE2F F3E8 dw E8F3 97  ELSE
DE31 65CB dw CB65 98 END
DE33 85D3 dw D385 99 ENT
DE35 4ED3 du D34E SA ENV
DE37 COD9 dw D9C0 9B ERASE
DE39 8FCA dw CASF 9C  ERROR

DE3B 79C9 dw cg79 9D EVERY

-I11 119-

DE3D
DE3F
DE4
DE43
DE4S
DE47
DE49
DE4B
DE4D
DEUF
DE51
DE53
DE55
DE57
DE59
DESB
DESD
DESF
DE61
DE63
DE65
DE67
DE69
DE6B
DE6D
DE6F
DE71
DE73
DE75
DE77
DE79
DE7B
DE7D
DE7F
DE81
DE83
DE85
DE87
DE89
DE8B

29C5
EDC6
E8C6
C7C6
2AC2
2BDB
39D4
S4D6
F8DA
F7E0
F6E9
D2C2
EFF4
AGEA
93F9
4FC2
O5C5
OACS
FBC5
2BC1
E3C7
CBC8
F8CB
4OC9
5FD2
56D2
8cc4
77F1
OAC2
1202
DOC4
D5C4
5FF1
FDF1
F3E8
EBD4
59D5
EBDC
1ED3
F3E8

dw
du
dw
dw
dw
dw
dw
du
dw
du
dw
dW
dw
dw
dw
dw
dn
dw
dw
dw
dw
dw
dw
du
dw
dw
dw
dw
dW
dn
dw
dw
dw
du
dW
dn
dn
du
dw
du

C529
C6ED
C6E8
C6C7
C22A
DB2B
D439
D654
DAF8
EOF7
E9F6
C2D2
FUEF
EAAG
F993
C24F
C505
C50A
C5FB
C12B
C7E3
C8CB
CBF8
C940
D25F
D256
c48c
F177
C20A
C212
C4DO
C4Ds
F15F
F1FD
E8F3
DUEB
D559
DCEB
D31E
E8F3

BASIC 1.0

SE FOR

9F  GOSUB
AO  GOTO
AT IF

A2 INK

A3 INPUT
A4 KEY

AS LET

A6 LINE
A7 LIST
A8 LOAD
A9  LOCATE
AA MEMORY
AB MERGE
AC  MID$
AD MODE
AE MOVE
AF  MOVER
BO  NEXT
B1 NEW

B2 ON

B3 ON BREAK

B4 ON ERROR GOTO O
B5 ON SQ

B6  OPENIN
B7  OPENOUT
B8 ORIGIN
B9 OUT

BA PAPER
BB PEN

BC  PLOT

BD  PLOTR
BE  POKE

BF PRINT
CO ‘

C1 RAD

C2 RANDOMIZE
C3  READ

C4  RELEASE
C5 REM

-111 120-

DE8D
DE8F
DE91
DE93
DE95
DE97
DE99
DE9B
DE9D
DE9F
DEA1
DEA3
DEAS
DEA7
DEA9
DEAB
DEAD
DEAF
DEB1
DEB3
DEBS
DEB7
DEB9

DFE7
D9DC
D3CC
OFC7
BDE9
OSŒC
COD2
SuD4
SACB
SDF6
19C3
20C3
EG6DD
E2DD
7DF1
76C7
47C7
E3C3
E1C2
7BF4
F6F1
E1C8
E7C8

dw
dW
dn
dw
dn
dn
dw
dw
dn
dw
dw
dw
dw
dn
dw
dW
dw
dw
dw
dw
dn
dw
dw

BASIC

E7DF
DCD9
CCo3
C70F
E9BD
ECO9
D2C0
D4gu
CB5SA
F69D
C319
C320
DDE6
DDE2
F17D
C776
C747
C3E3
C2E1
F47B
F1F6
C8E1
C8E7

DD DD HE DE DEDEDE DH DE DE DE DEDEDEHE DE HE HD DE D DE DE DE HE HE

DEBB
DEBC
DEBD
DECO
DEC
DEC2
DEC3
DEC6
DEC9

DECC
DECD
DECE
DEDO
DED2

D5
EB
2A7FAE
EB
D5
AF
3239AE
012001
CDE1DE

7E
B7
20F9
3E2D
g1

push
ex
id
ex
push
xor
ld
ld
call

Id
or
jr
ld
sub

de

de,hl

hl, CAE7F)
de,hi

de

a
(AE39),a
bc,012C
DÉE1

a,(hl)
a

nz, DEC9
a, 2D

re

C6
C7
C8
C9
CA
CB
CC
CD
CE
CF
DO
D1
D2
D3
Du
D5
D6
D7
D8
D9
DA
DB
DC

RENUM
RESTORE
RESUME
RETURN
RUN
SAVE
SOUND
SPEED
STOP
SYMBOL
TAG
TAG OFF
TRON
TROFF
WAIT
WEND
WHILE
WIDTH
WINDOW
ZONE
WRITE
DI

EI

début de la Ram libre

max, 300 caractères
aller chercher caractère dans buffer
d'entrée

dernier caractère ?
non

301 - état compteur

-F11 121-

BASIC 1.0

DED3 4F ld C,a

DED4 3EO1 id a,01 égale longueur de ligne
DED6 98 sbc a,D

DED7 47 id b,a dans b

DED8 AF xor a

DED9 12 ld (de),a

DEDA 13 inc de trois fois zéro pour terminer
DEDB 12 ld (de),a

DEDC 13 inc de

DEDD 12 1d (de),a

DEDE El pop hl

DEDF D1 POP de

DEFG C9 ret

PCR EEHEE Q]]ler chercher caractère dans buffer d'entrée

DEET CDIOAC cal] AC10 ret

DEE4 7E ld a,(hl)

DEES B7 or a dernier caractère ?
DEE6 C8 ret zZ oui

DEE7 CD/1FF call FF71 lettre ?

DEEA 381D ir c,DFO9 oui

DEEC CD/FFF call FF7F numerique ?

DEEF  DAFFDF jp c,DFFF oui

DEF2 FE26 Cp 26 '&' ?

DEFH  CASAEO JP Z, EO5A oui

DEF7 23 inc hl

DEF8 FE80 CP 80 token ?

DEFA DO ret nc oui

DEFB FE20 CP 20 EX

DEFD C280E0 jp nz, E080

DFOO  3AOOAC ld a, (ACO0) ignorer espaces supplémentaires ?
DFO3 B7 or a

DFOH CO ret nz oui

DFO5 3E20 Id a, 20 F5

DFO7 181C jr DF25 écrire dans buffer
DFO9 CD4EDF call DFE

DFOC D8 ret C

DFOD FECS5 cp C5 "REM’

DFOF  CAEDEO Jp Z,E0ED

-III 122-

BASIC 1.0

DF12 ES push hl

DF13 2130DF 1d h1,DF30 adresse de base de la table

DF16 CDAAFF call FFAA parcourir table

DF19 E1 pop hl

DFIA 3819 Jr c,DF35 trouvé ? alors ne pas convertir le
reste

DF1C F5 push af

DF1D FE97 CP 97 "ELSE"

DF1F 3E01 id a,01

DF21 CC25DF call Z,DF25 écrire dans le buffer

DF24 F1 pop af

DF25 12 ld (de),a écrire un caractère dans le buffer

DF26 13 inc de augmenter le pointeur de buffer

DF27 OB dec bc diminuer le compteur

DF28 79 ld a,C

DF29 BO or b

DF2A CO ret nz

DF2B 1E17 ld e,17 ‘Line too long’

DF2D C394CA JP CA94 sortir message d'erreur

LÉLRRLLLELLL SES LS LS SSL LL LISE LL LLLLL) tokens spéciaux

DF30 8C db 8C "DATA"

DF31 8E db 8E "DEFINT'

DF32 90 db 90 *DEFSTR'

DF33 8F db 8F "DEFREAL"

DF34 O0 db 00 fin de table

LÉELRLLLELLSLSSILSLSRSLSSLLSSLLSSLSLEES ES)

DF35 CD25DF call DF25 écrire dans le buffer

DF38 7E ld a, (hl)

DF39 B7 or a

DF3A C8 ret Z

DF3B FE3A Cp 3A rue

DF3D 280A jr z, DF49

DF3F 23 inc h1

DF4O FE22 CP 22 te

DF42  20F1 Jr nz,DF35

DF4H  CDBFEO call EOBF
DF47 18EF Jr DF38

-111 123-

DF49
DFHA
DFD

DFHE
DF4F
DF50
DF51
DF54
DF55
DF56
DF59

DF5C
DF5F
DF61
DF62
DF64
DF67
DF69
DF6A
DF6C
DF6E
DF6F
DF72
DF74
DF75
DF76
DF77
DF7A
DF7B
DF7C
DF7D
DF7F
DF82
DF83
DF86
DF87

DF89

AF
3239AE
C9

C5
D5
ES
CD16AC
7E
23
CD8AFF
CDDDE2

CD27E3
3028
79
E67F
CD7BFF
300B
TA
FEE4
2806
JE
CD/BFF
3815
F1

TA

B7
FAC8DF
D1

C1

F5
3EFF
CD25DF
F1
CD25DF
ÂF
183A

ET

Xor
Id
ret

push
push
push
cal]

id

inc
call

call

call
jr
id
and
call
jr

CP
jr
ld
cal]
jr
pop
id
or
jp
pop
pop
push
1d
call
pop
call
xor
jr

POP

BASIC 1.0

a
(AE39),a

bc

de

hl

AC16 ret

a, (hl)

hl

FF8A convertir minuscules en majuscules
E2DD calculer adresse des mots

instruction

E327

nc, DF89

a,C

7F

FF7B tester si lettre ou chiffre

nc,DF74

a, (de)

E4 FN’

z,DF74

a,(hl)

FF7B tester si lettre ou chiffre

c,DF89

af

a, (de)

a

m, DFC8

de

bc

af

a,FF ‘fonction’

DF25 écrire dans buffer

af

DF25 écrire dans buffer

a

DFC3

nl

-III 124-

BASIC 1,0

DF8A D1 pop de

DF8B C1 pop je

DF8C ES push hl

DF8D 2B dec hi

DF8E 23 inc hi

DF8F 7E ld a,(hl)

DF90 CD7BFF call FF7B tester si lettre ou chiffre
DF93 38F9 jr c,DF8E

DF95 CDEADF call DFEA

DF98 3804 jr c, DFE

DFSA 3EOD ld a, OD token pour variable
DF9C 1806 jr DFA4

DFIŒ 23 inc hl

DF9F FEOS cp 05

DFA1 2001 jr nz, DFA4

DFA3 3D dec a

DFA4  CD25DF call DF25 écrire dans buffer
DFA7 AF xor a zéro

DFA8 CD25DF cal] DF25 écrire dans buffer
DFAB AF xor a

DFAC CD25DF call DF25 écrire dans buffer
DFAF E3 ex (sp),hl

DFBO 7E ld a, (hl)

DFB1 CD/BFF call FF7B tester si lettre ou chiffre
DFB4 3007 jr nc, DFBD

DFB6 7E ld a,(hl)

DFB7 CD25DF call DF25 écrire dans buffer
DFBA 23 inc hl

DFBB 18F3 Jr DFBO

DFBD CDDFEO call EODF

DFCO E1 pop hi

DFC1 3EFF ld a,FF
DFC3 3239AE ld (AE39),a
DFC6 37 scf

DFC7 C9 ret

DFC8 ES push hl

DFC9 4F ld c,a

-111 125-

BASIC 1,0

DFCA 21DCDF ld hl,DFDC adresse de base de la table
DFCD CDAAFF call FFAA parcourir la table
DFDO 9F sbc a,a

DFD1 E601 and 01

DFD3 323SAE id (AE39),a

DFD6 79 ld a,C

DFD7 Ei pop hl

DFD8 D1 pop de

DFD9 C1 pop bc

DFDA B7 or a

DFDB C9 ret

LÉELLLLÉELSLSLSSSSLLLSLSLSSLSLLLLLL SES: instructions avec numéro de
DFC C7 db C7 "RESTORE"

DFDD 81 db 81 "AUTO"

DFDE C6 db C6 ! RENUM'

DFDF 92 db 92 !DELETE”’

DFEO 96 db 96 EDIT’

DFE C8 db C8 "RESUME

DFE2 E3 db E3 "ERL'’

DFE3 97 db 97 "ELSE”

DFE4 CA db CA RUN"

DFES A7 db A7 LIST’

DFE6 AO db AO GOTO'

DFE7 EB db EB ! THEN‘

DFE8 9F db 9F ! GOSUB'

DFE9 00 db 00 fin de la table
LELLLLLLLLL LIL LLS LI SLESSLLLILLLLLEL EL:

DFEA FE26 cp 26 '&'

DFEC DO ret nc

DFED FE21 CP 21 "1"

DFFO DO ret nc

DFF1 FE22 CP 22 Lu

DFF3 C8 ret Z

DFF4 FE23 cp 23 "#4"

DFF6 C8 ret Z

DFF7 EE27 xor 27 pire

DFFQ FEO4 cp 04

DFFB CEFF adc a,FF

-[11 126-

ligne

DFFD
DFFE

E000
E001
E002
E003
E005
E006
E007
EOOA
EOOC
EOOF
E010
EO11
EO14
E016
E018

EOTA
E01B
E01C
EO1F
E020
E022
E025
E027
E029
EO2A
E02D
EO02E
E02F
E030
E032
EO54
E035
E036
E037
E039
E03B

37
cg

39

AE

B7
2815
7E

23
FA25DF
FE2E
CA25DF
2B

D5
CDOUEE
3034
3E1E
184F

D5

C5
CDBEEC
C1
3028
CD27FF
3E1F
3040
EB
2AC2B0
EB

7A

B7
3E1À
2035
E3

EB

7D
FEOA
3004
C60E

scf
ret

add
xor
or
jr
ld
inc
jp
cp
JP
dec
push
call
Jr
ld
Jr

push
push
call
pop
jr
call
ld
Jr
ex
1d
ex
ld
or
ld
Jr
ex
ex
ld
Cp
Jr
add

BASIC 1,0

hl,sp

(h1)

a

Z,E01A

a,(hl)

hl

m, DF25 écrire dans buffer
2E tt

z,DF25 écrire dans buffer
hl

de

EEO4

nc, EO4A

a,1E token pour numéro de ligne
E069

de

bc

ECBE

bc

nc,EO4A

FF27 tester si chaîne
a,1F token pour virgule flottante
nc, E069

de,hl

h1, (BOC2)

de,hl

a,d

a

a,1A token pour nombre deux octets
nz,E069

(sp),hl

de,hl

a, 1

OA 10

nc, E03F

a, 0E additionner offset

-111 127-

EO3D

E03F
EOu1
EO4H
EO4S
EO48
E049

EO4A
EO4B
EO4C
EO4D
EOUE
E051
E052
E053
E056
E058
E059

EO5A
EO5B
EOSC
EOSF
E060
E062
EC64
E066
E068
E069
EO6A
E06D
EOGE
E071
E074
E075
E076
E077
EO7A

1806

319
CD25DF
7D
CD25DF
E1

C9

7E

23

E3

EB
CD25DF
EB

E3
CDB8FF
20F2
D1

C9

D5

C5
CDBEEC
C1
30E8
FE02
3E1B
2801
3C

D1
CD25DF
ES
21C2B0
CD23FF
F5

7E

25
CD25DF
F1

jr

ld
call
id
call
pop
ret

id
inc
ex
ex
call
ex
ex
call
jr
pop
ret

push
push
call
pop
jr
Cp
1d
jr
inc
pop
call
push
ld
call
push
ld
inc
call
pop

BASIC 1.0

EO4S

a,19 token pour valeur sur un octet
DF25 écrire dans buffer

a, ]

DF25 écrire dans buffer

hi

a,(h1)

hl

(sp},hl

de,hi

DF25 écrire dans buffer
de,hl

(sp),hl

FFB8 comparer hl <> de
nz, EO4A

de

de

bc

ECBE

bc

nc, EO4A

02

a, 1B token pour nombre binaire
Z,E069

a

de

DF25 écrire dans buffer

hl

h1,BOC2

FF23 aller chercher type de variable
af

a, (nl)

hl

DF25 écrire dans buffer

af

-II1 128-

EO7B
EO7C
EO7E
EO7F

E080
E082
EO84
E086
E088
E089
EO8A
EO8C
EO8E
E090
E091
EO94
E097
E098
EO9A
EO9B
EO9D
EO9F
EOA1
EOA2
EOA3
EODA6
EOA9
EOAA
ECAB
EOAC
EOAE
EOBO

EOB3
EOB4
EOB5
EOB7
E0B8
ECBB

3D
20F6
ET
cg

FE22
285B
FE7C
2845
C5

D5
EE3F
O6BF
2816
2B
114BE6
CD27E3
1A
3808
7E
FE20
3002
3E20
23

47
CDB3EO
3239AE
78

D1

C1
FECO
2836
C325DF

3D

C8
EE22
C8
ZA39AE
3C

dec
jr

pop
ret

cp
jr
cp
jr
push
push
xor
id
jr
dec
1d
call
ld
jr
id
cp
jr
id
inc
id
call
ld
id
pop
pop
CP
jr
jp

dec
ret
xor
ret
ld

inc

BASIC

a
nz,E074
hl

22

Z, EOBF
7C
Z,E0CD
bc

de

3F
b,BF
Z,E0A6
hl

de, E64B
E327

a, (de)
c,EUA2
a,(hl)
20
nc,EOA1
a, 20
hl

b,a
EOB3
(AE39),a
a,b

de

bc

C0
Z,E0E6
DF25

a

z

22

z

a, (AE39)
a

1.0

tu

'EBIEA', extension d'instruction

vou
PRINT’

adresse des opérateurs Basic

15!

15!

écrire dans buffer

‘ut

-IIT 129-

BASIC 1.0

EOBC C8 ret zZ

ECBD 3D dec a

EOBE C9 ret

EOBF CD25DF cal] DF25 écrire dans buffer
EOC2 7E ld a, (hl)

EOC3 B7 or a

EOC4 C8 ret Z

EOC5 23 inc hl

EOC6 FE22 cp 22 FA

EOC8 20F5 Jr nz, EOBF

EOCA C325DF Jp DF25 écrire dans buffer
LÉERE SELS SSL LS LSLLSLSLSLLSLILLILIZLS. traiter extension d'instruction
EOCD CD25DF cal] DF25 écrire dans buffer
EODO AF xor a zéro

-111 130-

EOD1
EOD4
EOD7
EOD8
EOD9
EODC
EODE
EODF
ECEO
EOE1
ECE3
EOE4
EOES

EOE6
EOE8
ECEB
ECED
EOFO
EOF1
EOF2
EOF3
EOFS
EOF6

3239AE
CD25DF
7E

23
CD/BFF
38F6
2B

1B

TA
F680
12

13

C9

3E01
CD25DF
3EC0
CD25DF
7E

23

B7
20F8
2B

cg

ld
call
1d
inc
call
Jr
dec
dec
1d
or
Id
inc
ret

ld
call
ld
call
ld
inc
or
Jr
dec
ret

BASIC

(AE39),a
DF25
a, (hl)
hl
FF7B
c,E0D4
hl

de

a, (de)
80
(de),a
de

a,01
DF25

a, CO
DF25

a, (hl)
hl

a

nz, EOED
hl

HDI DEHEDÉEDEHEDE ND DE DE HE HE HE DE CHEDÉHÉ HÉENEDEHEHJENE NE

EOF7

EOFA
EOFB
EOFC
EOFF

E102
E105
E106
E107
ETOA

CDBOCE

C5

D5
CDC6C1
CDUADD

CDCBDD
Di
C1
CDODE1
C364C0

call

push

push

call
call

call
Pop
pop
call
JP

CEBO

bc

de

C1C6
DDHA

DDCB
de
bc
E10D
C064

HERHEONHEODOHEENE HE HE MEME HE OHEDÉDEDÉ ME EEE SE DE JE HE GENE

1.0

écrire dans buffer

prochain caractère
augmenter pointeur

tester si lettre ou chiffre
oui, alors dans buffer

rétrograder pointeur

mettre bit 7 pour dernier caractère

écrire dans buffer
écrire dans buffer
caractère

jusqu'à fin de ligne
écrire dans buffer

instruction Basic LIST
aller chercher zone de numéros de
ligne

aller chercher numéro canal

fin de l'instruction, sinon ‘’Syntax
error’
adresse de ligne actuelle sur zéro

lister lignes
au mode READY

lister lignes Basic bc -de

-111 130-

E10D
ET0E
ETOF
E110
E113
E114
ETS
E116
E117
E118
E119
ETIA
E11B
EVE
ETF
E120
E121
E122
E123
E124
E125
E126
E127
E128
E129
E12A
E12D
E12E
E130
E133
E136
E137
E138
E139
E13B
EI3E
E13F
E140

E142

D5

50

59
CDA3E7
D1

LE

23

46

2B

78

B1

C8
CD3CC4
ES

09

E3

D5

ES

23

23

BE

23

56

E1

E3
CDB8FF
E3
3812
CD63E1
CD45E1
23

7E

B7
20F8
CDAECS
Di

ET
18D2

ET

push
ld
id
call
pop
ld
inc
1d
dec
ld
or
ret
call
push
add
ex
push
push
inc
inc
1d
inc
1d
pop
ex
call
ex
Jr
call
call
inc
ld
or
jr
call
pop
POP
jr

POP

BASIC

de

d,b
e,C
E7A3
de
c,(hl)
hl

b, (h1)
h1

a,b

C

Z

C43C

hi
h1,bc
(sp),hl
de

hl

hl

hl
e,(h1)
hl
d,(h1)
hl
(sp},hl
FFB8
(sp},hl
c,E142
E163
E145

h1

a, (h1l)
a
nz,E133
C3UE

de

nl

E114

hl

1.0

numéro de ligne dans de

chercher ligne Basic de

fin du programme ?

terminé
interruption par 'ESC' ?

additionner longueur de ligne

prochain numéro de ligne dans de

comparer hl <> de

supérieur dernier numéro de ligne ?
lister ligne Basic dans buffer
sortir un caractère tiré du buffer

dernier caractère ?
non
sortir LF

-IIT 131-

E143
E144

E1
C9

pop
ret

BASIC

hl

EEE RD DEMO DE DE DE DD DE DEEE HE JE ME DE DE GENE NN

E145
E148
ET4A
E14B
ETUE
E150
E151
E153

CDBAC1
380B
7E
CD6EC3
FEOA
CO
3EOD
180B

call
jr
ld
call
Cp
ret
ld
Jr

C1BA
c,E155
a, (hl)
C36E
DA

nz

a, OD
E160

PEDERODEOMEDEDE DE DE DE DE DEEE DEN DE DE DE DE DE DE DE DE DE DE DEEE NE

E155
E156
E158
E15A

E15C
E15F
E160

ZE

FE20

3006
3E01

CD6EC3
7E
C36EC3

ld
CP
jr

1d

call
ld
Jp

a,(hl)

20

nc,E160
a, 01

C36E
a, (hl)
C36E

DEEE ED HEDEHEHEMEDE DE DE DE DE DE DE DEEE DH DE DE DEEE DE JEDE HE

E163
E164
E167
E168
E169
E16A
E16B
E16C
E16D
E16E
E16F
E170
E173
E176
E179
E17A

D5
OTAUAC
C5
23
23
SE
23
56
23
E5
EB
CDODFF
CD82EE
110000
7E
23

push
1d
push
inc
inc
Id
inc
1d
inc
push
ex
call
call
ld
1d
inc

de
bc,ACAU
bc

hl

hl
e,(hl)
h1
d,(h1)
hl

hl
de,h1
FFOD
EE82
de, 0000
a, (hl)
hl

1.0

sortir un caractère tiré du buffer
canal de sortie inférieur 8 ?
oui, Sortie sur écran

sortir caractère
LF ?

envoyer CR à la suite

sortie sur écran
aller chercher caractère
caractère de contrôle ?
non, sortir tel quel
caractères de contrôle comme
caractères imprimables
sortir caractère
aller chercher caractère
et le sortir

lister ligne Basic dans buffer

pointeur sur buffer d'entrée

numéro de ligne dans de

prendre nombre entier hl
convertir en ASCII

-111 132-

E17B
E17C
EVE
E181

E183
E185
E188
E189

E18A
E18B
E18D
E190

E192
E193
E194
E195

E196
E199
E19C
EYE
E1A0
ETA2
ETA4
ETA6
E1A8
ETAA
ETAC
ETAE
E1B0
E1B2
E1B4
E1B7
E1BA
E1BB

E1BD

B7
2805
CDFEE1
18F6

3E20
CDFEE1
E1

7E

B7
2805
CD96E 1
18F7

02
E1
Di
cg

CD13AC
FA20E2
FEO2
381D
FEO5
3843
FEOB
3822
FEOE
383B
FE20
382E
FE7C
2851
CDEADF
DCIAE2
7E
180D

23

or
ir
call
jr

1d

call

POP
ld

or
jr
call
Jr

ld

pop
pop
ret

call
Jp
CP
ir
CP
Jr
CP
Jr
Cp
Jr
cp
jr
CP
jr
call
cal]
id
Jr

inc

BASIC 1.0

a

Z,E183

E1FE écrire dans buffer
E179

a, 20 LDX
E1FE écrire dans buffer
hl
a, (hl) aller chercher caractère dans
programme

Z,E192 fin de ligne ?
E196 étendre token
E189

(bc),a
hl
de

AC13 ret

m, E220 token d'instruction ?
02

C,E1BD

05

C,E1E7

0OB

c,E1CA

0E

C,E1E7

20 15?

C,E1DE sortir constante

7C 'EBIEA', extension d'instruction
Z,E205

DFEA

C,E21A sortir espace

a, (hl)

E1CA

hl

-II1 133-

E1BE
E1BF
ETC
E1C3
ECS
E1C7
E1C8
ETCA
E1CC
EICE
E1D0
E1D3
E1D4
E1D5
E1D6
E1D7
E1D9
ETDB
E1DC

E1DE
EE
ETE4
EIE6

ETE7
ETEA
EIEB
EIEC
ETED
EEE
EIEF
ETF2
E1F3
E1F5
E1F7
E1F8
ETFA
EIFC
EIFE

7E
FECO
285D
FE97
2859
2B
3E3A
1E00
FE22
200B
CDFEE1
23
7E
B7
C8
FE22
20F5
25
1820

CD1AE2
CD53E2
1E01
cg

CDTAE2
7E

F5

23

23

23
CDOFE2
F1
1E01
FEOB
DO
1E00
EE27
E6FD
02

ld
Cp
Jr
CP
jr
dec
ld
1d
CP
Jr
call
inc
ld
or
ret
CP
Jr
inc
Jr

call
cal]
1d
ret

call
ld
push
inc
inc
inc
call
pop

cp
ret
Id
xor
and
1d

a, (h1)
Co
Z,E220
97
Z,E220
hl

a, 3A
e,00
22
nz,E1D
EFE
h1
a,(hl)
a

Z

22
nz,E1D
hl
E1FE

E21A
E253
e,01

E21À
a, (hl)
af

hl

hl

hl
E20F
af
e,01
0B

nc
e,00
27

FD
(bc),a

BASIC 1,0

ELSE”

ft
ï

aus

B
écrire dans buffer

ut

0

écrire dans buffer

sortir espace
sortir constante

sortir espace

écrire caractère dans buffer

-111 134-

ETFF
E200
E201
E202
E203
E204

ROMEO DE DE DE HOME MEME DEEE HE HOMME HERO CH

E205
E207
E20A
E20B
E20C
E20D
E20E
E20F
E210
E212
E215
E216
E217
E219

E21A
E21B
E21C
E21E
E220
E221
E223
E225
E226
E227
E228
E229
E22C
E22D
E22F
E230
E233

03
15
Co
OB
14
C9

1E01
CDFEE1
23

7E

23

B7

le)

7E
E67F
CDFEE1
BE

23
30F6
C9

1D

CO

3E20
18DE

23

FEFF

2002

7E

23

F5

ES

CDEDE2

B7

2808

F5

CDIAE2

F1

inc
dec
ret
dec
inc
ret

id
call
inc
id
inc
or
ret
id
and
call
CP
inc
jr
ret

dec
ret
id

inc
Cp
jr
ld
inc
push
push
call
or
Jr
push
call
pop

BASIC 1,0

bc

nz
pc

e,01
E1FE
hl

a, (nl)
hl

a

nz

a, (hl)
7F
E1FE
(h1)
hl

nc, E20F

e

nz

a, 20
jr

hl

FF

nz, E227

a, (hl)

hi

af

hi

E2ED

a

z,E237

af

E21À

af

augmenter pointeur de buffer

lister extension d'instruction

écrire dans buffer

prochain caractère
augmenter pointeur

fin de ligne ?

aller chercher caractère
annuler bit 7

écrire dans buffer
dernier caractère ?

non, prochain caractère

16!
E1FE écrire dans buffer

fonction ?
non

lister token ?

-111 135-

E234
E237
E238
E23A
E23C
E23F
E240
E241
E243
E246
E248
E24A
E24C
E24D
E2UE
E250
E251
E252

E253
E254
E255
E256
E258
E25A
E25C
E25E
E260
E262
E264
E266
E268
E26A
E26C
E26E
E270
E272
E274
E275

CDFEE1
7E
E67F
FE09
CUFEE1
BE

23
28F4
CD7BFF
1E00
3002
1E01
El

F1
D6E 4
(re)

SF

cg

D5

7E

23

FE1B
2849
FEIC
2850
FETE
2826
FE1D
2822
FETF
285E
FE19
2809
FETA
280B
D6CE
5F

1802

call
Id
and
Cp
call
cp
inc
jr
call
ld
jr
Id
pop
pop
sub
ret
id
ret

push
1d
inc
Cp
jr
cp
jr
cp
jr
cp
jr
cp
jr
cp
jr
cp
jr
sub
ld
jr

E1FE
a,(hl)
7F

09
nz,E1F
(hl)
nl
Z,E237
FF7B
e,00
nc, E24
e,01
hl

af

E4

nz

e,a

de

a, (nl)
hi

1B
Z,E2A3
1C
Z,E2AE
1E
z,E288
1D
z,E288
1F
z,E2C8
19
Z,E277
1A
Z,E27D
0Ë

e,a
E279

BASIC 1.0

écrire dans buffer

E écrire dans buffer

tester si lettre ou chiffre

C

nombre binaire ?

nombre hexadécimal ?
adresse de ligne ?

numéro de ligne ?

nombre à virgule flottante ?
nombre sur un octet ?

nombre sur deux octets ?

chiffre ?

-111 136-

BASIC

DEDENEHDEDENENENE DE NEHE DE DE MEME HE DE DE DE DE DE DE DE DE DE EDEN N

E277 GE id e,(hl)
E278 23 inc nl

E279 1600 ld d,00
E27B 1804 jr E281
CELL LLLÉESLSLÉELLLSLSLSSLSLLÉESSLSELE ES)
E27D SE id e,(hl)
E2/E 23 inc hl

E27F 56 1d d,(hl)
E280 23 inc hi

E281 E3 ex (sp},hi
E282 EB ex de,hl
E283 CDODFF call FFOD
E286 1847 jr E2CF
LELSZSLSLLLILLLLLLLLLSLLLÉESISLLSLLLS
E288 SE ld e,(hl)
E289 23 inc hl

E28A 56 ld d,(h1)
E28B 23 inc hl

E28C FEIE cp 1E

E28E 2809 Jr z,E299
E290 E5 push hl

E291 EB ex de,h1
E292 23 inc hl

E293 23 inc hl

E294 23 inc hl

E295 GE ld e,(h1)
E296 23 inc hl

E297 56 ld d,(h1l)
E298 E1 pop hl

E299 E3 ex (sp),h]
E29A EB ex de,hl

E29B CDODFF call FFOD
E29E CD82EE call EE82
E2A1 182F ir E2D2

LELEL LISE SSL LS LE LL LL. LS LL LL LL LL.)

E2A3 C5 push bc

1,0

sortir nombre sur un octet

Lo-byte

Hi-Byte zéro

sortir nombre sur deux octets

Lo-Byte

Hi-Byte

prendre nombre entier hl

sortir numéro de ligne

numéro de ligne ?
oui

prendre nombre entier hl
convertir en ASCII

sortir nombre binaire

-111 137-

BASIC

E2A4 010200 id bc, 0002
E2A7 CDI4F1 call F114

E2AA 3E58 1d a,58
E2AC 1809 Jr E2B7

SM DE DE DE DE DE DEDE DE DD DE DEEE DE DE DE DEEE DE DE DE DEEE
E2AE C5 push bc

E2AF 010200 Id bc,0002
E2B2 CD19F1 call F119

E2B5 3E48 ld a,48
E2B7 C1 pop bc

E2B8 E3 ex (sp),hl
E2B9 EB ex de,hl
E2BA F5 push af

E2BB 3%E26 ld a, 26
E2BD CDFEE] call E1FE
E2C0 F1 pop af

E2C1 FEu8 cp 48

E2C3 C4FEEI call nz,E1FE
E2C6 1804 Jr E2D2
LÉELESLLLLLELRELLLELRELLLLEILLL IL LLLESL]
E2C8 3E05 1d a,05
E2CA CDABFF call FF4B
E2CD E3 ex (sp),hl
E2CE EB ex de,h]
E2CF CD8FEE call EE8F
E2D2 7E ld a, (hl)
E2D3 23 inc hl

E2D4 CDFEE1 call E1FE
E2D7 7E ld a,(hl)
E2D8 B7 or a

E2D9 20F7 Jr nz,E2D2
E2DB E1 pop hl

E2DC C9 ret
LÉLLELLLS LIL S LISE LSLLLLSRLLLLILS TS]
E2DD ES push hl

E2DE D641 sub 41

E2E0 87 add a,à

1.0

convertir en nombre binaire
tx

sortir nombre hexa

convertir en nombre hexa
FH

Eu
écrire dans buffer

'H' pas

écrire dans buffer

sortir nombre à virgule flottante
type de variable ‘Real’
aller chercher nombre

convertir en ASCII
aller chercher caractère
écrire dans buffer

fin du nombre ?
non

-I11 138-

E2E1
E2E3

E2E4
E2E6
E2E7
E2E8
E2E9
E2EA
E2EB
E2EC

E2ED
E2EE
E2EF
E2F1
E2F4
E2F7
E2F9
E2FA
E2FC
E2FF
E302
E304
E306
E307
E309
E3OA

E30B
E30E
E310

E313
E314
E315
E316
E317
E318
E319

C654
6F

CEE3
95
67
5E
23
56
Eî
cg

C5

UF
0614
2188E3
CD13E3
380D
23
10F8
214BE6
CD13E3
3007
06C0
78
C640
C1

C9

CD19AC
1E02
C394CA

7E
B7
c8
E5
7E
23
17

add

adc
sub
1d
id
inc
1d
pop
ret

push
id
1d
ld
call
Jr
inc
dinz
1d
call
Jr
ld

d
add
pop
ret

call
ld
JP

ld
or
ret
push
1d
inc
rla

BASIC 1,0

1,a plus E354, adresses des mots-
instructions
a,E3
l
ha
e,(h1)
hl
d,(h1)
hl

bc

c,a

b,1A 26 lettres

h1,E388 table des mots instruction
E313

c,E306

hl

E2F4 lettre suivante

hl,E64B table des opérateurs Basic
E313

nc, E30B

b, CO
a, D

a, 40

bc

AC19 ret
e,02 ’Syntax error’
CA94 sortir message d'erreur

a,(hl)
a
Z
hl
a, (hl)
hi

-II1 139-

E3TA
E31C
E31D
E31E
E31F
E321
E322

E324
E325
E326

E327
E328
E329
E32A
E32B
E32C
E32D
E32F
E331
E333
E335
E338

E33A
E33B
E33C
E33D
E340
E341
E343
E345
E347
E348
E349
E3UA
E34B
E34D
E3UE

30FB
7E
23
B9
2803
F1
18EF

EI
37
cg

1A
B7
C8
E5
TA
13
FE09
2804
FE20
2005
CD61DD
18F1

UF
7E

23
CD8AFF
A9
28E8
E67F
2804
1B

1A

13

17
30FB
13

E

Jr
ld
inc
CP
jr
pop
jr

pop
scf
ret

ld
or
ret
push
ld
inc
cp
Jr
cp
Jr
call
jr

1d
id
inc
call
xor
Jr
and
Jr
dec
ld
inc
rla
Jr
inc
pop

BASIC 1.0

nc,E317
a, (h1)
hl

C
2,E324
af

E313

hl

a, (de)

a

Z

h1

a, (de)

de

09 TAB ?
Z,E335

20 5!
nz,E33A

DD61 ignorer espace, TAB et LF
E32B

c,à

a, (h1)
hl
FF8A convertir minuscules en majuscules
C
z,E32B
7F
Z,E351
de

a, (de)
de

nc,E348

de
hl

-111 140-

BASIC 1,0

E34F 18D6 Jr E327

E351 F1 pop af

E352 37 scf

E353 C9 ret

LELLLLESLLSLLLLLSLLLLLLLLLSL EL] adresses des mots instructions
E554 35E6 dn E635 À
E356 2AE6 dw E62A B
E358 EFES5 dw ESEF C
E35A B9ES dun ESB9 D
E35C 8AES dw ES8A E
E3SE 7EES dw ES7E F
E360 72E5 dw E572 G
E362 68E5 dw E568 H
E364 47ES dn ES47 I
E366 43E5 dW E543 J
E368 3FE5 dn E53F K
E36A 13E5 dw E513 L
E36C EDE4 dn EUED M
E36E E2E4 dw EUE2 N
E370 AAE4 dw EAAA 0
E372 86E4 dw E486 P
E374 85E4 dw E485 Q
E376 3BE4 dw E43B R
E378 FBE3 dn E3FB S
E37A CFE3 dy E3CF T
E37C COE3 dw E3C0 U
E37E B8E3 dw E3B8 V
E380 9AE3 dw E39A W
E382 92E3 dw E392 X
E384 8DE3 dn E38D Y
E386 88E3 dw E388 Z

RER EME Taple des instructions Basic

-II1 141-

BASIC 1.0

OUR ON ONONOUOUONONONONONONONONORONONONONONONOMONONONONONONONONON NOR RU

E388 4F 4E C5
E38C 00

DA ZONE

ARR RRONONONONOUOROUONONONONONONONONONOMONONOMONONONONONONONONONONEONEON NE NENEEE EEE N

E38D 50 4F D3
E391 00

48 YPOS

AU UUUUNUNNONOMONONONNONONOEONEONEONE NME NEONE MH HN ONNNO ONE NE EE

E392 50 4F D3
E396 4F D2
E399 00

47 XPOS
FD XOR

ENORME

E39SA 52 49 54 C5
E39F 49 4E 44 4F O7
ESAS 49 44 54 C8
E3SAA 48 49 4C C5
ESAF 45 4E C4

E3B3 41 49 D4

E3B7 00

D9 WRITE
D8 WINDOW
D7 WIDTH
D6 WHILE
D5 WEND
D4 WAIT

ROROMOROMOUOUOMOUOONOROUONONONONONOUONONONONEONOEOUEONONCEONONOONOMONEONOMEONEONOMEONENONEON ON

E3B8 50 4F D3
E3BC 41 CC
E3BF 00

7FVPOS
1DVAL

RRRUROUOUOUONOUOUONONOUONONE NON NE ONEOHE ONE

E3CO 53 49 4E C7
E3CS 50 50 45 52 A4
E3CB 4E D4

E3CE 00

ED USING
1C UPPERS
1BUNT

AOROROUOMOMORORONORONONORORONONONONONONONONONOUONONONONONONONONONONONONONOMONOMOUEOHEONNE HN

E3CF 52 4F CE
E3D3 52 4F 46 C6
E3D8 CF

ESDA 49 4D C5
ESDE 48 45 CE
ESE2 45 53 54 D2
E3E7 45 53 D4
E3EB 41 CE

ESEE 41 47 4F 46 C6
E3F4 41 C7

ESF7 41 C2

E3FA 00

D3 TRON
D2 TROFF
EC TO

46 TIME
EB THEN
7DTESTR
7C TEST
1ATAN
D1 TAGOFF
DO TAG
EATAB

RU ROAONOROROROROROEOUOREONOMORONON ANNEE RON

ESFB 59 4D 42 4F CC
E401 57 41 DO

E405 54 52 49 4E 47 A4
E40C 54 52 A4

E410 54 4F DO

E414 54 45 DO

E418 51 D2

CF SYMBOL
E7 SWAP
7B STRING$
19STR$

CE STOP

E6 STEP

18 SOR

-111 142-

E41B
E41D
E422
E425
E42B
E430
E433
E436
E43A

Di

50 45 45 C4
50 C3

50 41 43 45 A4
4F 55 4E C4
49 CE

47 CE

41 56 C5

00

BASIC 1.0

17SQ

CD SPEED

E5 SPC

16 SPACES
CC SOUND
15 SIN

14 SGN

CB SAVE

ARR MEEEREEEREEREEEEEONONORONONONONONONONONN N #

E43B
E43E
E443
E446
E44C
E452
E458
E45F
E464
E46A
E460
E474
E478
E481
E484

55 CE

4F 55 4E C4

4 C4

49 47 48 54 A4

45 54 55 52 CE
45 53 55 4D C5

45 53 54 4F 52 CS
45 4E 55 CD

45 4D 41 49 CE
45 CD

45 4C 45 41 53 C5
45 41 C4

41 4E 44 4F 4D 49 5A C5
41 C4

00

CA RUN

7A ROUND
45 RND

79 RIGHTS
C9 RETURN
C8 RESUME
C7 RESTORE
C6 RENUM
13 REMAIN
C5 REM

C4 RELEASE
C3 READ

C2 RANDOMIZE
C1 RAD

ARR ONOMONEONONURREOUEOKONEONONOMONOHOEOROHEOEOHEUEOEONHEOEHEUEOEONONOUNENEOREOEONONEONN NN

E485

00

ORNE NEEOUEOUEONEOUEOREOUECHEOHE OUEN NN

E486
E48B
E48E
E492
E497
E49B
E49)
E4A0
E4A4
E4A9

52 49 4E D4
4F D3

4F 4B C5
4C 4F 54 D2
4C 4F D4
cg

45 CE

45 45 CB
41 50 45 D2
00

BF PRINT
78 POS
BE POKE
BD PLOTR
BC PLOT
44PI

BB PEN
12 PEEK
BA PAPER

MORE RH ROHAN NEURONES

E4AA
E4AD
E4B3
E4B5
E4BC
E4C2
E4C7
E4CF
E4D7
E4DF
E4E1

55 D4

52 49 47 49 CE

D2

50 45 4E 4F 55 D4

50 45 4E 49 CE

4 20 53 Di

4E 20 45 52 52 4F 52 20
47 4F 09 54 4F 20 B0
4E 20 42 52 45 41 CB
CE

00

B9 OUT

B8 ORIGIN
FC OR

B7 OPENOUT
B6 OPENIN
B5 ON SQ

B4 ON ERROR GO TO 0
B3 ON BREAK
B2 ON

-[I1 143-

BASIC 1.0

MR RSR RUN RU SE RUOUOUOUOUOUONOUOUOUONOUONONONONONONOUONONEOONOAEONONK

E4E2 4F D4
E4ES 45 D7
E4E8 45 58 D4
E4EC 00

FE NOT
Bi NEW
BO NEXT

RON OROROUOROUONORORONONONONONONOROUONOUENORONOUENONOONONORONEONONE RON UMR

E4ED 4F 56 45 D2
E4F2 4F 56 C5

E4F6 4F 44 C5

E4FA 4F C4

E4FD 49 CE

E500 49 44 A4

E504 45 52 47 C5
E509 45 4D 4F 52 D9
E50F 41 D8

E512 00

AF MOVER
AE MOVE

AD MODE
FB MOD

77 MIN

AC MID$

AB MERGE
AA MEMORY
76 MAX

BRUNO OROUOUOUOUONONOUONOUOMONONOUONO NON HONNEUR

E513 4F 57 45 52 A4
E519 4F 47 31 B0
ESTE 4F C7

E521 4F 43 41 54 C5
E527 4F 41 C4

E52B 49 53 D4

E52F 49 4E C5

E533 45 D4

E536 45 CE

E539 45 46 54 A4
E53€ 00

11 LOWERS
10L0G10
0F LOG

A9 LOCATE
A8 LOAD
A7 LIST

AG LINE

A5 LET

0E LEN

75 LEFT$

LRERLLELELEELLE ELLE ELE LEE LIELIS LE EEE SL LS LIL LL LLLL))

E53F 45 D9
E542 00

RON OUOUONOUOUOMOUOROUONOUONONOUONONOUONONONOUONUONONNONONONNNONHENON ONE

E543 4F D9
E546 00

RER LESELSEELLELLELSEE EEE SELE SELLES LL ELLE ELLE LEE LL LIL)

E547 4E D4

ES4A 4E 53 54 D2
E54F 4E 50 55 D4
E554 4E DO

E557 4E 4B 45 59 A4
E55D 4E 4B 45 D9
E562 4E CB

ES65 C6

E567 00

A4KEY

0D JOY

OC INT

74 INSTR
A3 INPUT
0B INP

43 INKEY$
OA INKEY
A2 INK

AT IF

LRELLELELELLELELEIERESE EEE EEE EE RES SE LS TL)

E568 49 4D 45 CD
E56D 45 58 A4
E571 00

42 HIMEM
73HEX$

-IIT 144-

BASIC 1.0

RME RRRRR RER RU RR RSR RER RU RRNRRR RSR RN RSS NS

E572
E577
E57D

4F 09 54 CF
4F 09 53 55 C2
00

A0 GO TO
9F GO SUB

ROROUORORONOROMOUOUONONOROROUNOROOR OO OUR RON

E57E
E581
E584
E586
E589

52 C5
4F De
CE
49 D8
00

09 FRE
SE FOR
E4FN
08 FIX

LÉREEESLLESESRLSIR ISLE LL IEEE RRERLRE RIRE SERR RIRE EEEE ES,

E58A
E58D
E592
E597
E59A
E59D
ESA2
ESAS
ESAB
ESAB
ESAE
E5B2
E5B4
E5B8

58 DO

56 45 52 D9
52-52 4F D2
52 D2

52 CC

52 41 53 C5
4F C6

4E D6

4 D4

4E C4

4C 53 C5
C9

44 49 D4
00

07 EXP
9D EVERY
9C ERROR
AERR
E3ERL
9B ERASE
40 EOF
JA ENV
99 ENT
98 END
97 ELSE
DC EI

96 EDIT

LÉLRELLESLLLLLLLSELLILLLLLLLELSELLLLLLLSERSLERRES LIRE]

E5B9
E5BE
E5C2
E5C5
E5C7
E5CD
E5D0
ESD6
E5DD
E5E3
ESE6
ESEA
ESEE

52 41 57 D2
52 41 D7

49 CD

C9

45 4C 45 54 C5
45 C7

45 46 53 54 D2
45 46 52 45 41 CC
45 46 49 4E D4
45 C6

45 43 A4

41 54 C1

00

95 DRAWR
94 DRAW
93 DIM

DB DI

92 DELETE
91 DEG

90 DEFSTR
8F DEFREAL
8€ DEFINT
8D DEF

72 DECS
8C DATA

ROOMS

ESEF
E5F4
E5F7
E5FB
E5FE
E606
E600
E610
E615
E619
E61D

52 45 41 CC

4F D3 OS 05

4F 4 D4

4C D3

4C 4F 53 45 4F 55 D4
4C 4F 53 45 49 CE
4C C7

4C 45 41 D2

49 4 D4

48 52 A4

48 41 49 CE

06 CREAL
05 COS

8B CONT
8ACLS

89 CLOSEOUT
88 CLOSEIN
87 CLG

86 CLEAR
04 CINT

03 CHRS

85 CHAIN

-11II 145-

BASIC 1.0

E622 41 D4 84 CAT

E625 41 4C CC 83 CALL

E629 00

CÉRRE EEE TETE
E62A 4F 52 44 45 D2 82 BORDER

E630 49 4E A4 71 BINS

E634 00

CPÉÉREEEE SET TT
E635 55 54 CF 81 AUTO

E639 54 CE 02 ATN

E63C 53 C3 01 ASC

E63F 4E C4 FA AND

E642 46 54 45 80 AFTER

E647 42 D3 00 ABS

E64A 00

-III 146-

BASIC 1,0

DANONE opérateurs Basic et token correspondants
E64B DE db DE Hé

E64C F8 db F8

E6UYD DC db DC "Backslash'
EGUE F9 db F9

E6uF  3EO9BD db 3E,09, BD 1>=!

E652 FO db FO

E653 3D20BE db 3D,20,BE "=>!

E656 F0 db F0

E657 BE db BE >

E658 EE db EE

E659 BD db BD "=!

E65A EF db EF

E65B 3CO9BE db 3C,09, BE <>!

E65E F2 db F2

E65F  3CO9BD db 3C,09, BD "<=!

E662 F3 db F3

E663 3D20BC db 3D,20, BC "=<'

E666 F3 db F3

E667 BC db BC '<!

E668 F1 db F1

E669 AF db AF Ep

E66A F7 db F7

E66B BA db BA Fa

E66C O1 db 01

E66D AA db AA SA

E66E F6 db F6

E66F AD db AD =}

E670 F5 db F5

E671 AB db AB ‘+!

E672 F4 db F4

E673 A7 db A7 EE

E674 CO db CO

E675 00 db 00

LILI LLLLLLL LL LILI LIL SI LL LLLLL LL LL LL] supprimer pointeur de programme
E676 AF xor a

E677 323AAE ld (CAE3A),a

E67A 2A8I1AE ld h1, (AE81) début de programme
E67D 77 ld (hl),a

-111 147-

E67E
E67F
E680
E681
E682
E683
E686

23
77
23
77
23
2283AE
C9

inc
ld
inc
1d
inc
1d
ret

hl
(hl),a
hl
(hl),a
hl
CAE83)

BASIC 1.0

trois fois zéro en fin de programme

hl fin de programme

EDEN DE DE DE DE DE DD DU DE DE DE DE DE DE DE DE DE DE D DE DE DE DE

à, (AE3A)

E687
E68A
E68B
E68C
E68D
E68E
E68F
E692
E695
E696
E699
E69A
E69B
E69C

3A3AAE
B7
c8
C5
D5
E5
019DE6
CDFFE8
AF
323AAE
EI
D1
C1
C9

ld
or
ret
push
push
push
1d
call
xor
1d
pop
pop
pop
ret

LÉRÉELSELELLELETS EL:

E69D CD43E9

E6AO
E6A2
E6A3
EG6AS
E6A7
EG6A8
E6A9
EGAA
EG6AB
EG6AC
EGAD
EGAE
EGAF

FE02
D8
FE1D
20F6
56
2B
5E
2B
ES
EB
23
23
23

call

Cp
ret
cp
Jr
1d
dec
ld
dec
push
ex
inc
inc
inc

a
zZ

bc

de

hl
bc,E69
E8FF

a
(AE3A)
hl

de

bc

rempla
E943

02

C

1D
nz,E69
d,(hl)
hl
e,(hl)
hl

h]
de,h]l
hl

hl

hl

D ‘utiliser numéros de ligne

,à

cer adresse de ligne par numéros de ligne
aller chercher prochain élément de
la ligne
fin de l'instruction ?
oui
‘adresse de ligne’ ?
D non

-111 148-

E6BO
E6B1
E6B2
E6B3
E6B4
E6B6
E6B7
E6B8
E6B9
EGBA

LÉLLSSS SIL LLLLLLELLL LL EL.)

E6BC
E6BF
E6CO
E6C1
E6C2
E6C5
E6C6
E6C7
E6C9
E6CB
E6CC

E6CF
E6DO
E6D1

LÉSÉSESSES SSL ELLE LL LEE S)

E6D2
E6D5

E6D8
E6D9
E6DC
EG6DD
EGDF
EG6EO
E6E1
E6E4

SE
23
56
ET
361E
23
73
23
72
181

CD61DD
B7

37

c8
CDOLEE
DO

7E
FE20
2001
235

CDD2E6

37
9F
cg

CD87E6
CDBBDE

E5
CD61DD
B7
2828
C5

D5
210400
09

id
inc
ld
pop
1d
inc
id
inc
ld
Jr

call
or
scf
ret
call
ret
Id
Cp
jr
inc
call

scf
sbc
ret

call
call

push
call
or
Jr
push
push
ld
add

BASIC 1.0

e,(hl)

h1 numéro de ligne dans de
d,(h1)

hl

(h1),1E ‘numéro de ligne’

hl

(hl),e

hl mettre en place

(h1),d

E69D

convertir ligne d'entrée en code interpréteur
DD61 ignorer espace, TAB et LF [berlesen
a

Z
EEO4
nc
a, (hl)
20 15’
nz, E6CC
hl
E6D2 convertir instruction en code
interpréteur

a,à

convertir instruction en code interpéteur
E687
DEBB token dans buffer à partir de
(&AE7F) (840)
hi
DD61 ignorer espace, TAB et LF
a
Z,E707
bc
de
h1,0004
hl,bc

-111 149-

EG6E5
E6E6
EGE7
EGEA
E6EB
EGEE
EG6EF
E6FO

E6F3

E6F6
E6F7
E6F8
E6F9
EGFA
E6FB
E6FC
E6FD
EGFE
E6FF
E700
E701
E702
E703
E704

E707
E708
E70B
E70C
E70D
E70E
E70F
E712
E715
E716
E717
E718
E719

E5
E5
CDA3E7
E5
DCOBE7
D1
C1
CDF8F5

CD2CFS

EB
D1
73
23
72
25
D1
73
23
72
235
C1
EB
E1
C3F2FF

E1
CD9AE7
cs
ES
09
EB
2AB9AE
CDCFFF
4u
4D
EB
D
78

push
push
call
push
call
pop

pop

call

call

ex
POP

inc
ld
inc
pop
1d
inc
ld
inc
pop
ex
pop
Jp

pop
cal]
push
push
add
ex
ld
call
ld
ld
ex
pop
ld

BASIC 1.0

hl
nl

E7A3 chercher ligne Basic de

hi
c,E70B
de

bc nombre d'octets

F5F8

réserver place dans zone de

variables
F52C augmenter pointeurs prg et variable

de b
de,hl
de
(hl).e
hl
(hl),d
hl
de
hl),e
h]
(hl),d
hl
bc
de,hl
hl
FFF2 ldir

hl

C

E79A chercher ligne Basic de

bc
hl
hl,bc
de,hl

hl,(AE89) fin des tableaux

FFCF hl
b,h

C, 1

de,hl

de

a,b

-III 150-

hl - de

E7TA
E71B
E71E
E71F
E722
E725

Bi
CUF2FF
D1

210000
CDDAFF
C32CF5

or
call
pop
ld
call
Jp

BASIC

C
nz, FFF2
de
h1,0000
FFDA
F52C

HOMO NON DE DD NE DEEE DD MDEDE DE DD DE DE ED DÉ DD DE DEN DE

E728
E72B

E72E
E731
E734

CD37E7
CD4ADD

CDSAE7
CD7AC1
C364C0

call
call

call
cal]
Jp

E737
DDHA

E75SA
C17A
CO64

1.0

ldir

bc := hl - de

augmenter pointeurs prg et variable
de bc

instruction Basic DELETE

fin de l'instruction, sinon ‘’Syntax

error’

au mode READY

ITEMS

BASIC 1,0

DERDEUDEDE DEEE HD DE DE DH DE DE DE ED DE DE DEEE DE DEEE HE

E737 CDBOCE call CEBO aller chercher zone numéros de ligne

E73A E5 push hl

E73B C5 push bc

E73C CDCIE7 call E7C1 chercher ligne Basic de

E73F D1 pop de

E740 ES push hl

E741 CDAGE7 call E7A3 chercher ligne Basic de

E744  223BAE ld (AE3B),h1

E747 EB ex de,hl

E748 El pop hl

E749  CDCFFF call FFCF hl := hl -de

E74C 223DAE ld (AE3D),h1

E74F 3804 Jr c,E755 ‘Improper argument

E751 7C ld a,h

E752 B5 or l

E753 Ei pop hl

E754 CO ret nz

E755 1E05 ld e,05 ‘Improper argument’

E757 C394CA Jp CA94 sortir message d'erreur

DEDEMEMEHEDEDEHEHEDEDE DD EE DEEE DE DE DM DE ED DE

E75A  CD87E6 call E687 remplacer adresses de ligne par
numéros

E75D EDUBSDAE ]d bc, (AE3D)

E761 2A3BAE id h1, (AE3B)

E764 C3OBE7 Jp E70B

LÉLSÉELLLL ESS LL LLLLELLLLLSLSLS SES) aller chercher adresse de ligne

E767 23 inc hl

E768 SE ld e,(hl)

E769 23 inc hl numéro ou adresse dans de

E76A 56 1d d,(h1)

E76B 23 inc hl

E76C FE1D Cp 1D ‘adresse de ligne’ ?

E76E C8 ret Z oui, terminé

E76F FEIE CP 1E ‘numéro de ligne’ ?

E771 C2EAES8 JP nz,E8EA non, ‘Syntax error’

E774 ES push hl

E775 CDD6DD call DDD6 aller chercher numéro ligne dans h1l

-111 152-

BASIC 1.0

E778 DCB8FF call c,FFB8 comparer hl <> de

E77B 3009 jr nc, E786 inférieur, alors chercher début PRG

E77D E1 pop hl

E7/E ES push hl

E77F CDF3E8 call E8F3 ignorer reste de la ligne

E782 23 inc hl à partir de l'adresse (h1)

E783 CDA7E7 call E7A7 chercher ligne Basic

E786  DA9AE7 call nc,E79A pas trouvé, chercher à partir de
début du programme

E789 2B dec hl

E78A EB ex de,hl

E78B E1 pop hl

E78C ES push hl

E78D 3E1D ld a,1D ‘adresse de ligne’

E78F 323AAE ld (AE3A),a

E792 2B dec hl

E793 72 ld (hl),d

E794 2B dec hl placer adresse de ligne dans le PRG

E795 73 1d (hl),e

E796 2B dec hl

E797 77 1d (h1),a token pour ‘adresse de ligne’

E798 E1 pop hi

E799 C9 ret

DOMOMOMOMOMEONEDEOMENENEUNE DE DEEE DE DEEE DE DE DE DE DE DD DEEE DEN chercher ligne Basic

E79A CDA3E7 call E7A3 chercher ligne Basic

E79D D8 ret C trouvée ?

E7SŒ 1E08 ld e,08 ‘Line does not exist’

E7AO C394CA jp CA94 sortir message d'erreur

DEDEHOMEHE DE HEHEDE HE DEMEHEDE ED DEDE DD HEDEOMEDEOMEDENE GENE DE DE HE chercher ligne Basic dans (de)

E7A3 2A81AE ld hl, (AE81) début du programme

E7A6 23 inc hl

E7A7 UE 1d c,(hl)

E7A8 23 inc h1 longueur de ligne dans bc

E7A9 46 ld b, (h1)

E7AA 2B dec hl

E7AB 78 ld a,b

E7AC B1 or C fin du programme ?

E7AD C8 ret Z pas trouvé

-111 153-

BASIC 1.0

E7AE ES push h1l

E7AF 23 inc hi

E7BO 23 inc hl

E7B1 7E ld a,(hl)

E7B2 23 inc hl numéro de ligne dans hl
E7B3 66 ld h, (h1)

E7B4 6F ld 1,a

E7B5 EB ex de,hl

E7B6 CDB8FF call FFB8 comparer hl <> de

E7B9 EB ex de,hl

E7BA El pop hl

E7BB 3F cof

E7BC DO ret nc supérieur, pas trouvé
E7BD C8 ret Z égal, trouvé

E7BE 09 add hl,bc additionner longueur de ligne
E7BF 18E6 jr E7A7 continuer à chercher
LELLLEL LILI SSISLLLSSSSLLSLSLLLLISLS] chercher ligne Basic

E7C1 2A81AE ld hl1, (AE81) début du programme

E7C4 23 inc h1

E7CS ES push hl ranger adresse de ligne
E7C6 LE ld c,(hl)

E7C7 23 inc hi longueur de ligne dans bc
E7C8 46 ld b,(h1)

E7C9 23 inc hl

E7CA 78 ld a,b

E7CB B1 or C

E7CC 280F jr Z,E7DD fin du programme ?

E7CE 7E ld a, (hl)

E7CF 23 inc hl numéro de ligne dans hl
E7D0 66 Id h, (h1)

E7D1 6F ld l,a

E7D2 EB ex de,hl

E7D3 CDB8FF call FFB8 comparer hi <> de

E7D6 EB ex de,hl

E7D7 3804 jr c,E7DD No de ligne actuel supérieur/égal?
E7D9 Ei pop h]

E7DA 09 add hi,bc additionner longueur de ligne
E/DB 18E8 jr E7C5 continuer à chercher

-111 154-

BASIC 1.0

E7DD Ei pop hi hl désigne adresse de ligne

E7DE C9 ret

LELLELSLLLLSLLLLLLL LL LS LL LLLLLS LS) instruction Basic RENUM

E7DF 110400 id de,0004A 10, défaut pour valeur de départ
E7E2 2805 jr Z,E7E9

E7E4 FE2C cp 2C AP

E7E6 C4E1CE call nz, CEE aller chercher No de ligne dans de
E7E9 DS push de

E7EA 110000 ld de,0000 0

E7ED CDS5SDD call DD55 virgule suit ?

E7FO 3005 Jr nc,E7F7 non

E7F2 FE2C CP 2C HA

E7F4 CUEICE call nz, CEE non

E7F7 DS push de

E7F8 110400 1d de, 0004 10, défaut pour incrément

E7FB CD55DD call DD55 virgule suit ?

E7FE DCEICE call c,CEE1 oui, aller chercher No ligne dans de
E801 CDAADD call DD4A fin de ligne, sinon ’Syntax error’
E804 El pop hl

E805 EB ex de,hl

E806 E3 ex (sp},hl

E807 EB ex de,hl

E808 D5 push de

E809 ES push hl

E80A CDA3E7 cali E7A3 chercher ligne Basic

E80D D1 pop de

E80E E5 push hi

E80F CDA3E7 call E7A3 chercher ligne Basic

E812 EB ex de,h1l

E813 E1 pop hl

E814 CDB8FF call FFB8 comparer hl <> de

E817 DASSE7 jp c,E755 ’Improper argument’

E81A EB ex de,hi

E81B D1 pop de

E81C C1 pop bc

E81D DS push de

E81£ ES push hl

E81F C5 push bc

E820 4E ld c,(h1)

-II1 155-

E821
E822
E823
E824
E825
E827
E828
E829
E82A
E82B
E82C
E82E
E82F
E830
E831
E832
E833
E834
E837
E838

E83A
E83D
E840
E841
E842
E843
ES
E845
E846
E847
E848
E849
E84A
ES84B
E84D
ES84E
E84F
E850
E851

23
46
78
B1
2813
2B
09
7E
23
B6
280€
2B
C1
ES
EB
09
EB
DASSE7
EI
185

0164E8
CDFFE8
C1

E1

D1

C5

ES

LE

23

46

23

78

B1
280€
73

23

72

23

E1

inc
ld
ld
or
jr
dec
add
ld
inc
or
jr
dec
pop
push
ex
add
ex
jp
POP
jr

ld
call
pop
pop
pop
push
push
ld
inc
ld
inc
id
or
Jr
1d
inc
ld
inc
pop

hl
b,(h1)
a,b

C
Z,E83À
hl
hi,bc
a, (hl)
h]
(hl)
Z,E83A
hl

bc

hl
de,hl
hl,bc
de,hl
c,E755
hl
E81F

bc,E86
E8FF
bc

hl

de

bc

hl
c,(h1l)
hi
b,(h1)
hl

a,b

C
Z,E859
(hl).e
hl
(h1),d
hl

hl

BASIC 1,0

‘Improper argument”

mn

-111 156-

E852
E853
E854
E855
E856
E857

£859
E85A
E85B
E85E
E861

E864

E867
E869
E86A
E86C
E86E
E86F
E870
E871
E872
E875
E877
E878
E879
E87A
E87B
E87C
E87D
E87E
E87F
E881
E882
E885
E886

E888

09
C1
EB
09
EB
18EA

ET
E1
0188E8
CDFFE8
C364C0

CD43E9

FEO2
D8
FETE
20F6
E5
56
2B
SE
CDA3E7
3O00E
2B
EB
E1
E5
72
2B
73
2B
3E1D
77
323ARE
ET
18DC

CD43E9

add
pop
ex
add
ex
Jr

pop
pop
ld
call
JP

call

cp
ret
cp
jr
push
ld
dec
ld
call
jr
dec
ex
pop
push
ld
dec
1d
dec
ld
id
ld
pop
jr

call

BASIC 1.0

hl,bc
bc
de,hl
hl,bc
de,hl
E843

h1

hl

bc,E888

E8FF

C064 au mode READY

E943 aller chercher prochain élément de
la ligne
02
C
1E ‘numéro de ligne’
nz,E864
hl
d,(h1)
hl
e,(hl)
E7A3 chercher ligne Basic
nc,E885
hl
de,hl
hl
hi
(h1),d
hl
(hl),e
hl
a,1D ’adresse de ligne’
(hl),a
(AE3A),a
hl
E864

E943 aller chercher prochain élément de

-[11 157-

E88B
E88D
E88E
E890
E892
E893
E894
E895
E896
E899
E89C
E89D

E89F
E8A1
E8A2

E8A5
E8A6
E8A7
E8A9
E8AB
E8AD
E8AF
E8B0

E8B2

E8B5
E8B7
E8B9
E8BA
E8BC
E8BF
E8C0

FEO2
D8
FEIE
20F6
E5

56

2B

5E
CDD6DD
CD18CB
E1
1809

0601
2B
CD43E9

B7
C8
FE01
2807
FEAT
20F3
04
18F0

CD43E9

FE97
20EC
05
20E6
CD3FDD
O4

C9

Cp
ret
cp
Jr
push
ld
dec
ld
call
call
pop
jr

Id
dec
cal]

or
ret
CP
Jr
CP
Jr
inc
jr

call

cp
jr
dec
jr
call
inc
ret

BASIC

02

(où

1E
nz,E888
hl
d,(h1)
hl
e,(h1)
DDD6
CB18
hl
E888

b,01
hl
E943

a
Z

01
z,E8B2
Aî

nz, E8A2
()

E8A2

E943

97
nz,E8A5
D

nz, E8A2
DD3F

b

UNE DEN DEDE DEEE DEEE DE DEEE DE DE DEEE DE DE DE JE NE

E8C1
E8C2

JE
FESB

id
CP

a, (h1)
5B

1.0

la ligne

‘numéro de ligne”

numéro de ligne dans hl
’Undefined line in’

aller chercher prochain élément de
la ligne

"IF"

aller chercher prochain élément de
la ligne
"ELSE"

ignorer les espaces

tester si variable indicée

‘ET!

-111 158-

BASIC 1.0

E8C4 2803 Jr z,E8C9

E8C6 FE28 CP 28 fe

E8C8 C0 ret nz pas d’index

E8C9 0600 ld b,00

E8CB O4 inc b augmenter imbrication de parenthèses

E8CC CD43E9 call E943 aller chercher prochain élément de
la ligne

E8CF FESB CP 5B 'ET'

E8D1 28F8 Jr z,E8CB

E8D3 FE28 Cp 28 °C

E8D5 28F4 Jr z,E8CB

E8D/7 FESD Cp 5D "EU

E8D9 280A jr Z,E8E5

E8DB FE29 CP 29 ")'

E8DD 2806 Jr Z,E8E5

E8DF FEO2 cp 02

E8E1 3807 jr c,E8EA ’Syntax error’

E8E3 187 Jr E8CC

E8ES 05 dec o] diminuer imbrication

E8E6 20E4 jr nz, E8CC encore parenthèses isolées ?

E8E8 23 inc hl hl pointé maintenant sur après index

E8E9 C9 ret

E8EA 1E02 1d e,02 ’Syntax error’

E8EC C394CA jp CA94 sortir message d'erreur

AOHOHEOEHEHEOHOHEOMEOMDE EU HEHE DE DE DE EME DE HEDEOH EME JE GE instruction Basic DATA

ESEF 0601 id b,01 1, fin de l'instruction

E8F1 1802 ir E8F5

LÉLESLSLSSLLLE LE LS SSL LL LLLLLL LL TS. instruction Basice ELSE, REM et 4

E8F3 0600 id b, 00 0, fin de la ligne

E8F5 2B dec hi

E8F6 CD43E9 call E943 aller chercher prochain élément de
la ligne

E8F9 B7 or a

E8FA C8 ret Z

E8FB B8 CP D atteint marque de fin ?

E8FC 20F8 jr nz,E8F6 non

-[11 159-

BASIC 1.0

E8FE C9 ret

E8FF CDD2DD call DDD2 adresse de ligne actuelle dans h]
E902 ES push hl

E903 2A81AE ld h1, (AE81) début du programme

E906 23 inc hl

E907 7E ld a, (hl)

E908 23 inc hi

E909 B6 or (h1l)

E9OA 2813 Jr Z,E91F

E90C 23 inc hi

E90D CDCEDD call DDCE fixer adresse de ligne actuelle
E910 23 inc hl

E911 C5 push bc

E912 CDF9FF call FFF9 Jp (bc)

E915 C1 pop bc

E916 2B dec hl

E917 CD35E9 call E935

E91A B7 or a

E91B 20F4 Jr nz,E911

E91D 18E7 Jr E906

E91F E1 pop hl

E920 C3CEDD JP DDCE fixer adresse de ligne actuelle
E923 CD35E9 call E935

E926 B7 or a

E927 CO ret nz

E928 23 inc hl

E929 7E 1d a,(hl)

E92A 23 inc hl

E92B B6 or (h1)

E92C 59 ld e,C

E92D CA9UCA Jp Z, CA94 sortir message d'erreur
E930 23 inc hl

E931 54 ld d,h

E932 SD ld e,l

E933 23 inc hl

E934 C9 ret

-I11 160-

BASIC 1.0

ONODHOHON DEN DE DEEE DE DEEE DE DE DE DE MEME ME DE DE EME DEEE DE JE

E935 CD43ES call E943 aller chercher prochain élément de
la ligne

E938 FEO2 cp 02 fin de ligne ?

E93A D8 ret C

E93B FE97 cp 97 ELSE"’

E93D C8 ret Z

E93E FEEB CP EB THEN’

E940  20F3 Jr nz,E935

E942 C9 ret

CTELLLLLLLLLLLLLLLLLLLESRLILLLLSLÉES SZ: aller chercher prochain élément de
la ligne

E943 CD3FDD call DD3F ignorer les espaces

E946 C8 ret Z

E947 FECOE CP 0E

E949 381D Jr c,E968

E94B FE20 cp 20 5

E94D 3829 jr c,E978

E9UF FE22 cp 22 A

E951 2809 Jr Z,E95C ignorer chaîne

E953 FE7C cp 7C "EBIEA'

E955 2819 jr Z,E970

E957 FEFF cp FF fonction ?

E959 C0 ret nz

E95A 23 inc hl

E95B C9 ret

E95C 23 inc h]

E95D 7/E id a, (hl)

E9SE FE22 cp 22 DE

E960 C8 ret Z

E961 B7 or a

E962 20F8 Jr nz,E95C

E964 2B dec hl

E965 3E22 ld a, 22 ni

E967 C9 ret

E968 FEO8 cp 08

E96A C8 ret Z

-111 161-

E96B
E96D
E96E
E96F
E970
E971
E972
E973
E974
E976
E977

E978
E97A
E97B
E97D
E97F
E981
E983
E984
E985
E986
E987
E988

E989
E98A
E98B
E98C
E98F
E992
E993
E994
E995

FE07
C8
23
23
F5
23
JE
17
30FB
F1
C9

FE18
D8
FE19
2808
FE1F
3803
23
23
23
23
23
C9

C5
D5
E5
0196E9
CDFFE8
Eî
Di
C1
C9

Cp
ret
inc
inc
push
inc
1d
rla
Jr
pop
ret

Cp
ret
CP
jr
CP
Jr
inc
inc
inc
inc
inc
ret

push
push
push
id
call
pop
POP
POP
ret

BASIC 1.0

07

Z

hl

hl

af

hl
a,(hl)

nc,E971
af

18 constante de chiffres ?

C

19 nombre sur un octet ?

z,E987

1F nombre à virgule flottante ?
c,E986 non, nombre sur deux octets

hl

hl ignorer nombre correspondant

hl d'octets

hl

hl

bc

de

hl
bc,E996
ES8FF

hi

de

bc

LES LL SES ES LE SELS SSL SSL ESS LL LES ES)

E996

E997  CD43E9

E99A

E5

D1

push
cal]

pop

hl
E943 aller chercher prochain élément de
la ligne
de

-[11 162-

BASIC

E99B FEO2 CP 02

E99D D8 ret C

E9SŒ  FEOE Cp 0E

ESAO 30F4 Jr nc, E996
E9A2 FEO7 cp 07

ESA4 28F0 jr Z,E996
E9A6 FEO8 cp 08

E9A8 28EC Jr Z,E996
E9SAA EB ex de,hl
E9AB CD3FDD call DD3F
E9AE FEOD cp 0D

E9BO 3802 Jr c,E9B4
E9B2 360D ld (h1),0D
E9B4 23 inc hl

E9BS 3600 ld (h1),00
E9B7 23 inc hl

E9B8 3600 id (h1),00
ESBA EB ex de,hl
E9BB 18D9 Jr E996

LRLLS ES LE EL LL LL LL I LLEL LL LL SELLES]

E9BD CD51DD call DD51

E9CO EB ex de,hl
E9CT 2A81AE 1d hl, (AE81)
E9C4 EB ex de,h1
E9C5 381C jr c,E9E3
E9C7 FEIE CP 1E

E9C9 2815 Jr Z,E9E0
E9CB FE1D cp 1)

E9CD 2811 Jr Z,E9E0

E9CF  CDODEA call EAOD
E9D2 2130FA ld hl,EA30
E9D5 D213BD JP nc,BD13
E9D8 CDASEB call EBA8

ESDB Z2A81AE ld hl, (AE81)
E9DE 1811 jr E9F1

ESEO CD6/E7 call E767
ESS D5 push de
ESE4 CDADD2 call D2AD

1.0

fin de la ligne ?
oui

ignorer les espaces

instruction Basic RUN
fin de l'instruction ?

début de programme comme défaut

oui, fin de l'instruction
numéro de ligne ?

adresse de ligne ?

charger programme à partir de K7
MC BOOT PROGRAM

tester type de fichier

début du programme

aller chercher adresse de ligne

interrompre [1/0 cassette

-I11 163-

ES%7
ESŒÆA
ESED
E9F0
E9F1
E9F2
E9F3

CD8CC1
CD7ACI
CDSEC1
El
23
F1
C393DD

cal]
call
call
pop
inc
pop
Jp

BASIC

C18C
C17A
C15E
hl
hl
af
DD93

LESSR ELLES SL RR SELS ESS SSL ELLL LL LES]

E9F6
E9F9
E9FB
E9FE

EAO1
EAO2
EAOS
EAO8
EAOB
EAOC

EAOD
EA10
EA12
EAT4
EAT6

EA19
EAIC
EATF
EA20

EA21
EA24
EA27
EA2B

EA2E
EA2F

CDODEA
3006

CDA8EB
C364C0

E5
CDO1F5
CD30EA
CA6BCB
Eî
C9

CD8FEB
EGOE
EE02
280B
CD4ADD

CD8CC1
CD6BC1
37
C9

CD55DD
DC91CE
EDS33FAE
CDHADD

B7
C9

call
Jr
call]
Jp

push
call
call
Jp
pop
ret

call
and
xor
Jr
call

call
call
scf
ret

cal]

call

ld
call

or
ret

EAOD
nc, EA01
EBA8
CO64

hl
F501
EA30
z,CB6B
hl

EB8F

0E

02

Z,EA21
DD4A

C18C
C16B

DD55
c,CE91
CAE3F), de
DDAA

1.0

CLEAR

à la boucle de l'interpréteur

instruction Basic LOAD

au mode READY

tester si place en mémoire
charger programme

aller chercher nom, ouvrir fichier
type de fichier

fin de l'instruction, sinon ‘’Syntax
error’

virgule suit ?
oui, aller chercher valeur 16 bits
adresse de début

fin de l'instruction, sinon ’Syntax
error’

-111 164-

EA30
EA33
EA36
EA37
EA3A
EA5B

2A3FAE
CD83BC
ES
DC7ABC
EI
C9

ld
call
push
call
pop
ret

BASIC

h1, (AE3F)
BC83

hi

c,BC7A

hl

LLELLLEL LL LES EL LL LL LL LLLL ES ELLES ES)

EA3C
EA3E
EA4O
EAUS
EAU
EAUS
EA48
EAUB
EAUE
EA51
EA53
EA54
EA56
EA59
EASA
EASD
EASF
EA61
EAG6E
EA65
EAG68
EAGA
EAGB

EAGE
EA71
EA74
EA77
EA7A
EA7D
EA7E
EA7F

EEAB
2004
CD3FDD
37

SF
32U1AE
CD8FEB
110000
CD55DD
3006
7E
FE2C
C491CE
D5
CDS5DD
3E00
3009
CD37DD
92
CD37E7
3EFF
F5
CDAADD

CD1BFB
CD3EFC
CD8SES
CDD2D5
CD49F5
F1
C5
D5

xor
Jr
call
scf
sbc
ld
call

AB
nz,EAU4
DD3F

a,a
(AE41),a
EB8F

de, 0000
DD55
nc,EA59
a, (hl)
2C

nz, CE91
de

DD55
a,00

nc, EAG6A
DD37

92

E737
a,FF

af

DD4A

FB1B
FC3E
E989
D5D2
F549
af
bc
de

1,0

adresse de début
CAS IN DIRECT

CAS IN CLOSE

instruction Basic CHAIN
"MERGE

ignorer les espaces

flag pour MERGE

aller chercher nom, ouvrir fichier
valeur défaut zéro pour ligne début
virgule suit ?

non

aller chercher valeur 16 bits

ranger comme ligne de début
virgule suit ?

non

tester si encore un caractère
"DELETE”

vider zone de lignes

mettre flag pour DELETE

fin de l'instruction, sinon ‘Syntax
error’

Garbage Collection

supprimer fonctions FN

-I11 165-

BASIC 1.0

EA80 B7 or a DELETE ?

EA81 CASAE7 call nz,E75A oui, supprimer lignes

EA84 3AUTAE ld a, (AE41) flag pour MERGE

EA87 B7 or a

EA88 2008 Jr nz,EA92 oui, CHAIN MERGE

EA8A CD6BC1 cal] C16B supprimer variables

EA8D CDASEB cal] EBA8 examiner type de fichier

EA9O 1803 Jr EA95

EA92 CDS9DEB cal] EB9D tester type de fichier

EA9S D1 pop de longueur des variables

EA96 C1 pop pc longueur de la zone des chaînes

EA97 CD71F5 call F571 décaler les chaînes

EA9A D] pop de aller chercher ligne de début

EA9B 2A81AE ld h1, CAE81) début du programme comme défaut

EASŒE 7A ld a,d

EA9F B3 or e pas ligne de début

EAAO C8 ret Z

EAAT CD9AE7 call E79A chercher ligne Basic

EAAH 2B dec hl

EAAS C9 ret

DEMHDEDENEDE DE DEN DE HD DH DE DE DE MEME DE DE NE DE DE DE HE DEEE DE instruction Basic MERGE

EAAG CDS8FEB call EB8F aller chercher nom, ouvrir fichier

EAA9Q  CD4ADD call DD4A fin de l'instruction, sinon ‘Syntax
error’

EAAC CD8CC1 call C18C supprimer les variables

EAAF  CD9IDEB call EB9D tester type de fichier

EAB2 C364C0 JP C064 au mode READY

LÉ LES SE SELLES SL LL LL IL IL LLLLLZ)

EAB5 CD7ACI call C17A
EAB8 CD8/E6 call E687

EABB 2A83AE ld hl, (AE83) fin du programme
EABE EB ex de,h]l

EABF 2A81AE ld hi, (AE81) début du programme
EAC2 23 inc hl

EAC3 2283AE 1d (AE83),hl fin du programme
EAC6G EB ex de,hl

EAC7  CDDAFF call FFDA bc := hl - de

-111 166-

EACA
EACB
EACE
EACF
EADO
EAD3
EAD4
EAD5
EAD6
EAD9
EADC
EADD
EADE
EADF
EAE2
EAE4
EAE7
EAE8
EAEA
EAEB
EAEE
EAEF
EAFO
EAF1
EAF2
EAF4
EAFS
EAF6
EAF7
EAF8
EAF9
EAFC
EAFD
EAFF
EBO1
EBO4

EB06
EBO7
EB08

EB
2A8DB0
EB

2B
CDFSFF
13

EB

E5
2A83AE
112000
19

EB

E1
CDB8FF
3850
CD84EB
B3
2830
D5
CD84EB
ES

7E

23

B6
2812
23

7E

23

66

6F
CDB8FF
E1
280F
3006
CDASEB
18E8

ET
E3
CDSEEB

ex
ld
ex
dec
call
inc
ex
push
ld
ld
add
ex
pop
call
Jr
cali
or
Jr
push
call
push
id
inc
or
jr
inc
1d
inc
ld
id
cal]
pop
jr
Jr
call
jr

pop
ex
call

BASIC 1,0

de,hl

h1, (BO8D) début des chaînes
de,hl

hl

FFF5 lddr

de

de,hl

hl

nl, (AE83) fin du programme
de, 0020

h!,de

de,h]l

hl

FFB8 comparer hl <> de
c,.EB34 ‘Memory full’
EB84

e

Z,EB1A

de

.EB84

hl

a, (h1l)
hl

(nl)
Z,EB06
hl
a,(hl)
h]

h, (hl)
1,a
FFB8 comparer hl <> de
hl

Z, EBOE
nc, EBO7
EB48
EAEE

hl

(sp),hl
EBSE

-III 167-

EBOB
EBOC

EBOE
EBOF
EB12
EB13
EB14
EB15
EB16
EB17
EB18

EB1A
EB1B
EB1C
EB1D
EBIE
EB20
EB23

EB25
EB28
EB2A
EB2B
EB2D
EB2E
EB31

EB54
EB36

EB38
EB3A
EB3B
EB3E
EB41
EB44
EB4S

E1
18C7

E3
CDSEEB
E1

5€

23

56

2B

19
18BB

7E

23

B6

2B
2805
CD4SEB
18F5

2A83AE
3600
23
3600
23
2283AE
C3B1D5

1E07
1802

1E18
D5
CDADD2
CD8CC1
CD6BC1
D1
C394CA

POP
jr

ex
call
pop
ld
inc
ld
dec
add
jr

id
inc
or
dec
Jr
call
Jr

ld
1d
inc
ld
inc
ld
Jp

id
Jr

ld
push
call
call
call
pop
Jp

hl
EADS

(sp),h
EBSE
hl
e,(h1)
hi
d,(h1)
hl
hl,de
EADS

a, (h1)
hl
(h1)
hl
Z,EB25
EB48
EB1A

h1, (AE
(h1),0
hl

BASIC 1,0

1

83) fin du programme
    0

(h1),00

hl
CAE83)
D5B1

e,07
EB3A

e,18
de

D2AD
c18C
C16B
de

CA94

sh fin du programme

‘Memory full’

"EOF met

interrompre 1/0 cassette

sortir message d'erreur

-[11 168-

EB48
EB49
EBUA
EB4B
EB4C
EB4D
EBLE
EB51
EB52
EB53
EB56
EB57
EB5A
EB5B
EB5C
EBS5SD

EB5SE
EB5F
EB60
EB63
EB64
EB65
EB66
EB67
EB68
EB69
EB6A
EB6B
EB6C
EB6D
EB6E
EB6F
EB70
EB71
EB72
EB73
EB/4
EB75
EB77

C5
D5
E5
LE
23
46
2A83AE
EB
E1
CDF2FF
EB
2283AE
EB
D1
C1
C9

D5
EB
2A83AE
75
23
72
23
EB
E3
EB
73
23
72
23
Di
1B
1B
1B
1B
7A
B3
2809
CD80BC

push
push
push
ld
inc
ld
ld
ex
pop
call
ex
ld
ex
pop
pop
ret

push
ex
ld
id
inc
ld
inc
ex
ex
ex
ld
inc
ld
inc
pop
dec
dec
dec
dec
id
or
jr
call

BASIC 1,0

bc

de

hl

c;(hl)

hl

b,(h1l)

h1, (AE83) fin du programme
de,hl

h1

FFF2 ldir

de,hl

(AE83),h1l fin du programme
de,hl

de

bc

de
de,hl
h1, (AE83) fin du programme
(hl),e
hl
(h1),d
hl
de,hl
(sp),hl
de,hl
(hl)e
hl
(hl),d
hl

de

de

de

de

de

a,d

e

Z, EB80
BC80 CAS IN CHAR

-I11 169-

EB7A
EB/C
EB7D
EB7E

EB80
EB83

EB84
EB87
EB88
EB8B
EB8D
EB8E

EB8F
EB92
EB95
EB98
EB9C

EB9D
EBAO
EBAI
EBA4
EBAG
EBA8
EBAB
EBAD
EBAF
EBB1
EBB3
EBB5

EBB8
EBBB
EBBE
EBBF
EBCO
EBC3

30BC
77
23
18F2

2283AE
C9

CD80BC
5F
DC80BC
30AB
57

C9

CDADD2
CD6AD2
32U42AE

ED43USAE
cg

3AU2AE
B/
CABSEA
FET6
200B
3AU2AE
FE16
2840
EG6FE
2805
1E19
C394CA

CD7/AC1
2A81AE
23
EB
2A8DB0
0180FF

jr
ld
inc
jr

ld
ret

call
Id
call
jr -
ld
ret

call
call
id

1d

ret

ld
or
Jp
CP
jr
ld
CP
Jr
and
Jr
ld
Jp

call
ld
inc
ex
Id
ld

BASIC 1.0
nc,EB38 "E0F met’
(hl),a
hl
EB72
(AE83),h1 fin du programme
BC80 CAS IN CHAR
e,a
c,BC80 CAS IN CHAR
nc,EB38 E0F met’
d,a
D2AD interrompre 1/0 cassette
D26A aller chercher nom, ouvrir fichier
(CAEU42),a ranger type de fichier
(AE43),bc ranger longueur de fichier
a, (AE42) type de fichier
a
Z,EAB5
16 fichier ASCII ?
nz,EBB3 File type error’
a, (AE42) type de fichier
16 fichier ASCII ?
Z,EBEF
FE annuler bit O0 (fichier protégé)
Z, EBB8
e,19 ‘File type error’
CA94 sortir message d'erreur
C17A
hl, CAE81) début du programme
h1
de,hl
h1, (BO8D) début des chaînes
bc,FF80

-111 170-

EBC6
EBC7
EBCB
EBCE
EBDI
EBD4
EBD5
EBD6
EBD7
EBDA
EBDD
EBDE
EBDF
EBE2
EBE3
EBE6
EBE9
EBEC

EBEF
EBF2
EBFS5

EBF8
EBFB
EBFE
ECO0
ECO2
ECO4
ECO6

09
EDABU3AE
CDCFFF
DUBEFF
DASUEB
60

69

19
2283AE
3AU2AE
1F

9F
32USAE
EB
CD83BC
CA38EB
CDB1D5
C398D2

CD7AC1
CDCBDD
CDACCA

D298D2
CDBCEG
38F5
1E15
2802
1E06
C394CA

add
ld
cal]
call
jp
1d
ld
add
ld
1d
rra
sbc
id
ex
call
jp
call
jp

call
call
cal]

jp
call
jr
ld
jr
ld
jp

BASIC

hl,bc

bc, (AE43)
FFCF

nc, FFBE
c,EB34
h,b

15C

hl,de
(AE83),h]
a, (AE42)

a, à
(AE45),a
de,hl
BC83
z,EB38
D5B1
D298

C17A
DDCB
CAUC

nc,D298
E6BC
c,EBF5
e,15
Z,EC06
e,06
CA94

HONHOHEHEHEHE EH HE HOME EME DEHEHEME DE HE MEME OH DE DEEE HE

ECO9
ECOC
ECOF
EC11
EC14
EC16
EC19
ECTA

CDADD2
CD56D2
0600
CD55DD
3029
CD3/DD
0D

23

call
cal]
ld
call
jr
call
db
inc

D2AD
D256
b,00
DD55
nc,.EC3F
DD37

OD

hl

1.0

longueur du fichier
hl := hl - de
comparer hl <> bc
‘Memory full”

fin du programme
type de fichier
protégé ?

fixer flag pour programme protégé

CAS IN DIRECT
"EOF met’

CLOSEIN

adresse de ligne actuelle sur zéro
ligne de cassette dans buffer

d'entrée

CLOSEIN

convertir ligne en code interpréteur

pas instruction directe ?

‘Direct command found’

"Overflow'
sortir message d'erreur

instruction Basic SAVE
interrompre 1/0 cassette

OPENOUT

type de fichier 0, programme Basic
tester si virgule

tester si encore un caractère
‘variable numérique’ (A,B,P)

-111 171-

BASIC

EC1B 23 inc hl

EC1C 7E Id a,(h1)
ECID 23 inc hl

EC1E E6DF and DF

EC20 F238EC JP p,EC38
EC23 E5 push hl

EC24 212CEC ld hl1,EC2C
EC27 CD93FF call FF93
EC2A E3 ex (sp},hl
EC2B C9 ret

ADN DE DE DD DE D DE DE DE DD DE DE NÉ DE HE DEEE EE
EC2C 03 db 03

EC2D 38C dw EC38
EC2F C1 db C1

EC30 87EC dw EC87
EC32 C2 db C2

EC33 SCEC dn ECES
EC35 DO db DO

EC36 3DEC dw EC3D
DOME DE DE DE DD DEEE DE DEEE DE DE DE DE EE DEN EEE NE
EC38 1E02 Id e,02
EC3A C394CA jp CA94
DENON DE DE DEEE HE DEEE DEEE DE DEEE ÉD
EC3D 0601 id b,01
EC3F  CDUADD call DD4A
EC42 ES push hl

EC43 C5 push bc

EC4u4 CD87E6 call E687
EC47  CD89,E9 call E989
ECUHA 2A81AE id hl, (AE81)
EC4D 23 inc hi

ECHE EB ex de,hl
ECUF 2A83AE ld h1, (AE83)
EC52 CDCFFF call FFCF
EC55 EB ex de,hl
EC56 F1 pop af

1.0

nom de variable

convertir minuscules en majuscules
’Syntax error’

adresse de base de la table
parcourir la table

nombre d'entrées dans la table
adresse de retour si pas trouvé
FU

'B'

tp!

"Syntax error’
sortir message d'erreur

SAVE ,P

type de fichier 1, protected

fin de l'instruction, sinon ’Syntax
error’

début du programme

fin du programme
hl := hl - de

-111 172-

BASIC

EC57 010000 ld bc,0000
ECSA 1823 Jr EC7F
HOMME DEEE DE ME DE DEDE MEME EEE DEN DE DE HE DE DE DE DE DE DE DE DEN
EC5C 0602 ld b,02
ECSE CD37DD call DD37
EC61 2C db 2C

EC62 CD91CE call CE91
EC65 D5 push de

EC66 CD37DD call DD37
EC69 2C db 2C

EC6A  CD91CE call CE91
EC6D D5 push de

EC6E CDS55DD call DD55
EC71 110000 ld de, 0000
EC74 DC91CE cal] c,CE91
EC77 D5 push de

EC/78 CD4ADD call DD4HA
EC7B 78 ld a,b
EC7C C1 pop bc

EC7D D1 pop de

EC7E E3 ex (sp),hl
EC/F  CD98BC call BC98
EC82 D26BCB Jp nc, CB6B
EC85 1817 Jr ECSE
LELLLLESSLELSLLSLLLLSLLLLLLLLLLSES]
EC87  CD4ADD call DD4A
EC8A ES push hl

EC8B 3E09 ld a,09
EC8D CDA2C1 call C142
EC90 F5 push af

EC91 010100 ld bc, 0001
EC94 11FFFF ld de, FFFF
EC97 CDODE1 call E10D

1.0

SAVE ,B
type de fichier 2, binaire
tester si encore un caractère

11
,

aller chercher valeur 16 bits,
adresse de début

tester si encore un caractère
aller chercher valeur 16 bits,
adresse de fin

virgule suit ?
défaut zéro

oui, aller chercher valeur 16 bits,
adresse d'entrée

fin de l'instruction, sinon ‘’Syntax
error’
type de fichier
adresse d'entrée
adresse de fin
adresse de début
CAS OUT DIRECT
interruption par ‘ESC’ ?
CLOSEOUT

SAVE ,A
fin de l'instruction, sinon ‘’Syntax
error’

9

sortie sur canal 9, cassette

1
à 65535
lister lignes

-I11 173-

EC9A
EC9B
EC9E
ECAI
ECA2

ECA3
ECAG6
ECA8
ECAB

F1
CDA2C1
CDA1D2
ET
C9

CD4HUED
2005
CD61DD
182F

POP
call
call
POP
ret

call
jr
call
jr

af
C1A2
D2A1
hl

ED44
nz,ECA
DD61
ECDC

BASIC 1.0

sortie à nouveau sur défaut
CLOSEOUT

D
ignorer espace, TAB et LF

HER NEDEDE HD HEDE DD DE DE HE DH DE DEEE DE DEEE DE DE

ECAD
ECAF
ECB1
ECB4
ECB6
ECB9
ECBC
ECBD

ECBE
ECBF
ECC2
ECC3
ECC4
ECC5

ECC6
ECC8
ECC9
ECCB
ECCD
ECDO
ECD1
ECD2
ECD5
ECD6
ECD/
ECD8

FE26
281C
CD/FFF
3826
CD10FF
CDF3FE
37

C9

ES
CDCGEEC
D1
D8
EB
C9

1600
7E
FE26
200F
CDICEE
EB

F5
CDODFF
Fi

EB

D8

C8

CP
Jr
call
Jr
call
call
scf
ret

push
call
pop
ret
ex
ret

1d
Id
Ep
jr
call
ex
push
call
Pop
ex
ret
ret

26
Z,ECCD
FF7F
c,ECDC
FF10
FEF3

hl
ECC6
de

de,hl

d,00
a, (h1l)
26

nz, ECD
EE1C
de,hl
af
FFOD
af
de,h]
C

Z

1g'

tester si numérique

type sur entier
supprimer variable

1g'
C

accepter nombre entier hl

-I11 174-

ECD9

ECDC
ECDD
ECDE
ECDF
ECET
ECE4
ECE7
ECES8
ECEA
ECEB
ECED
ECEE
ECEF

ECFO
ECF3
ECF4
ECF7
ECFA
ECFD
ECFF
EDO1
EDO4
EDO7
EDO8
EDOB
EDOC
DOD
DOF
D10

m

OS OC C0 © ©
Fr Er OR

OO M M Om I M M 1 m1 mm

Le]
3

M © © NM UE

C3F3CA

ES

7E

23
FE2E
CC61DD
CD83FF
ET
3806
7E
ÉE2E
CO

23

C9

CD1OFF
D5
010000
TIUGAE
CDS3ED
FE2E
200B
CDC9JED
CD19FF
0C
CD53ED
OD

F5
3EFF
12

F1
CD77ED
D1

2F

ES

D5
2TUGAE
CDCEED
D1

JP

push
1d
inc
Cp
call
call
pop
jr
1d
xor
ret
inc
ret

call
push
id
1d
call
cp
jr
call
call
inc
call
dec
push
1d
ld
POP
call
pop
id
push
push
1d
call
POP

CAF3

hl
a,(h1l)
hi

2E
z,DD61
FF83
h1
c,ECFO
a,(hl)
2E

nz

fl

FF10
de

‘bc,000

de, AE4
ED53
2E

nz, EDO
EDC9
FF19

C

ED53

C

af
a,FF
(de),a
af
ED/7
de

e,à

hi

de
hl,AE4
EDCE
de

BASIC 1.0

ignorer espace, TAB et LF
tester si chiffre
type sur entier
0
6
6
type sur ‘Real’
6

-111 175-

ED1F
ED22
ED24
ED25
ED26
ED29
ED2A
ED2C
ED2D
ED2E
ED2F

ED32
ED33
ED36
ED37

ED3A
ED3D
ED3F
ED40
EDH1

ED44
ED47
ED48
ED4A
ED4C
ED4D
ED4E
ED50
ED51
ED52

ED53
ED54
ED57
ED58
ED5B
ED5D

CD27FF
3008
ES

42
CDOGFE
E1
3811
7A

LE

23
CD94BD

7B
CDS5BD
EB
CD16FF

DC3DBD
3EOA
ET

D8
C3F3CA

CD61DD
25
16FF
FE2D
C8

14
FE2B
C8

2B

C9

ES
CD61DD
23
CD83FF
3804
E1

call
jr
push
1d
call
pop
jr
ld
ld
inc
call

ld

call

ex
call

call
Id
pop
ret
JP

call
inc
1d
cp
ret
inc
CP
ret
dec
ret

push
call
inc
call
Jr
pop

BASIC 1,0

FF27 tester si chaîne
nc,ED2C
hl
b,d
FE06
hl
c,ED3D
a,d
c,(hl)
hl
BD94 d'entier 4 octets*256 en nombre à
virgule flottante
a,e
BD55 multiplier nombre par 10°a
de,hl
FF16 fixer type de variable sur virgule
flottante
c,BD3D variable de (de) à (hi)
a,0A
hl
C
CAF3

DD61 ignorer espace, TAB et LF
hl

d,FF

2D 1!

zZ

d

2B 141

Z

hl

fl

DD61 ignorer espace, TAB et LF
hl

FF83 tester si chiffre

c,ED61

hl

-111 176-

EDSE

ED61
ED62
ED63
ED65
ED66
ED67
ED69
ED6A
ED6B
ED6D
ED6F
ED70
ED71
ED72
ED74
ED75

ED77
ED79
ED7B
ED7C
ED7F
ED82
ED85
ED88
ED8A
ED8B
ED8C

ED8E
ED8F
ED90
ED93
ED94
ED95
ED98
ED9A
ED9B

C38AFF

E3
EI
D630
12
BO
2807
78
Où
FEOC
3001
13
79
B7
28DF
OC
18DC

FE4S5
2010
ES
CDCSED
CD4HED
CC61DD
CD83FF
3804
ET

AF
181E

E3

E1
CD19FF
D5

C5
CD3SEE
3009
7B
D664

jp

ex
pop
sub
ld
or
jr
1d
inc
CP
jr
inc
1d
or
jr
inc
jr

Cp
Jr
push
call
call
call
call
jr
pop
xor
Jr

ex
POP
call
push
push
call
Jr
1d
sub

FF8A

(sp),h
hl

30
(de),a
b
Z,ED70
a,b

OC
nc,ED7
de

a,C

a
Z,ED53
C

ED53

45

nz, ED8
hl
EDC9
ED44
Z,DD61
FF83
c,ED8E
hl

a

EDAC

(sp},h
hl
FF19
de

bc
EE35

BASIC 1.0

convertir minuscules en majuscules
l

‘0°

0

B

ignorer espace, TAB et LF
tester si chiffre

l

fixer type sur ‘Real’

nc,EDA3

a,e
64

100

-[II 177-

ED9D
EDS
EDAO
EDA
EDA3
EDAS
EDA6
EDA7
EDA8
EDAA
EDAB
EDAC
EDAE
DAF
DBO
DB2
DB4
DB5
DB6
EDB8
EDB9
EDBB
EDBC
EDBE
EDCO

mm mm m mm

EDC1
EDC2
EDC4
EDC6
EDC8

EDC9
EDCC
EDCD

EDCE
EDCF
EDD2
EDDS

7A
DEOO
7B
3802
3E7F
C1
D1
14
2002
2F
3C
C680
5F
78
D60C
3001
AF
g1
3009
85
3801
AF
FEO1
CE80
C9

83
3002
3EFF
D680
C9

CD61DD
23
cg

EB
2158AE
010105
2B

ld a,d

sbc a, 00

1d a,e

jr C,EDAS
id ä,7F
pop bc

pop de

inc d

jr nz, EDAC
cpl a

inc a

agd a, 80

ld e,a

Id a,b

SUD OC

jr nc, EDBS
xor a

SUD C

jr nc,EDC1
add a,e

Jr c,EDBC
xor a

CP 01

adc a, 80
ret

add a,e

Jr nc,EDC6
ld a, FF
SUD 80

ret

call DD61 ignorer espace, TAB et LF
inc hl

ret

ex de,hl
id hl,AES8
Id bc, 0501
dec hl

-111 178-

EDD6
EDD8
EDDA
EDDB
EDDD
EDDE
EDDF
EDE2
EDE3
EDE4
EDE6
EDE7
EDE8
EDE9
EDEB
EDEC
EDED
EDEE
EDEF
EDFO
EDF1
EDF2
EDF3
EDF4
EDF5
EDF6
EDF7
EDF8
EDF9
EDFA
EDFC
EDFD
EDFE
EE00
EE01
EEO2

EEO4
EEOS
EE06

3600
10FB
1A
FEFF
C8
7
2153ÂE
13
1Â
FEFF
C8
D5
y
1600
E5
SE
62
6B
29
29
19
29
5F
19
5D
7C
ET
73
23
10EF
D1
B/
28DF
77
OC
18DB

C5
E5
CD35EE

id
djnz
ld
CP
ret
1d
id
inc
ld
cp
ret
push
1d
ld
push
1d
ld
ld
add
add
add
add
ld
add
id
ld
pop
1d
inc
dJnz
pop
or
jr
ld
inc
jr

push
push
call

BASIC 1,0

(h1),00
EDD5

a, (de)
FF

Z
(h1),a
h1,AE53
de

a, (de)
FF

Z

de

b,c
d,00

hl
e,(h1)
h,d

l,e
hLh]
hl,hl
hl,de fois 10
hl,hl
e,a plus chiffre suivant
hl,de
e,l

a,h

hl
(hl).e
hl

EDEB

de

a

Z, EDDF
(hl),a
C

EDDF

bc

hl
EE35

-111 179-

EEO9
EEOA
EEOD
EEOE
EEOF
EE11
EE12
EE13
ÉE15
EE17
EE18
EE19
EETA
EE1B

EE1C
EE1D
EE20
EE23
EE25
EE27
EE29
EE2B
EE2D
EE2F
EE30
EE33

EE35
EE37
EE38
EE3B
EE3D
EE3E
EE4O
EE42
EE4S
EE47
EE48
EEUA

EB
CDODFF
EB
C1
3006
7A
B3
C6FF
3803
50
59
EB
C1

C

23
CD61DD
CD8AFF
0602
FE58
2806
0610
FE48
2004
23
CD61DD
1802

O60A
EB
CD61EE
2600
6F
301E
0E00
CD6IEE
3014
D5
1600
5F

ex
call
ex
pop
Jr
ld
or
add
Jr
Id
1d
ex
POP

ret

inc
call
call
ld
cp
jr
1d
CP
jr
inc
call
jr

id
ex
call
Id
ld
jr
1d
cal]
Jr
push
ld
Id

BASIC 1.0

de,hl
FFOD accepter nombre entier hl
de,hl
bc
nc,EE17
a,d

e

a,FF
C,EE1A
d,b

e,C
de,hi
bc

hl

DD61 ignorer espace, TAB et LF
FF8A convertir minuscules en majuscules
b,02 base 2, binaire

58 Ex

Z,EE2F

b,10 base 16, hex

48 "H'

nz,EE33

hl

DD61 ignorer espace, TAB et LF
EE37

b,0A base 10, décimal

de,hl

EE61 convertir chiffre hexa en binaire
h, 00

],a

nc, EESE

c, 00

EE61 convertir chiffre hexa en binaire
nc,EESB

de

d,00

e,a

-I111 180-

BASIC 1.0

EE4B DS push de

EE4C 58 id e,b base du système numérique

EE4D CDBEBD call BDBE multiplication entiers sans signe
EE50 D1 pop de

EES1 3803 Jr c,EE56

EE53 19 add hl,de

EE54 3002 Jr nc, EE58

EE56 OEFF ld c,FF

EE5S8 D1 pop de

EE59 18E7 Jr EEU2

EESB 79 ld a,C

EESC FE01 cp 01

EESE EB ex de,hl

EESF 78 ld a,b

EE60 C9 ret

LELELLELILLLSLLLLLILLSLLSLSLLLLSESS convertie chiffre hexa en binaire
EE61 14 ld a, (de) aller chercher caractère

EE62 13 inc de

EE63 CD83FF call FF83 tester si chiffre

EE66 380A Jr c,EE72 oui

EE68 CD8AFF call FF8A convertir minuscules en majuscules
EE6B FE4l cp 41 "A

EE6D 3F ccf

EE6GE 3005 jr nc, EE75 inférieur ‘A’, erreur

EE70 D607 sub 07 "A'-("9'+1)

EE72 D630 sub 30 0"

EE74 B8 cp D

EE75S D8 ret C pas erreur ?

EE76 1B dec de

EE77 AF xor a annuler carry

EE78 C9 ret

HOHONONOHOHEODEONOMOMEDEME ME DEEE DE DE DE ED DE DE DEEE DEN Ne sortir numéro de ligne

EE79 CDODFF call FFOD accepter nombre entier dans hl
EE/7C CD82EE call EE82 convertir en représentation ASCII
EE7F C341C3 jp C341 sortir chaîne

HOMOHONE DE HE HEOHHE DM DEEE DEEE DE DE HE DEEE JE DE DE DE JE DEN convertir nombre entier en ASCII

-111 181-

EE82
EE83
EE84
EE87
EE88
EE8B
EE8C
EE8D
EE8E

D5
C5
CDC3FC
AF
CDA7EE
23
C1
D1
C9

push
push
call
xor
call
inc
pop
pop
ret

de
pc
FCC3

EEA7
hl
bc
de

BASIC 1.0

LÉRSSS LES SL ELLLLSLLLLLLLLLIL LL LLS]

EES8F
EE90
EE91
EE92
EE95
EE96
EE97
EE98
EE9A
EE9B
EE9C

D5
C5)
ÂF
CDSFEE
C1
D1
7E
FE20
CO
23
C9

push
push
Xor
call
Pop
pop
ld
CP
ret
inc
ret

de

bc

a
EE9F
bc

de

a, (h1l)
20

nz

hl

zéro
convertir nombre en chaîne formatée

15!

ECHEC HE DE DEEE EME ME DE DEEE HE HE EH HE DEEE HE DEEE HE HE

EE9D
EE9F
EEA2
EEA3
EEAG
EEA7
EEA8
EEA9
EEAA
EEAB
EEAE
EEBO
EEB3
EEB6
EEB7
EEBA

3E40
226EAE
F5
CDB3FC
F1

C5

57

D5

EB
2168AE
3600
2270ÂAE
CDB/F0
Di
CDDUEE
CD3DFO

ld
1d
push
call
POP
push
id
push
ex
id
id
ld
cal}
pop
cal]
call

a, 40
(AEGE )
af
FCB3
af

bc

d,a

de
de,hl

,hl convertir nombre en chaîne formatée

h1,AE68
(h1),00

(AE70)
FOB7
de
EED4
FO3D

hi

-I11 182-

EEBD
EEBE
EEBF
EECO
EECI
EEC4
EEC7
EECA
EECD
EECE
EECF
EEDO
EED1
EED3

EEDH
EED5
EED6
EED8
EEDB
EEDC
EEDD
EEDF
EEE2
EEE4
ÉBES
EEE6
EEE7
EEE9
EEEA
EEEB
EEEC
ÉEEF
EEF1
EEF2
EEF&
EEF6
EEF8
EEFA
EEFB

58

C1

7B

B7
CC50FO
CDSFFO
CD69F0
CD7CFO
7A

1F

DO

2B
3625
C9

7A

87
3029
FA27EF
7B

81
D6OA
FA88EF
1601
41

79

B7
2815
83

3D

5F
CDOEFO
0601
79
FE07
3804
CB72
2026
B8
CHAOEF

1d
pop
id
or
call
call
call
call
ld
rra
ret
dec
1d
ret

1d
add
Jr
jp
id
add
sub
jp
Id
id
id
or
jr
add
dec
ld
call
ld
1d
cp
jr
bit
jr
CP
call

BASIC 1.0

e,b

bc

a,e

a

Z, FO50
FOSF
F069
FO7C
a,d

nc
hi dépassement,
(h1),25 4%’ devant nombre formaté

a,d

a,a

nc, EFO01
m,EF27
a,e

a,C

OA 10
m,EF88
d,01
b,c

a,C

a
Z,EEFE
a,e

a

e,a
FOOE
b,01
a, C

07
C,EEFA
6,d
nz,EF20
b

nz, EFAO

-II1 183

EEFE

EFO1
EFO2
EFO3
EFO6
EFO8
EF09

EFOA
EFOB
EFOE
EFOF
EF10
EF12
EF13
EF14
EF15
EF16
EF17
EFTA
EF1D

EF20
EF22
EF25

EF27
EF29
EF2C
EF2E
EF31
EF32
EF33
EF36
EF38
EF39
EF3C
EF3D
EF3F

C362EF

7B

B/
FAOAEF
20DC
a

C9

43
CDOEFO
78

B7
28F6
93

58

47

81

85
FAEUEE
CDBUEF
C3ACEF

3E06
326EAE
1824

0680
CD25F0
3004
CD96F0
AF

47
CC36F0
200€
04
3AGEAE
B7
2805
05

jp

Id
or
jp
jr
ld
ret

ld
call
1d
or
jr
sub
1d
1d
add
add
JP
call
jp

Id
ld
jr

ld
call
jr
call
xor
ld
call
Jr
inc
ld
or
jr
dec

BASIC

EF62

a,€

a

m, EFOA
nz,EEE4
D,cC

b,e
FOOE
a,b

a
Z,EFO8
e
e,
D,
a

,

es Sx

a,€
m, EEE4
EFB4
EFAO

a, 06
(AËGE), a
EFUB

b, 80
F025
nc, EF32
F096

a

b,a
z,F036
nz,EF44
o)

a, (AE6E)
a

2, EF44
D

-I11 184-

.0

EF40
EF
EF44
EF45
EF46
EF48
EF49
EFHA
EF4B
EF4C
EF4D
EFUE
EF51
EF52
EF53
EF55
EF56
EF57
EF58
EF59
EF5A
EF5C
EFSE
EF9F
EF61
EF62
EF64
EF67
EF68

3C
326EAE
79

B7
2804
83

90

5F

78

F5

47
CD8BEF
F1

B8
280D
1C

23

05

E5

JE
FE2E
2001
23
3631
ET
3E45
CD6FFO
7B

87

inc
ld
ld
or
jr
add
sub
ld
ld
push
1d
call
pop
cp
jr
inc
inc
dec
push
ld
Cp
Jr
inc
Id
pop
ld
cal]
Id
add

BASIC 1.0

a
(AGE), a

a,C

a

z,EF4C

a,e

b

e,a

a,bD

af

b,a

EF8B

af

D

z,EF62

e

hl

b

hl

a,(hl)

2E Part
nz, EFSF

hl

(h1),31 Fe
hl

a,45 fE*
FO6F

a,e

a,a

-111 185-

EF69
EF6B
EF6D
EF6E
EF6F
EF70
EF72
EF75
EF76
EF78
EF79
EF7B
EF7D
EF7E
EF7F
EF82
EF83
EF85

EF88
EF8B
EF&E
EF8F
EF90
EF92
EF95

EF97
EF98
EF9B
EFSE
EF9F
EFAO
EF A2
EFA3
EFA4
EFAS
EFAG6
EFA7
EFA8

3E2B
3005
AF

93

5F
3E2D
CD6FFO
7B
0E2F
OC
D6OA
30FB
5F

79
CD6FFO
7B
C63A
C36FFO

CDBUEF
CD36F0
80

B9
3005
CDC8EF
1804

g1
CUEFEF
3AGEAE
B7

C8
0E2E
78

C5

47

O4

85

6F

8c

ld
ir
xor
sub
ld
ld
call
1d
Id
inc
sub
jr
Id -
1d
call
id
add
jp

call
call
add
CP
Jr
call
jr

sub
call
1d
or
ret
1d
id
push
ld
inc
add
ld
adc

a, 2B
nc,EF7
a

e

e,a

a, 2D
F06F
a,e
C;2F

C

OA

nc, EF7
e,a
a,C
F06F
a,e

a, 3
F06F

EFB4
F036
a,b

C

nc, EF9
EFC8
EF9B

C
nz,EFE
a, AEGE
a

Z

c,2E
a,D

C

à

,

,

b
D
()
a
Il
a

TT D —

,

BASIC 1.0

14!

2
10'-1
10
8
19'+1
7
F

1!
0

-111 185-

EFA9
EFAA
EFAB
EFAC
EFAD
EFAE
EFAF
EFBO
EFB2
EFB3

EFB4
EFB5
EFB6
EFB7
EFB8
EFB9
EFBA
EFBC
EFBD
EFBF
EFCO
EFC1
EFC3
EFC4
EFCS
EFC7

EFC8
EFC9
EFCA
EFCB
EFCC
EFCD
EFCE
EFCF
EFDO
EFD2
EFD5
EFD7

95
67
2B
79
LE
77
05
20F9
C1
cg

7B
81
47
FO
2F
3C
0614
B8
3001
47
2B
3630
OC
05
20F9
C9

ES

UF

85

6F

8c

g5

67

7E
3600
2270AE
FE35
DUEIEF

sub
ld
dec
id
ld
ld
dec
jr
POP
ret

ld
add
ld
ret
cpl
inc
ld
CP
jr
ld
dec
ld
inc
dec
jr
ret

push
Id
add
id
adc
SUD
ld
1d
1d
id
cp
call

BASIC 1.0

1

h,a

nl

a, C
c,(h1)
(h1),a
D
nz,EFAB
bc

S S
D OO

Ÿ

SO Où D 0:00:
N
_
EE

nc, EFCO

ba

hl

(h1),30 F0
C

D

nz,EFCO

a, (hl)

(h1),00
(CAE70),h1

35 157
nc,EFE1

-111 186-

EFDA
EFDB
EFDC
EFDD
EFDF
EFEO

EFE1
EFE2
EFE3
EFE4
EFES
EFE6
EFE7
EFE8
EFEA
EFEB
EFED

EFEF
EFFO
EFF1
EFF2
EFF3
EFF4
EFF5
EFF6
EFF7
EFF8
EFF9
EFFA
EFFB

EFFD
EFFE
EFFF
F000
F001
FO02
FOO4

E1
D8
2B
3631
Où
C9

79
B/
C8
2B
OD
JE
34
FE39
D8
3630
18F2

D5
C5
EB
47
7B
90
6F
9F
82
67
ES
OC
1804

A
13
77
23
OD
20F9
3630

pop
ret
dec
ld

inc
ret

ld
or
ret
dec
dec
ld
inc
CP
ret
1d
jr

push
push
ex
ld
1d
Sub
id
sbc
add
ld
push
inc
jr

ld
inc
1d
inc
dec
jr
Id

BASIC 1.0

h1
C
hl
(h1),31 if
b

a, C

a

zZ

hi

c

a,(hl)

(h1)

39 9’
c

(h1),30 "0
EFE1

de
bc

hl

nz, EFFD
(h1),30 '0'

— 111 187-

F006
F007
F008
FOOA
FO0B
FOOC
FO0D

FOCE
FOOF
F012
F013
F014
F015
F017
F019
FOTA
FO1B
F01C
FOIE
F020
F023
FO24

F025
F028
F029
FO2A
F02B
FO2C
FO2E
F030
F031
FO34
F035

F036
F039
FO3A
FO3B

23
05
20FA
E1
C1
D1
C9

E5
2A7OAE
2B

7E

23
FE30
2005
2B

OD

Où
20F4
3600
2270AE
E1

C9

CD9BFO
9F

3C

47

7A
E604
2801
04
3AGFAE
90

cg

3AGEAE
B7
C8
5D

inc
dec
Jr

pop
pop
pop
ret

push
Id
dec
ld
inc
CP
Jr
dec
dec
inc
Jr
id
ld
pop
ret

call
sbc
inc
ld
ld
and
jr
inc
ld
sub
ret

1d
or
ret
dec

BASIC

hl

nz,FO04
hl
bc
de

hi

h1, (AE70)
hi

a, (hl)

h]

30

nz, FOIE
hl

C

D

nz, F012
(h1),00
(AE70),h1
hl

FO9B
8,4

É]

b,a

a,d

O4

z, F031

b

a, (AE6F)
b

a, (AEGE)
a
z
a

1.0

-I11 188-

FO3C

FO3D
FO3E
FO40
F041
FO42
FO4L
FO45
FO46
FO47
FO49
FO4C
FO4D
FOUE

F050
F051
F052
FO54
FO55
FO58
F059
FOSA
FOSB
FO5D

FO5F
F060
F062
F063
F065
F066
F067
F068

F069
FO6C
FO6D
F06F

cg

7A
E602
C8
78
D603
D8
C8
(ED)
OE2C
CDAZEF
(ep)
F1
18F2

7A

87
3007
C5
CD25F0
C1

D8

C8
3E30
1806

7A
E604
C8
3E24
1C
2B
77
cg

CD9BFO
C8
30F6
ES

ret

ld
and
ret
ld
suD
ret
ret
push
id
call
inc
pop
jr

ld
add
Jr
push
call
pop
ret
ret
1d
jr

1d
and
ret
id
inc
dec
ld
ret

call
ret
jr
push

BASIC 1.0

a,d
02

a,b
03

af
c,2C PEN
EFA3

af
FO42

a,d

a,à

nc, FO5B
bc

F025

bC

a, 30 "0"
F065

a,d

04

Z

a, 24 $:
e

hl

(h1),a

FO9B

Z

nc, F065
nl

-I11 189-

F070
F073
FO74
F075
F077
FO7A
FO7B

FO7C
FO7D
FO7E
FO7F
F082
F083
FO084
F086
F087
F088
FO8A
FD8C
FO8E
F090
F091
F092
F093
F095

F096
F097
F099
FO9A

FO9B
FO9C
FOSE
FO9F
FOAT
FOA2
FOAL
FOAG

2A7OAE
77

23
3600
2270AE
ET

C9

7A
B7
FO
3AGFAE
93
C8
3810
47
7A
E620
3E2A
2002
3E20
2B
#1
05
20FB
C9

7À
F601
57
cg

78
062D
87
380F
7A
E698
EE80
37

id
ld
inc
id
1d
pop
ret

1d
or
ret
Id
sub
ret
jr
id
1d
and
id
jr
1d
dec
1d
dec
ir
ret

1d
or
ld
ret

id
id
add
jr
ld
and
xor
scf

BASIC

h1, (AE70)
(hl),a

hl
(h1),00
CAE70),h1
nl

s
Q

, (AEG6F)

Q
Lo
[ea]

D SONO N D D TU ©
SIN LS
eo ® nm

N
QO

a, 2A
nz, F090
a, 20

nl
(hl),a
b

nz, F090

a,d

MOI

1.0

190-

FOA7
FOA8
FOAA
FOAC
FOAE
FOBO
FOB1
FOB3
FOB5
FOB6

FOB7
FOB8
FOB9
FOBC
FOBD
FOBE
FOBF
FOCO
FOC1
FOC2
FOC4
FOC6
FOC7
FOC8
FOC9
FOCB
FOCC
FOCD
FOCE
FOCF
FOD1
FOD2
FOD3
FOD4
FOD5
FOD7
FOD9
FODA
FODB

c8
062B
E608
2002
0620
7A
FG6EF
C610
78
C9

ES
EB
CDDDFO
E
78
87
UF
c8
1A
E6OF
C630
2B
77
1A
E6FO
1F
1F
1F
1F
C630
2B
77
13
05
20EA
FE30
co
0D
23

ret
ld
and
jr
Id
ld
or
add
ld
ret

push
ex
call
pop
ld
add
ld
ret
ld
and
add
dec
ld
id
and
rra
rra
rra
rra
add
dec
id
inc
dec
jr
cp
ret
dec
inc

08

nz, FOBO

b, 20
a,d
EF
a,10
a,b

hl
de,hl
FODD
fl

a, D
a,a
C,a

zZ

a, (de)
0F

a, 30
hl
(h1),a
a, (de)
F0

a, 30

h]
(h1),a
de

b

nz, F0C1
30

nz

C

hl

1.0

16!

charger octet
isoler quartet inférieur
0’, en ASCII

dans buffer
charger octet
isoler quartet supérieur

décaler vers le bas
0’, en ASCII

devant
dans buffer

-111 191-

FODC

FODD
FOEQ
FOE1
FOE2
FOE3
FOE4
FOE6
FOE7
FOE9

FOEA
FOEB
FOEC
FOEE
FOEF
FOFO
FOF1

FOF3
FOFH
FOF5
FOF6
FOF7
FOF8
FOF9
FOFA
FOFB
FOFC
FOFD
FOFE
FOFF
F100
F102
F104
F105
F107
F10A
F10B

C9

1TUGAE
AF

47

B6

2B
2004
OD
20F9
C9

37
8F
30FD
EB
D5
57
1811

1A
1B
D5
37
8F
57
58
JE
8F
27
77
23
1D
20F8
3003
Où
3601
2146AE
7A
87

ret

ld
xor
ld
or
dec
jr
dec
Jr
ret

scf
adc
jr
ex
push
ld
jr

ld
dec
push
scf
adc
1d
1d
1d
adc
daa
1d
inc
dec
jr
Jr
inc
ld
ld
ld
add

BASIC 1

de, AE46

b,a
(h1)

hl

nz, FOEA

nz, FOE2

a,a

nc, FOEB
de,hl
de

d,a
F104

a, (de)
de
de

a, à
d,a
e,b
a, (hl)
a,

(h1),a
hl

e

nz, FOFA
nc, F107
0)
(h1),01
h1,AE46
a,d

a,à

-111 192-

.0

F10C
F10E
F10F
F110
F112
F113

BASIC

nz,FOF8
de

C
nz,FOF3
de,h]

ENORME DEEE DEEE NE EH DE HE HE EE DH HÉHÉ DE HE HE JE JE

F114
F117

de,0101
F11C

HEHEDHODEDEDEHE DEN DH D DEEE HE HE DE DE DE HE DEEE HE EE HE

F119
F11C
F11D
F11E
F121
F122
F123
F124
F127
F12A
F12B
F12C
F12D
F12E
F130
F132
F133
F134
F135
F137
F138
F13A
F13C
F13D
F13E
F13F
F140
F141

20EA ÿr
Di pop
OD dec
20E1 jr
EB ex
C9 ret
110101 ld
1803 jr
110F04 ld
D5 push
79 1d
CDAUBFF call
E3 ex
ES push
C5 push
CDC2FE call]
1157AE Id
AF xor
12 Id
F1 pop
C1 POP
D601 sub
CEO0 adc
F5 push
7D Id
A1 and
F6FO or
27 daa
C6AO add
CE4O adc
1B dec
12 ld
7D Id
B1 or
A9 xor
6F ld

dé, OUOF
de

a,C
FFUB
(sp),hl
hl

DC

FEC2
de, AE5S7
a
(de),a
af

bc

01

a,00

af

a, |

FO

a, AO
a, 40
de
(de),a

1.0

conversion en binaire

conversion en hexa

fixer type de variable

UNT

-111 193-

BASIC

F142 B4 or h

F143 280E jr 25F 153
F145 C5 push pc

F146 7C 1d a,h
F147 1F rra

F148 67 ld h,a
F149 7D ld a, 1
Fi4A 1F rra

F14B 6F Id l,a
F14C 05 dec D

F14D 20F7 jr nz,F146
FiuF C1 POP bc

F150 F1 pop af

F151 18DB jr F12E
F153 F1 pop af

F154  20D8 jr nz,F12E
F156 E1 pop hl

F157 C9 ret

RENE HOMME EH HE HEHE DE HE MED HEHOHEHE HE HE HE DE EDEN EE HE

F158 CDC2FE call FEC2
F15B E7 rst 4

F15C  C3OAFF JP FFOA
LÉLÉLESELL SL ESS SL E SSL SSL SSSLSSS)
F15F  CD91CE call CE91
F162 DS push de

F163 CD37DD call DD37
F166 2C db 2C

F167 CD67CE call CE67
F16A D1 pop de

F16B 12 Id (de),a
F16C C9 ret

LELLES SSL SSL LL S STILL SSSLSLSSILLSSSS:
F16D CD8DFE call  FE8D
F170 44 ld b,h

F171 4D ld (al

1.0

fonction Basic PEEK
UNT
READ RAM; Id a,(hl)
accepter contenu accu comme nombre
entier

instruction Basic POKE
aller chercher adresse 16 bits

tester si encore un caractère

1!
2

aller chercher valeur 8 bits

écrire valeur dans adresse

fonction Basic  INP
CINT

adresse port dans bc

-[11 194-

BASIC

F172 ED78 in a, (c)
F174 C3OAFF Jp FFOA
LÉLLLELLLLLLS LS LS LS LLLLLSLSLSLSLS LS
F177 CD94F1 call F194
F17A ED79 out (c),a
F17C C9 ret

MUNDO DE DE DE D DE DE DE DE DE DE DE DE D DE DE DE DE DE DE DE DE DE DE NE
F17D CD94F1 cal] F194
F180 57 Id d,a

F181 1E00 ld e,00
F183 2808 jr z,F18D
F185 CD3/7DD cal] DD37
F188 2C db 2C

F189 CD67CE call CE67
F18C SF Id e,a

F18D ED78 in a, (c)
F18F AB xor e

F190 A2 and d

F191 28FA jr z,F18D
F193 C9 ret

AHEMENE DE DE DD DE DD DE DE D DEEE DE EDEN DEN DEN DE DE DEEE
F194  CD91CE call CE91
F197 42 1d b,d

F198 uB ld c,e

F199 CD37DD call  DD37
F19C 2C db 2C

F19) C367CE jp CE67
DEOUODEODEODEDEDE DD DE DE D DE DD NE DE DE DE DEN DEN EEE
FAO 23 inc hl

F1A1 7E 1d a, (hD)
F1A2 B7 or a

F1A3 2010 jr nz,F1B5
F1A5S 23 inc hl

F1A6 E5 push hi

F1A7  CDDURC call BCD4

1.0

lire port
accepter contenu accu comme nombre
entier

instruction Basic OUT
aller chercher adresse et valeur
sortir

instruction Basic WAIT

aller chercher valeurs 16 et 8 bits
valeur 8 bits dans d

3ème paramètre zéro

aucune autre valeur ?

tester si encore un caractère
aller chercher valeur 8 bits

et dans e

lire port

lier
et attendre

aller chercher valeurs 16 et 8 bits
aller chercher valeur 16 bits

dans bc
tester si encore un caractère

1!
,

et aller chercher valeur 8 bits

extension d'instruction
augmenter pointeur de programme

octet nul suit ?
non, ‘Unknown command’

KL FIND COMMAND

-II1 195-

FIAA
FIAB
FIAC
FIAE
FIAF
F1B0
F1B1
F1B3

F1B5
F1B7

EB
ET
3007
7E
23
17
30FB
180A

1E1C
C394CA

ex
pop
Jr
ld
inc
rla
Jr
Jr

id
JP

BASIC

de,hl
hl

nc, F1B5
a, (nl)
hi

nc, FIAE
F1BF

e,1C
CA94

EDEN DE DE DD DE DE DE DE DH DE DE DE DE DE DE DEEE DEEE DEN

FIBA
F1BD
F1BF
F1C3
F1C4
F1C7
FICB
F1CD
F1D0
F1D2
F1D5
F1D6
F1D8

FIDB
FIDE
FIE0
F1E1
FIES
F1E7
F1E8
FIEA
FIEE
F1F1

CD91CE
CEFF
ED5372AE
79
3274AE
ED7377AE
0620
CD55DD
3006
CD91CE
D5

10F5
CD4ADD

2275AE
3E20

90
DD210000
DD39

DF

72AE
ED/B77AE
2A75AE
C9

cal]
ld
ld
ld
ld
ld
ld
call
Jr
call
push
djnz
call

ld
1d
SUD
ld
add
rst
dy
1d
ld
ret

CE91

c,FF
(AE72),de
a,C
(AE74),a
CAE77),sp
b, 20
DD55

nc, F1D8
CE91
de
F1CD

DD4A

CAE75),hl
a, 20

b

ix, 0000
ix, Sp

3

AE72

Sp, (AE77)
h1, (AE75)

REDON DEEE MED DE DE HE DH DE HE MEDEDE DEJEMEDEDE DE DEN DEEE NE

F1F2

3EOD

id

a,0D

1.0

adresse de l'instruction dans de

pas trouvé, ‘Unknown command’
aller chercher caractère
ignorer mot instruction

bit 7 mis ?

non

à l'instruction CALL

“Unknown command’
sortir message d'erreur

instruction Basic CALL
aller chercher adresse
FF = Ram sélectionnée
adresse dans AE72

octet de configuration dans AE74
sauver pointeur de pile

maximum 32 paramètres

virgule suit ?

non

aller chercher paramètres

et sur la pile

aller chercher prochain paramètre
fin de l'instruction, sinon ’Syntax

error’

sauver registre h}

nombre de paramètres dans accu

pointeur de pile dans ix
exécuter routine

rappeler pointeur de pile

restaure registre hl

initialiser les TABS
13

-111 19%6-

BASIC 1,0

F1F4 1803 ir F1F9

LES ESLSLLSLLÉRLSLLSLLSLLLSLSLSLSLS LES instruction Basic ZONE

F1F6  CD6DCE call CE6D aller chercher valeur 8 bits non
nulle

F1F9 3279AE ld (AE79),a ranger écart du tabulateur

F1FC C9 ret

MERDE DEEE DE DE ED DE DE ED DE DE DE DE DE DE DE DE DE DE DE DE DE DEEE instruction Basic PRINT

F1FD CDC6C1 call C1C6 numéro de canal

F200 F5 push af

F201 CDO8F2 call F208 sortie PRINT

F204 F1 pop af

F205 C3A2C1 jp CTA2 restaurer numéro de canal

F208 CD51DD call DD51 fin de l'instruction ?

F20B DAUEC3 jp c,C34E oui, sortir LF

F20E FEED CP ED USING"

F210 CACUF2 jp z,F2C4

F213 EB ex de,hl

F214 2124F2 id hl,F224 adresse de base de la table

F217 CD93FF call FF93 parcourir la table

F21A EB ex de,hl

F21B CDFBFF call FFFB jp (de)

F21E CD51DD call DD51 fin de l'instruction ?

F221 30EB jr nc, F20E non, continuer

F223 C9 ret

HE JE DEEE DE EME EH M EE JE HE EH DE HE EH HUE HE

F224 O5 db 05 nombre d'entrées dans la table

F225 33F2 dw F233 adresse de retour si pas trouvé

F227 2C db 2C rare

F228 SCF2 dw F25C

F22A ES db ES SPC’

F22B 77F2 dw F277

F22D EA db EA TAB’

F22E 80F2 dw F280

F230 3B db 3B Fat

F231 3FDD dw DD3F ignorer espaces

= IT 197>

BASIC

CRSLLLEELL EEE LELLE ELLE LS LS ES SSL.)

F233
F236
F237
F238
F23B
F23D
F240
F243
F245
F2u8
F249
F24C
F24D
F250
F253
F256
F257
F258
F25B

CDFBCE
F5

ES
CDASFF
280€
CD9DEE
CDDCF7
3620
2AC2B0
34
2AC2B0
7E
CDB9C2
DAUEC3
CD28F8
ET

F1
CCHEC3
C9

call
push
push
call
jr
call
call
ld
ld
inc
ld
Id
call
call
call
pop
pop
call
ret

CEFB

af

hl

FF45
z,F249
EE9D
F7DC
(h1),20
h1, (BOC2)
(h1)

hi, (BOC2)
a, (hl)
C2B9

nc, C34E
F828

hl

af

z, C3UE

AEROHEOHOHEHE MH HEOHE HE EE HE HEEHHEHEEHE HE GENE GE NN

F25C
F25F
F262
F263
F266
F267
F268
F26À
F26B
F26C
F26D
F26E
F271
F274
F275

CD3FDD
3A79AE
LF
CD90C2
3D

91
30FD
2F

3C

47

81
CDB9C2
D2HEC3
78
181E

call
id
ld
call
dec
sub
jr
cpl
inc
1d
add
call
jp
ld
ir

DD3F

a, (AE79)
c,a
C290

a

C
nc,F267
a

a

b,a

a,C
C2B9
nc, C3UE
a,b
F295

HOHOHEOHEHEOHOME HOME HE DEEE HE EME HE He HE DE HE EH M EEE EE

F277

CDACF2

call

F2A0

1.0

PRINT
aller chercher expression

tester si chaîne

oui

convertir nombre en chaîne ASCII
aller chercher paramètres de chaîne
5’, ajouter espaces

sélectionner courant de sortie
sortir LF
sortir chaîne

sortir LF

PRINT ,
ignorer espaces
écart de tabulation

sélectionner courant de sortie
sortir LF

PRINT SPC
aller chercher argument entre

-111 198-

BASIC 1.0

parenthèses

F27A CDAFF2 cal] F2AF

F27D 7B id a,e

F27E 1815 jr F295

DÉS SE SSL SSL SRESLSIRLLLLILLLLLLLLILLS] PRINT TAB

F280  CDAOF2 call F2A0 aller chercher argument entre
parenthèses

F283 1B dec de

F284 CDAFF2 call F2AF

F287 CD90C2 call C290

F28A 2F cpl a

F28B 3C inc a

F28C 1C inc e

F28D 83 add a,e

F28E 3805 jr c,F295

F290 CDUEC3 call C3UE sortir LF

F293 1D dec e

F294 7B 1d a,e

F295 47 1d b,a

F29 O4 inc (e)

F297 05 dec b

F298 C8 ret Z

F299 3E20 ld a, 20 TDi

F29B CD56C3 call C356 sortir

F29E 18F7 jr F297

OA RE RERURE aller chercher argument entre parenthèses

F2A0 CD3FDD call DD3F ignorer espaces

F2A3 CD3/DD call DD37 tester si encore un caractère

F246 28 db 28 de

F2A8 CD86CE call CE86 aller chercher valeur entière avec
signe

F2A9 CD37DD call DD37 tester si encore un caractère

F2AD 29 db 29 DE

F2AE C9 ret

F2AF 7A ld a,d

F2B0 17 rla

F2B1 3003 Jr nc, F2B6

-111 199-

BASIC 1,0

F2B3 110000 ld de,0000

F2B6 CD9FC2 call C29F

F2B9 DO ret nc

F2BA ES push hl

F2BB EB ex de,hl

F2BC SF ld e,a accu comme diviseur

F2BD 1600 id d,00 Hi-Byte zéro

F2BF CDC1BD cal] BDC1 division entiers sans signe
F2C2 El pop hl reste en de

F2C3 C9 ret

CELLES LLL LIL L LS SL LL LILI SLSLSLSSSES) PRINT USING

F2C4 CD3FDD call DD3F ignorer espaces

F2C7 CDASCE call CEAS aller chercher expression chaîne
F2CA CD37DD call DD37 tester si encore un caractère
F2CD 3B db 3B “e

F2CE ES push hl

F2CF 2AC2B0 ld h1, (BOC2)

F2D2 7E ld a,(h1l)

F2D3 B7 or a

F2D4 2875 jr z,F34B ’Improper argument”

F2D6 E3 ex (sp),hl

F2D7 CDFBCE call CEFB aller chercher expression
F2DA AF xor a

F2DB 327AAE ld (AE7A),a

F2DE D1 pop de

F2DF DS push de

F2E0 EB ex de,hl

F2E1 46 ld b, (h1)

F2E2 23 inc hl

F2E3 7E ld a, (hl)

F2E4 23 inc hl

F2E5 66 ld h, (h1)

F2E6 6F ld La

F2E7 EB ex de,hl

F2E8 CD24F3 call F324

F2EB 305€ Jr nc,F34B ‘Improper argument’

F2ED CD51DD call DD51 fin de l'instruction ?
F2F0 381D Jr c,F30F oui

F2F2 FE3B cp 3B Fr

-111 200-

F2F4
F2F6
F2F8
F2FA
F2FD
F2FF
F300
F303
F304
F305
F306
F308
F30B
F30D

F30F
F310
F312
F315
F316
F317
F31A
F31B
F31E
F31F
F322
F323

F324
F325
F326
F328
F32A
F32B
F32D
F32F
F330
F331

F333

2804
FE2C
204C
CD3FDD
2810
DS
CDFBCE
Di

78

B7
28D6
CD24F3
30D1
18DE

F5
3EFF
327AAE
78

B7
CH2uF3
F1
DCUEC3
E3
CDE8FB
E1

cg

E5
1A
FESF
2009
78
FE02
380C
13
05
1808

CDS0F3

BASIC 1.0

jr Z,F2FA

(ao) 2C PSS

WR nz,F346 ’Syntax error’
call DD3F ignorer espaces
Jr Z,F30F fin de ligne ?
push de

call CEFB aller chercher expression
pop de

ld a,b

or a

Jr Z, F2DE

call F324

jr - nc,F2DE

jr F2ED

push af

1d a,FF

ld (AE7A),a

ld a,b

or a

call nz,F324

pop af

call c,C3HE sortir LF

ex (sp),h1

call FBE8

pop hl

ret

push hi

ld a, (de)

Cp 5F

Jr nz,F333

1d a,b

cp 02

Jr c,.F33B

inc de

dec D

Jr F33B

call F350

-11I 201-

BASIC 1.0

F336 DUA3F3 call nc, F3A3
F339 3809 jr c,F344

-111I 202-

F33B
F33C
F33F
F340
F341
F343
F34l
F345

F346
F348

F34B
F34D

F350
F351
F353
F355
F357
F359
F35B
F35D
F35F
F360
F361
F362
F364
F365
F366
F368
F369
F36B
F36D
F36E
F370
F372
F373
F374
F375

1A
CD56C3
13

05
20E2
B7

ET

C9

1E02
C394CA

1E05
C394CA

1A
FE21
0E01
2821
FE26
0E00
281B
EESC
CO
C5
D5
0E02
13
05
280A
JA
FESC
2809
OC
FE20
28F2
Di
C1
B7
C9

Id
call
inc
dec
Jr
or
pop
ret

1d
Jp

ld
JP

ld
CP
1d
jr
CP
1d
Jr
Xor
ret
push
push
ld
inc
dec
Jr
Id
CP
jr
inc
CP
ir
pop
pop
or
ret

BASIC

a, (de)
C356

de

b
nz,F325
a

h]

e,02
CA94

e,05
CA94

a, (de)
21
c;,01
z,F378
26
c,00
z,F378
5C

nz

bc

de
c,02
de

b
z,F372
a, (de)
5C
z,F376
C

20
z,F364
de

bc

a

sortir

’Syntax error’
sortir message d'erreur

’Improper argument‘
sortir message d'erreur

’Backslash'

‘Backslash'

-ÏII 202-

BASIC 1.0

F376 F1 pop af

F377 F1 pop af

F378 13 inc de

F379 05 dec D

F37A C5 push bc

F37B DS push de

F37C 3A7AAE id a, (AE7A)

F37F B7 or a

F380 201D Jr nz,F39F

F382 CD3CFF call FF3C type ‘chaine’, sinon ‘Type mismatch'
F385 79 1d a,C

F386 B7 or a

F387 F5 push af

F388 41 ld D,c

F389 0EO00 ld c,00

F38B 2AC2B0 ld h1, (BOC2)

F38E EB ex de,hl

F38F C471F9 call nz,F971

F392 CD28F8 call F828 sortir chaîne
F395 F1 pop af

F396 2807 Jr Z,F39F

F398 2AC2B0 ld h1, (BOC2)

F39B 96 SUD (h1l)

F39C CD95F2 call F295

F39F  D1 pop de

F3A0 C1 pop bc

F3A1 37 scf

F3A2 C9 ret

F3A3 CDBAF3 call F3BA tester si caractère de formatage
F3A6 DO ret nc

F3A7 3ZA7AAE ld a, (AE7A)

F3AA B7 or a

F3AB 200B jr nz,F3B8

F3AD C5 push bc

F3AE D5 push de

F3AF 79 1d a,C

F3B0 CD9FEE call EE9F formater nombre
F3B3 CD41C3 call C341 sortir chaîne Jusqu'à (0)
F3B6 D1 pop de

-111 203-

BASIC 1,0

F3B7 C1 pop bc

F3B8 37 scf

F3B9 C9 ret

LÉSÉSRSSLLLSLS ES LLLÉESSSLSRSSLSLESS SES.) tester si caractère de formatage
F3BA C5 push bc

F3BB D5 push de

F3BC 0E80 id c,80

F3BE 2600 1d h,00

F3C0 1A 1d a, (de)

F3C1 FE2B cp 2B 14!
F3C3 2007 Jr nz,F3CC

F3C5 13 inc de

F3C6 05 dec D

F3C7 2823 Jr Z,F3EC

F3C9 24 inc h

F3CA 0E88 Id c,88

F3CC A ld a, (de)

F3CD FE2E CP 2E se
F3CF 281F jr z,F3F0

F3D1 FE23 cp 23 4
F3D3 2839 Jr Z, F4OE

F3D5 13 inc de

F3D6 05 dec b

F3D7 2813 Jr Z,F3EC

F3D9 EB ex de,hl

F3DA BE CP (h1)

F3DB EB ex de,h]

F3DC 200E jr nz, F3EC

F3DE 24 inc h

F3DF 24 inc h

F3E0 2E04 id 1,04

F3E2 FE24 Cp 24 '$
F3E4 2823 jr z,F409

F3E6 2E20 1d 1,20 art
F3E8 FE2A CP 2A re,
F3EA 2811 jr Z,F3FD

F3EC D1 pop de

F3ED C1 pop bc

F3EE B7 or a

-FIT 204-

BASIC 1.0

F3EF C9 ret

F3F0 13 inc de

F3F1 05 dec D

F3F2 28F8 jr Z,F3EC

F3F4 A ld a, (de)

F3FS FE23 cp 23 #4"
F3F7  20F3 jr nz,F3EC

F3F9 1B dec de

F3FA Où inc e)

F3FB 1811 jr F4OE

F3FD 13 inc de

P3FE 05 dec D

F3FF  280A Jr z,F40B

F4OT TA ld a, (de)

F402 FE24 CP 24 (gs
F404 2005 jr nz, F40B

F406 24 inc h

F407 2E24 Id 1,24

F409 13 inc de

FUOA 05 dec D

F4OB 79 ld a, C

F40C B5 or l

F4OD 4F ld C,a

FUCE F1 pop af

F4OF F1 pop af

F410 CD1BF4 call F41B

F13 7C ld a,h

F414 85 add a, 1

F415 FE15 CP 15

F417  D2UBF3 jp nc,F34B ‘Improper argument”
FHIA C9 ret

FH1B 2E00 id 1,00

FH1D Où inc D

FE 05 dec D

F41F C8 ret Z

F420 A id a, (de)

F421 FEZ2E CP 2E AR

-[11 205-

F423
F425
F427
F429
F42B
F42D
F42E
F42F
F430
F432

F433
F434
F436
F437

FY39
FU3A
FU3B
FY3C
F43D
FU3E
F44O
F4u2
F4u3
Fuy
Fuu6
F4y8
F4yg
FUUA
FYuC
F4uD
FUUE
F450
F451
F452
F454
F455
F456
F458

2814
FE2C
2804
FE23
2015
24
13
05
20EE
C9

79
F602
UF
18F4

2C
13
05
C8
1A
FE23
28F7
EB
ES
FESE
2018
23
BE
2014
23
BE
2010
23
BE
200€
23
78
D604
3806

ir
cp
Jr
CP
Jr
inc
inc
dec
jr
ret

1d
or
Id
Jr

inc
inc
dec
ret
ld
CP
jr
ex
push
Cp
Jr
inc
CP
jr
inc
cp
Jr
inc
cp
jr
inc
ld
sub
Jr

BASIC

z,F439
2C
z,F433
23
nz,F442
h

de

D
nz,F420

a, C
02

c,a
F42D

l

de

b

zZ

a, (de)
23
z,F439
de,hl
hl

SE

nz, F460
hi

(hl)
nz, F460
hl

(h1)
nz,F460
h]

(h1)
nz, F460
hl

a,bD

04
c,F460

-III

1.0

206-

BASIC 1.0

FUYCA 47 ld b,a

FU4SB E3 ex (sp},hl

FSC 79 ld a,C

F45D F640 or 40

FUSF UF ld c,a

F460 E] pop hl

F461 EB ex de,hl

F462 78 ld a,b

F463 B7 or a

F464 C8 ret zZ

F465 79 1d a, C

F466  E608 and 08

F468 CO ret nz

F469 TA ld a, (de)

FH6A FE2D CP 2D =?

F46C 3E10 ld a,10

FU6E 2806 Jr z,F476

F470 A ld a, (de)

F471 FE2B Cp 2B r+?

F473 CO ret nz

F474 3618 ld a,18

F476 B1 or C

F477 UF 1d c,a

F478 13 inc de

F479 O5 dec D

F47A C9 ret

EME MEDE DE HE DEDEUDENE DEEE EME DE DE DE DEDEME DENON instruction Basic WRITE
F47B CDC6C1 call C1C6 numéro de canal présent ?
FY7E F5 push af

F47F  CDS1DD call DD51 fin de l'instruction ?
F482 3839 jr c,F4BD oui

F484  CDFBCE call CEFB aller chercher expression
F487 F5 push af

Fu88 ES5 push hl

F489 CDASFF call FF45 tester si chaîne

F48C 280B jr z,F499 oui, sortir avec guillemets
FUSE CD8FEE call EE8F

F491 CDDCF7 call F7DC

F494  CD28F8 call F828 sortir chaîne

-I11 207-

F497

F499
F49B
FUSE
FHA1
F4A3
FUAG
F4A7
FUAS
FUAA
FHAC
FUAE
F4BO
F4B3
F4B6
F4B8
FUBB

F4BD
F4CO
F4C1

180D

3E22
CD56C3
CD28F8
3E22
CD56C3
ET

F1
2813
FE3B
2805
FE2C
C246F3
CD3FDD
3E2C
CD56C3
18C7

CD4EC3
F1
C3A201

Jr

ld
call
call
ld
call
pop
Pop
Jr
Cp
jr
Cp
jp
call
id
cal]
Jr

call
pop
JP

BASIC

F4AG

a, 22
C356
F828
a, 22
C356
hl

af
z,F4BD
3B
z,F4B3
2C -
nz,F346
DD3F
a, 2C
C356
F484

C34E
af
C1A2

DOHHOHDEDEDE DE DEEE EH HE DE DE DE DEN DE DE DEEE DE D DD DEDE NE

F4C4
F4C7
F4CA
FACB
FACE
F4D1
F4D4
F4D5
F4D8
FUDB
F4DC
FUDD
F4EO
FHE1
F4E2
FUE3

O1OOAC
CDBEFF
DO
227BAE
228FB0
227DAE
EB
227FAE
012F01
09

D8
281AE
EB

23

B7
ED52

1d
call
ret
1d
id
id
ex
ld
1d
add
ret
ld

ex
inc
or
sbc

bc, ACOO
FFBE

nc
CAE7B),h1
(BO8F),h1
(AE7D),h1
de,h]
CAE7F),h1
bc,012F
hl,bc

C
(AE81),h1
de,hl

hl

a

hl,de

1,0

sut

sortir
sortir chaîne

1ur

sortir

’Syntax error’
ignorer espaces

Cr]
2

sortir

sortir LF

configurer mémoire

place en mémoire de de à hl
comparer hl <> bc

adresse supérieure < ACO0 ?
HIMEM

fin des chaînes

fin de la Ram libre

début de la Ram libre
plus 303

donne début du programme

-111 208-

BASIC 1.0

FUES D8 ret C

FUE6G 7C ld a,h

F4E7 FEO4 cp 04

F4E9 D8 ret C

FHEA AF Xor a

FUEB 3291B0 1d (B091),a

FUEE C9 ret

HOHOHOMOMDE DE EH ME DE HE DEEE HN DE DEEE HE HE EE instruction Basic MEMORY
FUEF  CD3EFC call FC3E Garbage Collection

F4F2 CD91CE call CE91 aller chercher valeur 16 bits
FUFS ES push hl

F4F6 CDS50F7 call F750

F4F9 CD75F6 call F675

FFC 227BAE id (AE7B),hl fixer HIMEM

FUFF El pop nl

F500 C9 ret

LÉELL ELLES TS E STILL SISSLLSLLLZLSLLIL.. place pour programme à charger
F501 DS push de

F502 2A7FAE 1d h1, CAE7F) début de la Ram libre
F505 EB ex de,hl

F506 2A7BAE 1d hl, (AE7B) HIMEM

F509  CDCFFF call FFCF hl :=hl - de

F50OC E3 ex (sp),hl

F50D CDCFFF call FFCF hl :=hl - de

F510 D1 pop de

F511 13 inc de

F512 CDB8FF call FFB8 comparer hl <> de

F515 3803 Jr c,F51A ‘Memory full”

F517 2B dec h]

F518 09 add hl,bc

F519 DO ret nc

F51A C33EF7 jp F73E ‘Memory full”
RARE RER REER ARE calculer longueur de la zone des chaînes
F51D DS push de

FSIE E5 push hl

F51F  2A8DB0 ld h1, (BO8D) début des chaînes

F522 EB ex de,hl

-[11 209-

F523
F526
F529
F52A
F52B

LRRÉLSL LS LL LL LL SL LSL SEL;

F52C
F52F
F530
F533
F536
F537
F53A
F53D
FS3E
F541
F54u
F545
F548

2A8FB0
CDDAFF
ET
D1
C9

2A83AE
09
2283AE
2A85AE
09
2285AE
2A87AE
09
2287AE
2A89AE
09
2289AE
C9

1d
call
POP
pop
ret

ld
add
ld
1d
add
ld
ld
add
ld
Id
add
ld
ret

BASIC 1.0

h1, (BO8F) fin des chaines
FFDA bc := hl - de
hl

de

augmenter pointeurs de PRG et de variables de bc
hl, (AE83) fin du programme
h1,bc

(AE83),hl fin du programme
hl,(AE85) début des variables
hl,bc

(AE85),h1 début des variables
hl, (AE87) début des tableaux
hl,bc

(AE87),hl début des tableaux
hl, (AE89) fin des tableaux
hl,bc

(AE89),h1 fin des tableaux

LELELELELLELLLLL ESS LS SSSR SI LS LEE.)

F549
F54C
F54D
F550
F553
F554
F557
F55A
F55B
F55E
F55F
F562
F563
F564
F565
F568
F569
F56C

2A85AE
EB
2A87AE
CDCFFF
ES
2A89AE
CDDAFF
C5
2A8DB0
EB
2A89AE
2B
78
Bi
CUFSFF
EB
228DB0
C1

1d
ex
ld
call
push
id
call
push
id
ex
ld
dec
id
or
call
ex
1d
pop

hl, (AE85) début des variables

de,hl

hl, (AE87) début des tableaux
FFCF hl :=hl - de

hl

hl, (AE89) fin des tableaux
FFDA bc := hl - de

bc

h1, (B0O8D) début des chaînes
de,hl

h1, (AE89) fin des tableaux
hl

a,b

C

nz,FFF5 lddr

de,hl

(B08D),hl début des chaînes
bc

-111 210-

BASIC 1.0

F56D D1 pop de

F56E C3BID5S jp D5B1 restaurer le pointeur de variable
CELILLILILLLLLLLLLLLSLSLSLSLLS SES ES)

F571 2A83AE id h1, (AE83) fin de programme

F574  2285AE 1d (AE85),h1 égale début des variables
F577 EB ex de,hl

F578 19 add hl,de plus longueur des variables
F579 2287AE ld (AE87),hl égale début des tableaux
F57C 2A8DB0 id h1, (B08D) début des chaînes

F57F 23 inc hl

F580 78 ld a,b zone des chaînes

F581 B1 or c

F582 C4F2FF call nz, FFF2 présent, alors Idir

F585 2B dec hl

F586 228DB0 ld (BO8D),h1 début des chaînes

F589 EB ex de,hl

F58A 2289AE ld (AE89),hl fin des tableaux

F58D C9 ret

LLLLILLLLILSLLSLLLLLLLSLSLSLLLSLSLSL) initialiser pile Basic
F58E F5 push af

FS8F ES push hi

F590 218BAE ld hl,AE8B

F593 228BB0 ld (B08B),h1 pointeur de pile Basic

F596 3E01 1d a,01

F598  CDBOFS5 call F5SBO réserver place dans pile Basic
F59B 3600 ld (h1),00

F59D El pop hl

FSJÆ F1 pop af

F59F C9 ret

HEDDE DE DE DE DEHE DEMO DE DE DE DE DE DE DE DE ÉD DE DEEE DEEE libérer place dans pile Basic
F5AO 2A8BB0 ld h]1, (BO8B) pointeur de pile Basic

FSA3 2F cpl a

F5A4  3C inc a ôter contenu accu

FSA5S C8 ret Z

F5SA6 85 add a,

F5A7 6F id 1,a

FSA8 3EFF ld a, FF

-111 211-

BASIC

FSAA  8C adc a,h

FSAB 67 ld ha

FSAC  228BB0 ld (B08B),h1
FSAF C9 ret

CELL LSISLLLLLLSLLLLSSISSLSLLLSLLSLSE SZ
F5BO 2A8BBO ld h1, (BO8B)
FSB3 ES push hl

F5B4 85 add a, l

F5SB5 6F ld 1,a

F5B6 8C adc a,h

F5B7 95 sub 1

F5B8 67 ld h,a

F5B9 228BB0 1d (BO8B),h1
F5BC 3E78 ld a,78
FSBE 85 add a, |

FSBF  3E4F ld a,4F

F5CT 8C adc a,h

F5C2 E1 pop hl

F5C3 DO ret nc

F5C4  CD8EFS call F58E

F5C7 C33EF7 JP F73E
LÉLLLEL ELLES LLL LS SSSLLSLLSSLLLLLS)
FSCA 2A8FBO ld h1, (B08F)
FSCD 228DB0 id (B08D),h1
F5DO C9 ret
DOMOHODEDEODONEDEDEDEHEDE DEEE DEEE DE DEEE DEEE DEEE
F5D1 2F cpl à

FSD2 4F ld C,a

FSD3 O6FF id b,FF

F5D5 03 inc bc

F5D6 CDE6FS call F5E6

F5D9 DO ret nc

FSDA CD3EFC call FC3E
F5SDD CDE6FS call F5E6

FSEO DO ret nc

FSET 1E0E ld e,0E

FSE3 C394CA jp CA94

1.0

pointeur de pile Basic

réserver place dans pile Basic
pointeur de pile Basic

additionner contenu accu

pointeur de pile Basic
donne plus &4F78 dépassement ?

alors pointeur de pile > 8&B088

initialiser pile Basic
‘Memory full’

fin des chaînes
début des chaînes

réserver place pour chaîne
accu contient longueur de chaîne

moins longueur dans bc

étendre zone de chaînes vers le bas
y a-t-il de la place ?

non, déclencher Garbage Collection
y a-t-il maintenant de la place ?
oui

‘String space full’

sortir message d'erreur

-JI1 212-

BASIC 1.0

LELLLLLLLSLSSLLLLLSLLSLLSES SES] ÿ a-t
F5SE6 2A89AE id h1, (AE89)
F5E9 EB ex de,hl
FSEA  2A8DB0 ld h1, (B08D)
FSED 09 add hi,bc
FSEE CDB8FF call FFB8

F5F1 D8 ret C

F5F2 228DB0 ld (B0O8D),h1
F5FS5S 23 inc hl

F5F6 EB ex de,hl
FSF7 C9 ret
LELÉILLLLÉELLLSLSLLLSSLSLLSLLESLSE)

F5SF8 2A89AE 1d h1, (AE89)
F5SFB C5 push bc

FSFC D5 push de

FSFD DS push de

FSFE ES push hi

FSFF  CD18F6 call F618

F602 DA3EF7 Jp c,F73E
F605 El pop hl

F606 C1 pop bc

F607 D5 push de

F608 7D id a, l

F609 91 SUD C

F60A 4F 1d C,a

F60B 7C 1d a,h

F60C 98 sbc a,b

F60D 47 id b,a

F60E 2B dec hl

F60F  1B dec de

F610 B1 or C

F611 CAUFSFF call nz, FFF5
F614 El pop hl

F615 D1 pop de

F616 C1 pop bc

F617 C9 ret

HORMONE NEONEUENENEE DE DE MEHÉ DE MEUEDEJÉ DE NÉ Y a-t
F618 09 add h1,bc

-I1

-il de la place dans la zone des chaînes
fin des tableaux

début des chaînes
moins longueur de la nouvelle chaîne
comparer hl <> de
début des chaînes
dans de
réserver place dans la zone des chaînes

fin des tableaux
contient nombre d'octets

y a-t-il de la place ?
non, ‘Memory full”

lddr

-il de la place dans la zone des chaînes

fin tableaux plus nouvelle place

I 213-

F619
F61A
F61B

F6IE
F61F
F622
F625

D8
EB
CD22F6

DO

CD3EFC
2A8DB0
C3B8FF

ret
ex
call

ret
call
Id
Jp

BASIC

C
de,hl
F622

nc

FC3E

h1, (BO8D)
FFB8

DEDENENEDEDE DE DD DE NE HE DE DE DE HE DE DE DE DEEE DE DE DEEE JE DEN

F628
F62B
F62C
F62F

2A89AE
EB

2A8DB0
C3CFFF

1d
ex
1d
Jp

h1, CAE89)
de,hl
h1, (BO8D)
FFFC

DD DEDE DE DEEE DE DEMI DE DEEE DE DE DEEE DE DEEE DE DE DEN

F632
F635

110100
1803

1d
Jr

de,0001
F63A

DEDEDEDEDE DE DE DE DEEE HD DÉ DEEE ÉD HE DE EU DEEE

F637
F63A
F63B
F63C
F63F
F640
F641
F643
F644
F645
F648
F64B
FGUE
F651
F652
F655
F658
F659
F65C
F65D

110208
C5

ES
2191B0
7E

B7
201D
D5

ES
210010
010000
CD43F7
2292B0
EB
2A7DAE
2294B0
EB
227DAE
ET

D1

ld
push
push
id
ld
or
Jr
push
push
ld
ld
call
ld
ex
Id
ld
ex
ld
pop
pop

de,0802
bc

hl
h1,B091
a, (h1)

a

nz,F660
de

hl
h1,1000
bc, 0000
F743
(B0O92),h1l
de,hl

h1, (AE7D)
(B094),h1
de,hl
(CAE7D),h1l
hl

de

1

.0

dépassement ?

comparer nouvelle fin des variables

avec début des chaînes

y a-t-il de la place ?

non, déclencher Garbage Collection
début des chaînes

comparer hl <> de

calculer place mémoire libre
fin des variables

début des chaînes
hl := hl - de

mettre en place le buffer d'entrée

mettre en place buffer de sortie

nouvelle fin de la Ram libre

fin de la Ram libre
mémoire pour Ram libre

fin de la Ram libre

-I11 214-

BASIC 1.0

F65E 3E04 ld a,04

F660 B3 or e

F661 77 ld (h1),a

F662 2A92B0 ld h1, (B092) nouvelle fin de la Ram libre
F665 23 inc h1

F666 1E00 ld e,00

F668 19 add hl,de

F669 EB ex de,hl

F664 El pop hl

F66B C1 pop bc

F66C C9 ret

LÉLSLSSELI SELS LLESLLLESLLLSSLSLS STE fermer buffer d'entrée
F66D 3EFE ld a,FE

F66F 1806 jr F677

HOUHOMOMEODE DEEE DE MEHE DE DE DD HE EDEN EH HE DE ME DE DE NE fermer buffer de sortie
F671 3EFD ld a,FD

F673 1802 Jr F677

HOHEHEHE HE DE HE DE DE HE DE DE EHE DE DE DEEE DEEE DEEE ÉD

F675 3EFF 1d a,FF

F677 C5 push bc

F678 D5 push de

F679 ES push hi

F67A 2191B0 1d h1,B091

F67D A6 and (hi)

F67E 77 ld (hl),a

F67F FEO4 CP 04

F681 2016 jr nz,F699

F683 2A92B0 ld h1, (B092) nouvelle fin de la Ram libre
F686 EB ex de,hl

F687 210010 ld h1,1000

F68A CD2EF7 call F72E

F68D 200A jr nz, F699

F68F AF Xor a

F690 3291B0 ld (B091),a

F693 2A94B0 1d h1, (B094) mémoire pour Ram libre
F696 227DAE ld CAE7D),hl fin de la Ram libre
F699 El pop h]

-I1II 215-

BASIC 1.0

F69A D1 pop de

F69B C1 pop bc

F69C C9 ret

CELL LLLILILLLLLLLLLLSISLSLSSISLSSS SES) instruction Basic SYMBOL

F69D FE80 CP 80 AFTER’

F69F  282C Jr z,F6CD

F6AT CD67CE call CE67 aller chercher valeurs 8 bits

F6A4 UF 1d c,à

F64A5S CD3/DD call DD37 tester si encore un caractère

F648 2C db 2C Fa

F64A9 0608 Id b,08 8 valeurs

F6AB CD67CE call CE67 aller chercher valeurs 8 bits

F6EAE F5 push af sur pile

F6AF O5 dec b

F6BO 2808 jr z,F6BA déjà 8 valeurs ?

F6B2 CD55DD call DD55 virgule suit ?

F6B5 38F4 Jr c,F6AB oui, aller chercher valeur suivante

F6B7 AF xor a

F6B8 18F4 Jr FGAE

F6BA EB ex de,hl sauver hl

F6BB 79 ld a, C caractère dans a

F6BC CDASBB call BBAS TXT GET MATRIX

F6BF 3068 Jr nc, F729 matrice pas dans Ram, ‘Improper
Argument ”

F6C1 010800 Id bc, 0008 8

F6C4 09 add hl,bc plus adresse de matrice

F6C5 F1 pop af aller chercher octet sur pile

F6C6 2B dec hl

F6C7 77 ld (h]),a écrire dans table de matrice

F6C8 OD dec C octet suivant

F6C9 20FA Jr nz,F6C5

F6CB EB ex de,hi ramener hl

F6CC C9 ret

ÉTELLILLIILLLLLLILSLLLLISISLSISSLSS SSD. SYMBOL AFTER

F6CD CD3FDD call DD3F ignorer espaces

F6D0 CD86CE call CE86 aller chercher valeur entière avec
signe

-1I1 216-

F6D3
F6D4
F6D7
F6DA

F6DC
F6DD
F6E0
F6E1
F6E3
F6E4
F6ES
F6E7
F6E8
F6E9
F6E À
F6EB
F6EC
F6EF
F6F1
F6F4
F6F7
F6FA
F6FD
F700
F701
F704

ES

210001

CDB8FF
384D

D5
CDAEBB
EB
301D
2F

6F
2600
25

29

29

29

1B
CD2EF7
2038
2A96B0
227DAE
CD/5F6
110001
CDABBB
Di
CDO6F7
E

push

ld

call
jr

push
call
ex
jr
cpl
ld
1d
inc
add
add
add
dec
call
jr
Id
1d
cali
ld
call
pop
call
pop

BASIC 1.0

hi
h1,0100 256
FFB8 comparer hi <> de
c,F729 supérieur égal 256, ‘Improper
argument‘
de
BBAE TXT GET M TABLE
de,hi adresse de matrice dans de
nc, F700 matrice pas encore définie ?
a
l,a
h,00
hl
hi,hl
hlhl
hl1,hl
de
F72E
nz,F729 ’Improper argument‘
h1, (B096)
(AE7D),h1 fin de la Ram libre
F675
de,0100
BBAB TXT SET M TABLE
de
F706
hl

-III 217-

F705

F706
F707
F708
F709
F70B
F70C
F70D
F70E
F70F
F710
F711
F712
F713
F716
F719
F71A
F71D
F720
F721
F724
F725
F726

F729
F72B

F72€
F72F
F732
F735
F736
F737
F738
F73B
F73C

F73E
F740

C9

AF

93

6F
3E01
JA

67

B5

c8

D5

29

29

29
010040
CD43F7
EB
2A7DAE
2296B0
EB
227DAE
Di

23
C3ABBB

1E05
C394CA

E5
2A/BAE
CDB8FF
ET

Co

19
227DAE
EB
1812

1E07
C394CA

ret

xor
sub
ld
ld
sbc
ld
or
ret
push
add
add
add
1d
call
ex
ld
Id
ex
ld
pop
inc
jp

Id
Jp

push
Id
call
pop
ret
add
id
ex
Jr

id
Jp

BASIC 1,0

de ranger premier caractère
hLh]

hLhl

hlhl

bc,4000

F743

de,hl

h1, (AE7D) fin de Ram libre
(B09%6),h1

de,hl

(AE7D),h1 fin de Ram libre

de premier caractère

hi adresse de début de la table
BBAB TXT SET M TABLE

e,05 ’Improper argument”
CA94 sortir message d'erreur

hl

h1, (AE7B) HIMEM

FFB8 comparer hl <> de
hi

nz

hl,de

CAE7D),h1l fin de Ram libre
de,hl

F750

e,07 ‘Memory full’
CA94 sortir message d'erreur

-111 218-

F743
F744
F747
F74A
F74D
F74F
F750
F753
F754
F757
F75A
F75C
F75F
F762
F763
F765
F766
F769
F76B
F76E
F76F
F772
F775
F778
F77B
F77F
F780
F781
F783
F784
F786
F789
F78A
F78B
F78C
F78D
F790
F791
F792
F793

EB
2A/BAE
CDCFFF
CDBEFF
38EF
EB
CD3EFC
D5
2A7DAE
CDB8FF
38E2
CDIDF5
2A89AE
09
38D9
2B
CDB8FF
30D3
2A7BAE
EB
CDCFFF
2298B0
11BBF7
CD74DA
ED4B98B0
78

07
3816
B1
282F
2A8FB0
54

5D

09

ES
CD1DFS
EB

78

B1
CUFSFF

ex
ld
call
call
Jr
ex
cal]
push
ld
call
jr
call
ld
add
Jr
dec
call
jr
1d
ex
call
1d
1d
call
Id
ld
rica
Jr
or
jr
ld
ld
id
add
push
call
ex
1d
or
call

BASIC

de,hl

hl, (AE7B)
FFCF
FFBE
c,F73E
de,hl
FC3E

de

h1, CAE7D)
FFB8
c,F73E
F51D

hi, (AE89)
hl,bc
c,F73E

hl

FFB8
nc,F73E
h1, CAE7B)
de,hl
FFCF
(BO98),h1
de, F7BB
DA74

bc, (B098)
a,D

c,F799
C
Z,F7B5
h1, (BO8F)
d,h

e,l
hl,bc
h]

F51D
de,hl
a,b

C
nz,FFF5

1.0

HIMEM

hl :=h1 - de
comparer hi <> bc
‘Memory full”

Garbage Collection

fin de Ram libre

comparer hl <> de

‘Memory full’

calculer longueur zone des chaînes
fin des tableaux

plus longueur zone des chaînes
‘Memory full”

comparer hl <> de
‘Memory full’
HIMEM

hl := hl - de

fin des chaînes

calculer longueur zone des chaînes

lddr

-111 219-

BASIC 1.0

F796 El pop hl
F797 1815 Jr F7AE

-111 220-

BASIC 1,0

F799 2A8DB0 1d h1, (BO8D) début des chaînes
F79C 54 ld d,h

F79D 5D ld e,l

F7/SŒ 09 add hl,bc

F79F ES push hl

F7AO CDIDFS call F51D calculer longueur zone des chaînes
F7A3 EB ex de,h1l

F7A4 23 inc hl

F7AS 13 inc de

F7A6 78 ld a,bD

F7A7 B1 or C

F7A8 C4F2FF call nz, FFF2 ldir

F7AB EB ex de,hl

F7AC 2B dec hl

F7AD D1 pop de

F/AE  228FB0 ld (BO8F),h1 fin des chaînes
F7B1 EB ex de,hl

F7B2 228DB0 1d (B08D),h1 début des chaînes
F7B5 El pop hl

F7B6 227BAE ld (AE7B),h1 HIMEM

F7B9 AF xor a

F/BA C9 ret

F7BB 2A83AE ld hl, (AE83) fin du programme
F7BE CDBEFF call FFBE comparer hl <> bc
F7C1 DO ret nc

F7C2 2A98B0 id h1, (B098)

F7C5 09 add h1,bc

F/C6 EB ex de,hl

F7C7 72 ld (hl),d

F7C8 2B dec hl

F7C9 73 ld (hl),e

F7CA C9 ret

LÉRLRLLLLLS SL ÉESLS LES LLSSSLLLLSLLILLLIZSS. lire chaîne

F7CB 23 inc hl

F7CC CDF9F7 call F7F9

FACF. JE ld a, (h1)

F7DO FE22 CP 22 1"!, fin de chaîne ?
F/D2 CA3FDD Jp z,DD3F oui, ignorer espaces suivants

-[11 221-

F7D5
F7D6
F7D8
F7D9
F7DA

F7DC
F7DF
F7E0
F7E1
F7E2
F7E3
F7E4

F7E6
F7E9
F7EA
F7EB
F7EC
F7EE
F7EF
F7F1
F7F3
F7F5
F7F6
F7F7

B7
2837
O4
23
18F3

CDF9F7
LÉ

B7

C8

23

04
18F9

CDF9F7
LF
7E
B7
2821
B9
281E
FE2C
2814
23
O4
18F1

or
jr
inc
inc
Jr

call
ld
or
ret
inc
inc
Jr

call
ld
ld
or
jr
Cp
Jr
CP
jr
inc
inc
jr

C]
z,F80F
b

hl
F7CF

F7F9
a, (hl)
a

Z

hl

b
F7DF

F7F9
c,a

a, (h1)
a
z,F80F
C
z,F80F
2C
Z,F80F
hl

b

F7EA

BASIC 1.0

HD HEDENE MED DE HE HE DE DEDEDE DD HE DE DE DE DEEE DE DENON

F7F9
F7FA
F7FB
F7FD
F800
F801
F802
F805
F806
F807
F808
F809

D1

ES
0600
CDFBFF
D1

E5
21BABO
70

23

73

23

72

pop
push
ld
call
pop
push
1d
id
inc
1d
inc
1d

de

hi

b, 00
FFFB
de

hl
h1,BOB
(h1),b
hl
(hl),e
hl
(hl),d

Jp (de)

À pointeur sur pile du descripteur
longueur

adresse

-I11 222-

BASIC 1.0

F80A CDBAFB call FBBA

F80D El pop hl

F80E C9 ret

F80F ES push hl

F810 Où inc b

F811 05 dec b

F812 2812 jr z,F826

F814 2B dec hl

F815 7E ld a,(hl)

F816 FE20 cp 20 15!

F818 28F7 jr z,F811

F81A FEO9 CP 09 TAB

F81C 28F3 jr z,F811

F81E FEOD cp OD CR

F820 28EF jr z,F811

F822 FEOA cp OA LE

F824 28EB ir z,F811

F826 El pop hl

F827 C9 ret

PPTTTLIILIILILILLLILSILLLS LIL SSSR SE) sortir chaîne

F828 CDDAFB call FBDA aller chercher paramètres de chaîne
F82B C8 ret Z chaîne vide ?

F82C 1A ld a, (de) aller chercher caractère
F82D 13 inc de augmenter pointeur

F82E CD6EC3S call C36E sortir caractère

F831 10F9 djnz F82C caractère suivant

F833 C9 ret

CETILSIILLILLLILILLILLLLLLSSISELSLSLLS SE) fonction Basic LOWER$
F834 0139F8 id bc,F839 convertir majuscules en minuscules
F837 180C jr F84s

CELLILILILLLLLLLLSLIL LL ELLL LS RSR EL) conversion ma] uscules en minuscules
F839 FE4 cp 41 "A

F83B D8 ret C

F83C FESB CP 5B 1'+1

F83E DO ret nc

F83F C620 add a, 20 'a'-'A

-I11 223-

F841

C9

ret

BASIC

EH DD DE MED DE DE DEEE DE DEEE DEEE DE DE HE E DEEE É E

F842
F845
F846
F849
F84A

F84D
F8u4E
F851
F852
F853
F854
F855
F858
F859
F85A
F85B
F85€
F85F
F860
F861

OT8AFF
C5
2AC2B0
7E
CD19FC

D5
CDDAFB
ET

C1

3C

3D
CABAFB
F5

14

15
CDF9FF
77

23

F1
18F1

ld

push

Id

id
call

push
call
pop
pop
inc
dec
Jp
push
ld
inc
call
1d
inc
pop
jr

bc,FF8A

bc

h1, (BOC2)

a, (hl)
FC19

de
FBDA
hl
_bc

a

a
z,FBBA
af

a, (de)
de
FFF9
(hl),a
hl

af
F854

LÉLLLSRSL EL LL LL LS LL LE LLLLLS LL LLE]

F863
F864
F865
F868
F869
F86B

F86E

F871
F872
F873
F874
F877

ES

7E
2AC2B0
86
1E0F
DA94CA

CD19FC

ET
D5
ES
CDDAFB
48

push
ld
ld
add
Id
JP

call

pop
push
push
call
ld

hl

a, (hl)
h1, (BOC2)
a, (h1l)
e,0F
c,CA94

FC19

hi
de
hl
FBDA
c,b

1.0

fonction Basic UPPER$
convertir majusc. en minusc.

longueur de chaîne
réserver place, placer descripteur
de chaîne

aller chercher paramètres de chaîne

jp (bc), exécuter conversion

addition de chaîne
longueur de chaîne

plus longueur de deuxième chaîne
‘String too long’

sortir message d'erreur

réserver place, placer descripteur
de chaîne

aller chercher paramètres de chaîne

-III 224-

BASIC 1.0

F878 EB ex de,hl
F879 E3 ex (sp),hl
F8/7A CDES8FB call FBE8
F87D EI pop hl

F87E E3 ex (sp},hl
F87F 78 1d a,b
F880 CD8BF8 call F88B
F883 D1 pop de

F884 79 Id a,C
F885 CD8BF8 call F88B
F888 C3BAFB Jp FBBA
F88B C5 push bc

F88C EB ex de,hl
F88D 4F ld c,a
F88E 0600 ld b, 00
F890 B7 or a

F891 C4F2FF call nz,FFF2 ldir
F894 EB ex de,hl
F895 C1 pop bc

F896 C9 ret

HN IE NE DEN HE DD EME DEHEDEHEHEEDEHE DEEE DE HE DEEE DE HE compa ra i son de chaî nes
F897 E5 push hl

F898 CDDAFB call FBDA aller chercher paramètres de chaîne
F89B 48 Id cb
F89C E1 pop hl

F89D DS push de

F89E CDES8FB call FBES
F8A1 El pop hi

F8A2 78 ld a,b
F8A3 B1 or C

F8A4 C8 ret zZ

F8A5 79 ld a,C
F8A6 B7 or a

F8A7 280C Jr z,F8B5
F8A9 78 ld a,b
F8AA B7 or a

F8AB 2809 Jr z,F8B6
F8AD 05 dec D

-111 225-

BASIC

F8AE OD dec C

F8AF 1A ld a, (de)
F8B0 13 inc de
F8B1 BE CP (h1)
F8B2 23 inc hl
F8B3 28ED jr Z, F8A2
F8B5 3F ccf

F8B6 9F sbc a,a
F8B7 CO ret nz
F8B8 3C inc a

F8B9 C9 ret

LES S SSSR RE LL ESS ESS SLLELLLLLLL LE)

F8BA CDCEF8 call  F8CE
F8BD D5 push de
F8BE CDI4FT call F114
F8C1 EB ex de,hl
F8C2 185€ jr F922
HD DE DD DEN DEDDEDEDEDEDEODEHEDEDEDEDÉDE DE DEDE DEEE DE
F8C4 CDCEF8 call  F8CE
F8C7 D5 push de
F8C8 CD19F1 call  F119
F8CB EB ex de,hl
F8CC 1854 Jr F92

AEDEHEDEDEDE DD DE DE DE DD D DEDE DEEE DE DE DEEE EDEN al ler

F8CE CDFBCE call CEFB
F8D1 CD53FF call FF53
F8D4 CDSSDD call DD55
F8D7 9F sbc a,a
F8D8 DC67CE call c,CE67
F8DB FE11 cp 11
F8DD D29CFA jp nc, FA9C
F8E0 47 ld b,a
F8E1 CD37DD call DD37
F8Eu 29 db 29
F8E5 EB ex hl,de
F8E6 79 Id a,C
F8E7 C3AOFS jp F5AO

1.0

comparer caractère première chaîne
avec seconde chaîne
identique, alors continuer compar.

fixer flags pour résultat

fonction Basic BIN$
aller chercher arguments

convertir en chaîne binaire
accepter chaine

fonction Basic HEXS$
aller chercher arguments

convertir en chaîne hexa
accepter chaîne

chercher argument pour BIN$ et HEX$
aller chercher expression

et placer sur pile Basic

virgule suit ?

0 comme défaut

oui, aller chercher
supérieur égal 17 ?
‘Improper argument

valeur 8 bits

tester si encore un caractère

DL

libérer place dans pile Basic

-I11 226-

BASIC

DE DE DIE DE DEEE NEED DE DEN DEEE DD DE DE DEEE EE DE

F8E À
F8ED
F8EE
F8F1
F8F4
F8F5
F8F8

F8FB
FSFE
F8FF
F900
F901
F904
F905
F906
F909
F90A
F90B
F90C
F9OF
F911
F912
F913
F915
F916
F919

F91B

CD37DD
28
CDFBCE
CD37DD
2C
CD53FF
CD9FCE

CD37DD
29

E5

79
CDAOF5
D5

79
CD4BFF
D1

78

B7
CUBAF3
300À
78

B7
2006
79
CD9FEE
1807

C39CFA

call
db
call
call
db
call
call

call
db
push
id
call
push
ld
call
pop
ld
or
call
Jr
id
or
Jr
ld
call
Jr

JP

DD37

28

CEFB

DD37

2C

FF53
CE9F

DD37

29

hl

a,C
F5AO

de

a,C
FF4B

de

a,bD

a
nz,F3BA
nc,F91B
a,b

a
nz,F91B
a, C
EE9F
F922

FA9C

CELLES SES SSLEL LS LS LS LS LL LL LS SSL SL)

F9TE
F91F
F922
F923

F926
F927
F928

ES
CDQDEE
ES

O1FFFF

03
7E
23

push
call
push
Id

inc
Id
inc

hl

EE9D

hl
bc,FFFF

bc
a, (hl)
hl

1.0

fonction Basic DEC$
tester si encore un caractère
‘(*,dé]à produit avec appel fonction
aller chercher expression
tester si encore un caractère
et placer sur pile Basic

aller chercher expression et
paramètres chaîne
tester si encore un caractère
LS ei

longueur
libérer place dans pile Basic

longueur
accepter variable

tester si caractère de formatage
‘Improper argument’
‘Improper argument’

formater nombre
accepter chaîne

’Improper argument”

fonction Basic STR$

convertir nombre en chaîne

compteur pour longueur de chaîne sur
-]

augmenter compteur
aller chercher caractère

-II1 227-

BASIC 1.0

F929 B7 or a
F92A 20FA Jr nz,F926
F92C 79 id a,C

F92D CD19FC call FC19

F930 E1 pop hl

F931 B7 or a

F932 DS push de

F933 C4F2FF call nz, FFF2
F936 D1 pop de

F937  CDBAFB call FBBA
F93A El pop hl

F93B C9 ret

LÉSLELLLISSLLLLLLLESLS LS LS LS LL LEE)

F93C CDE9F9 call F9E9
F93F  0E00 id c,00
F941 182A jr F96D

MH DEEE DE HE DD DE CNED EDEN DEEE DEN

F943 CDE9F9 call F9E9

F946 A ld a, (de)
F947 90 sub b

F948 UF id c,a
F949 1822 Jr F96D

LÉLSSS SELS LEE LL LL SL LES LS .;;L,)),E:)

F9uB CD37DD call DD37

FQUE 28 db 28
F9uF  CDE9FS call F9E9
F952 78 ld a,b
F953 B7 or a

F954  CA9CFA Jp z, FA9C
F957 O5 dec b

F958 48 ld c,b
F959 DS push de
F95SA C5 push bc

F95B CDFBF9 call F9FB

F9SE C1 pop bc

octet nul ?
non, prochain caractère
longueur de chaîne dans a

réserver place, placer descripteur

de chaîne

ldir

fonction Basic LEFTS$
amener chaîne et nombre 8 bits
à partir de position 0

fonction Basic RIGHT$

amener chaîne et nombre 8 bits
longueur de chaîne

moins paramètre

donne position de départ

fonction Basic MID$

tester si encore un caractère
a 62

amener chaîne et nombre 8 bits

‘Improper argument‘

aller chercher 3ème argument (défaut

= 255)

-I11 228-

F95F
F960
F961
F962
F964
F966
F967
F968
F96A
F96B
F96C
F96D
F970
F971
F972
F973
F974
F975
F976
F978
F979
F97B
F97C

F97F
F980
F983
F984
F985
F987
F988
F989
F98A
F98B
F98E
F991
F9g92

E3
JE
gi
0600
3805
BB
47
3801
43
EB
ET
CD37DD
29
E5
EB
7E
B8
78
3003
7E
0E00
F5
CD19FC

D5
CDE8FB
EB

D1
0600
09

F1

UF

B/
C4F2FF
CDBAFB
ET

C9

ex
ld
sub
ld
jr
CP
1d
jr
ld
ex
pop
call
db
push
ex
id
CP
ld
jr
id
id
push
call

push
call
ex
POP
ld
add
pop
ld
or
call
call
pop
ret

BASIC

(sp},hl
a,(h1)
C
b,00
c,F96B
e
b,a
c,F96B
b.e
de,h1i
hl
DD37
29
hl
de,hl
a, (h1l)
D
a,b
nc,F97B
a,(hl)
c,00
af
FC19

de
FBE8
de,hl
de

b, 00
hl,bc
af
C,a

a
nz,FFF2
FBBA
nl

HER GED EEE EHOHE DEEE EME EE DEN DE EH EURE NE E

F993

CD37DD

call

DD37

1.0

tester si encore un caractère
de

réserver place, placer descripteur
de chaîne

ldir

instruction Basic MID$
tester si encore un caractère

-I11 229-

F996
F998
F99A
F99D
F9SE
F99F
F9A2
F9A3
F9AG
F9A7

F9AA
FSAB

F9AE
F9AF
F9B2
F9B3
F9B6
F9B7
F9B8

F9BB
F9BC
F9BD
F9BE
F9BF
F9CO
F9C2
F9C3
F9C4
F9CS
F9C8
F9C9
F9CA
F9CC
F9CD
F9CE
FQ9CF
F9D0

28
CD86D6
CD3CFF
ES
EB
CD21FB
E3
CD37DD
2C
CD6DCE

47
CDFBF9

UB

CD37DD

29

CD37DD

EF

C5
CD9FCE

78
C1

E3

oc

oD
2825
F5

7E

90
DAQCFA
3C

B9
3801
79

4F

78

3D

23

db
call
call
push
ex
call
ex
call
db
call

ld
call

Id
call
db
cal]
db
push
call

ld
pop
ex
inc
dec
Jr
push
1d
sub
jp
inc
cp
Jr
1d
ld
1d
dec
inc

BASIC

28
D686
FF3C
hl
de,hl
FB21
(sp),hl
DD37
2C
CE6D

b,a
F9FB

c,e
DD37
29
DD37
EF
bc
CES9F

a,b

bc
(sp),hl
C

C
Z,F9Æ7
af

a, (hl)
b
c,FA9C
a

C
c,F9CD
a,C
c,a
a,b

a

hl

1,0

UC?
aller chercher variable
type ‘chaîne’, sinon ‘Type mismatch'”

tester si encore un caractère
aller chercher valeur 8 bits non
nulle

aller chercher 3ème argument (défaut
= 255)

tester si encore un caractère
)?
tester si encore un caractère

1!

aller chercher expression et
paramètres de chaîne

’Improper argument”

-111 230-

F9D1
F9D2
F9D3
F9D4
F9D5
F9D6
F9D7
F9D8
F9D9
F9DA
F9DB
F9DC
F9DD
F9DF
F9E0
FE
FSE3
FSE4
F9E7
F9E8

86
23
66
6F
8c
95
67
F1
47
EB
79
B8
3801
78
UF
0600
B7
CUF2FF
El
co

add
inc
ld
ld
adc
sub
1d
pop
id
ex
ld
cp
jr
ld
1d
id
or
call
pop
ret

BASIC

a,(hl)
pl

h, (h1)
1,a
a,h

1

h,a

af

b,a
de,hl
a, C

LÉLLSSSS SSI LLLELS SELLE LL LES L LE ST), ,,)

F9E9
FSEC
FEF
F9FO
F9F1
F9F4
F9F5
F9F8
F9F9
FOFA

CDASCE
CD37DD
2C
E5
2AC2B0
E3
CD67CE
47
D1
C9

call
call
db
push
ld
ex
call
id
pop
ret

CEAS
DD37

2C

hi

h1, (BOC2)
(sp),hl
CE67

b,a

de

EDEN DE DE DEE DEN DEEE DE DEN END DE DE DE DEEE DEEE DEN

F9FB
F9FD
F9FE
FAOO
FAO1
FAO4

1EFF
7E
FE29
c8
CD37DD
2C

1d
Id
cp
ret
call
db

e,FF
a, (h1)
29

zZ

DD37
2C

1.0

ldir

amener
amener
tester

1!
4

amener

amener
défaut

737

tester

1
,

-111 231-

chaîne et valeur 8 bits
expression chaîne
si encore un caractère

valeur 8 bits

3ème argument pour MID$

255

si encore un caractère

BASIC

FAOS CD67CE call CE67
FAO8 SF ld e,a
FAO9 C9 ret

EREOHODEHEDEME EH DEEE DEDE DE EH DE DE DE DEHE DEEE EX

FAOA  CDDAFB call FBDA

FAOD  C3OAFF JP FFOA

DEHDEDDEDE DE DD HE DH DE DE DEME DEEE DEN DE DE DE HE GENE DEEE

FA10 CD/OFA call FA70
FA13  C3OAFF jp 7 FFOA

HOMME DEEE DE HE DE DE DH EEE NE GE DEEE

FA16 CD92FA call FA92
FA19 F5 push af
FATA GEO ld a, 01
FAIC CD19FC call FC19
FATF F1 POP af
FA20 12 ld (de),a
FA21 C3BAFB Jp FBBA

RONDE DEEE EDEN HD DE DE HE HEDEDEDE DEEE DEEE DEN DE DEN

FA24 ES push hi

FA25 CD2AFA call FA2A
FA28 EI pop hl

FA29 C9 ret

FA2A CD39C4 call C439
FA2D 38EA Jr c,FA19
FA2F AF Xor a

FA3O 32BABO 1d (BOBA),a
FA33 C3BAFB jp FBBA

DD NEED NN ND DEMEDE EDEN HENE DEEE DE DE DEN DEEE

FA36  CD67CE call CE67
FA39 UF ld c,a
FA3A CD37DD call DD37

1.0

amener valeur 8 bits

fonction Basic LEN
amener paramètres de chaîne,
longueur dans a
accepter contenu accu comme nombre
entier

fonction Basic ASC

code ASCII du premier caractère
accepter contenu accu comme nombre

entier

fonction Basic CHR$
CINT, < 256

longueur 1
réserver place, placer descripteur

placer code ASCII comme chaîne

INKEYS

KM READ CHAR

touche enfoncée ?

non

descripteur de chaîne, longueur

STRING$

amener valeur 8 bits, longueur

tester si encore un caractère

-III 232-

FA3D
FASE
FA4T
FALL
FA4S
FAUG
FAUG
FAUB
FALE

FA5O
FA53

FA5S4
FA55

2C
CDFBCE
CD37DD
29

ES
CDASFF
2805
CD92FA
1803

CD70FA
41

4
1807

db
call
call
db
push
call
jr
call
jr

call
1d

ld
jr

BASIC

2C
CEFB
DD37
29

hl
FF45
z, FA50
FA92
FA5S3

FA70
b,c

c,a
FASE

EE EOHEHE DE DE DE DE HD DEEE HE EH DE DEEE HEHEHE HE EE

FA57
FASA
FASB
FASD
FASE
FASF
FA62
FA63
FA6E
FA66
FA67
FA68
FA69

FAG6B
FAGE
FAGF

CD92FA
47
0E20
E5

78
CD19FC
O4

05
2805
79

12

13
18F8

CDBAFB
ET
C9

cali
1d
1d
push
ld
call
inc
dec
jr
ld
ld
inc
jr

call
pop
ret

FA92
b,a

c, 20
hl

a,b
FC19

()

D
z,FA6B
a,C
(de),a
de
FA63

FBBA
hi

ROOMOHONHEMMEMEMEME DE ED DE DE DE EDEN DEN DD DEEE EEE E

FA70
FA73
FA75
FA76

CDDAFB
2827
1A

C9

call
Jr
id
ret

FBDA
Z, FA9C
a, (de)

1.0

1 ot
,

aller chercher expression

tester si encore un caractère

1)!

tester si chaîne
oui

CINT, < 256

amener code ASCII du premier

caractère

fonction Basic SPACES
CINT, < 256

15!

réserver place, placer descripteur

amener code ASCII

amener paramètres de chaîne
chaîne vide,
code du premier caractère

-III 233-

‘Improper argument’

BASIC 1.0

DOHOMOUENEDEMEUE ED DE DE DE DE DE DE D DE DE DE DE D DE DE DE DE DE DE DE JE fonction Basic VAL
FA77 CDDAFB call FBDA amener parametètre de chaîne
FA7A CAOAFF Jp Z, FFOA chaîne vide, alors zéro
FA7D EB ex de,h1l

FA7E ES push hl

FA7F SF ld e,a

FA80 1600 ld d,00

FA82 19 add hl,de

FA83 GE 1d e,(hl)

FA84 72 ld (h1),d

FA85 E3 ex (sp},hl

FA86 D5 push de

FA87 CDA3EC call ECA3

FA8SA D1 pop de

FA8B EI pop h]

FA8C 73 1d (hl),e

FA8SD D8 ret C

FA8E 1EO0D ld e,0D ‘Type mismatch'

FA9O 180C Jr FASE sortir message d'erreur
DOMEDEDEDE DE DD DEEE D DE DE DE DE DD HE EE DE DE DEEE NME DE CINT, Test < 256

FA92 ES push hl

FA93 CDS8DFE call FE8D CINT

FA96 EB ex de,hl

FA97 E1 pop hl

FA98 7A ld a,d Hi-Byte

FA99 B7 or a zéro ?

FA9JA 7B 1d a,e charger Lo-Byte

FAIB C8 ret z

FA9C 1E05 id e,05 ’Improper Argument‘
FAJE  C39UCA Jp CA9g4 sortir message d'erreur
DEMDEDE DD MED DE DE D DE DE DE DEDE DE DE DE DE DE DE DEEE DE DEN DE UEE fonction Basic INSTR
FAAT CDFBCE cal] CEFB aller chercher expression
FAA4 CD4SFF call FF4S tester si chaîne

FAA7  0E01 ld c,01 position de départ défaut 1
FAAS  280F Jr Z, FABA

FAAB CD92FA call FA92 CINT, < 256

FAAE B7 or a

FAAF  CAOCFA Jp Z, FA9C ‘Improper argument’

-III 234-

FAB2
FABS
FAB6
FAB7
FABA
FABD
FABE
FABF
FAC2
FAC3

FACE
FAC9
FACA
FACB
FACC
FACF

FAD2
FAD3

FAD&
FAD5
FAD6
FAD7
FADA
FADB
FADC
FADD
FADE
FADF
FAEO
FAET
FAE3
FAE4
FAES
FAE6
FAE7
FAES
FAE9

UF
CD37DD
2C
CDASCE
CD37DD
2C
ES
2AC2B0
E3
CD9FCE

CD37DD
29
E3
79
CDD4F A
CDOAFF

EI
cg

F5
48
D5
CDE8FB
EI
F1
ES
6F
60
78
BD
382D
2D
7D
83
5F
8A
93
57

ld
call
db
call
call
db
push
1d
ex
call

call
db
ex
1d
call
call

Pop
ret

push
ld
push
call
pop
pop
push
1d
ld
ld
cp
jr
dec
id
add
ld
adc
sub
ld

BASIC

C,a

DD37

2C

CEAS

DD37

2C

hl

h1, (BOC2)

(sp),hl
CE9F

DD37
29
(sp},hl
a,C
FAD4
FFOA

pl

1.0

tester si encore un caractère
g LA

amener expression chaîne
tester si encore un caractère

CR]
,

amener expression et paramètres
chaîne
tester si encore un caractère
Up

accepter contenu accu comme nombre
entier

-111 235-

FAEA
FAEB
FAEC
FAED
FAEE
FAFO
FAF1
FAF2
FAFH
FAF5
FAF6
FAF7
FAF8
FAF9
FAFA
FAFC
FAFD
FAFE
FBOO
FBO1
FBO2
FBO4
FBO5
FB06
FBO7

FB0O9
FBOA
FBOB
FBOC
FBOD
FB0E
FB10
FB11
FB12

FB13
FB14
FB15
FB16

78
95
47
79
D601
7D
3C
381D
E3
C5
D5
ES
TA
BE
200D
23
OD
2815
13
05
20F4
E
D1
C1
1807

Er
D1
C1
13
05
20E5
AF
Di
C9

ET
Di
C1
E1

ld
sub
ld
1d
SUD
ld
inc
ir
ex
push
push
push
ld
CP
jr
inc
dec
jr
inc
dec
jr
POP
POP
pop
Jr

pop
POP
pop
inc
dec
jr

xor
pop
ret

pop
pop
pop
pop

BASIC 1

a,b

b,a
a,C

01

a,

a
c.FB11
{sp},hl
bc

de

hl

a, (de)
(h1)
nz,FB0O9
h1

C
z,FB13
de

b

nz, FAF8
hl

de

bc

FB10

hl
de
bc
de
D
nz, FAFS
a
de

hi
de
bc
nl

-IIT 236-

.0

FB17
FB18
FB19
FB1A

FB1B
FBIE

FB21
FB22
FB23
FB24
FB25
FB26
FB27
FB28
FB29
FB2C
FB2D

FB2E
FB31
FB34
FB36
FB39
FB3C
FB5D
FB3E
FB3F
FB4O
FB4
FB4E
FB4S
FB46

FB49
FB4C
FB4F
FB52

7C
90
3C
cg

112EFB
C374DA

ES
7E
23
LE
23
46
EB
B7
C42EFB
ET
cg

2A8DB0
CDBEFF
3007
2A8FB0
CDBEFF
DO

EB

2B

2B

ÈS
CD8FFB
EB

ET
C3A6FB

2AC2B0
11BABO
CDB8FF
D8

id

sub
inc
ret

ld
jp

push
1d
inc
1d
inc
id
ex
or
call
pop
ret

1d
call
jr
Id
call
ret
ex
dec
dec
push
call
ex
pop
jp

1d
1d
call
ret

BASIC

de, FB2E
DA74

hl

a, (hl)
hl
c,(h1l)
hl

b, (h1)
de,hl

a

nz, FB2E
hl

h1, (BO8D)
FFBE
nc,FB3D
h1, (BO8F)
FFBE
nc
de,hl
hl
hl
hl
FB8F
de,hl
hl

FBAG

h1, (BOC2)
de,BOBA
FFB8

C

1.0

début des chaînes

comparer

hl <> bc

fin des chaines

comparer

descripteur de chaîne de (de) dans

(h1)

descripteur de chaîne

comparer

-I11 237-

hi <> bc

hl <> de

FB53
FB56

CD8FFB
C3BAFB

call
jp

BASIC 1.0

FB8F
FBBA

HD DEDE DE DE DE DE DE DEEE DE DEEE DE DE HE DE DE HE JE DD DE DE EEE KE

FB59
FB5C
FB5D
FB5E
FB5F
FB61
FB62
FB63
FB64
FB65
FB68
FB6B
FB6D
FB70
FB73
FB75
FB78
FB7B
FB7D
FB7E
FB7F
FB82
FB85
FB87
FB88

FB8B
FB8C
FB8F
FB90
FB93
FB94
FB95
FB97
FB98
FB99

2AC2B0
ES

7E

B7
2826
23

5E

25

56
2A8TAE
CDB8FF
301E
2A8FB0
CDB8FF
3816
2A83AE
CDB8FF
300A
ET

ES
119CB0
CDB8FF
2004
ET
C3FFFB

ET
CDFFFB
7E
CD19FC
D5

LE
0600
23

7E

23

Id
push
ld
or
jr
inc
ld
inc
1d
id
cal]
jr
1d
call
jr
ld
call
Jr
pop
push
1d
call]
Jr
pop
Jp

pop
call
1d
call
push
Id
Id
inc
ld
inc

hl, (BOC2) pointeur sur descripteur de chaîne
hl

a,(hl) longueur de chaîne

a

Z,FB87 chaîne vide ?

hl

e,(hl)

hl longueur de chaîne dans de
d,(h1l)

hl,(AE81) début de programme

FFB8 comparer hl <> de
nc,FB8B chaîne avant le programme
h1, (BO8F) fin des chaînes

FFB8 comparer hl <> de

c,FB8B chaîne en dehors de zone des chaînes
hl, (AE83) fin du programme

FFB8 comparer hl <> de

nc, FB87 chaîne dans programme

hl

hi

de, BO9C

FFB8 comparer hl <> de

nz, FB8B

hi

FBFF

hl

FBFF

a, (hl)

FC19 réserver place, placer descripteur
de

c,(h1) longueur de chaîne dans c
b,00 Hi-Byte longueur -zéro

hi

a,(hl)

hl adresse de chaîne dans hl

-111 238-

FBSA
FB9B
FB9C
FB9D
FB9E
FBAI
FBA2
FBAS

66
6F
78
B1
CHF2FF
D1
21BABO
C9

Id
ld
1d
or
call
pop
id
ret

h,(h1)
],a
a, D

C
nz,FFF
de
h1,BOB

BASIC 1.0

2 idir, transférer chaîne

Â descripteur de chaîne

EEE DEAD EME EE EKEEE jescripteur de chaîne de (de) dans (hl)

FBA6 1A id a, (de)

FBA7 13 inc de

FBA8 77 ld (hl),a

FBA9 23 inc hl

FBAA JA 1d a, (de)

FBAB 13 inc de

FBAC 77 ld (h1),a

FBAD 23 inc hi

FBAE JA id a, (de)

FBAF 13 inc de

FBBO 77 Id (hl),a

FBB1 23 inc hl

FBB2 C9 ret

LELLLL ESS LL LS LSLELSL ISLE SRI LLLISS LS] initialiser pile du descripteur

FBB3 219CB0 ld h1,B09C

FBB6  229AB0 1d (BOSA),h1 pointeur sur pile descripteur pour
chaînes

FBB9 C9 ret

LELLELELL LL EL LELSELSLLLSLLSLSLLSSS LS

FBBA 3E03 1d a,03 chaîne’

FBBC 32C1B0 ld (BOC1),a comme type de variable

FBBF  2A9ABO ld hi, (BOSA) pointeur dans pile descripteur

FBC2 22C2B0 id (BOC2),hl

FBC5 11BABO ld de, BOBA descripteur de chaîne

FBC8 CDB8FF call FFB8 comparer hl <> de

FBCB 1E10 id e,10 ‘String expression too complex’

FBCD CA9ACA JP z, CA94 sortir message d'erreur

-111 239-

FBDO
FBD3

FBD6
FBD9

11BABO

CDAGFB

229AB0
cg

ld
call

ld
ret

BASIC

de, BOBA
FBA6

(B0O9A),h1

DD DEDEHEDE DE DE DE DEEE DE DE DE HE HE MEHR EE EDEN HE

FBDA
FBDB
FBDE
FBE1
FBEL
FBES

FBE6
FBE7

FBE8
FBEB
FBEC
FBED
FBEE
FBF1
FBF4
FBF6
FBF7
FBF9
FBFA
FBFD
FBFE

FBFF
FCO0
FCO1
FCO2
FCO3
FCO4
FCO5
FCO6
FCO7

ES
CD3CFF
2AC2B0
CDE8FB
ET

78

B7
C9

CDFFFB
CO

D5

1B
2A8DB0
CDB8FF
2007
58
1600
19
228DB0
D1

C9

ES
46
23
7E
23
66
6F
E3
EB

push
call
ld
cali
pop
Id”

or
ret

cali
ret
push
dec
1d
call
Jr
id
1d
add
ld
pop
ret

push
ld
inc
1d
inc
ld
ld
ex
ex

hl
FF3C
h1, (BOC2)
FBE8
hl
a, D

FBFF

nz

de

de

h1, (BO8D)
FFB8
nz,FBFD
e,b

d,00
hl,de
(B08D),h1
de

hl
b,(h1)
hl
a,(hl)
hl

h, (hl)
l,a
(sp),hl
de,hl

1.0

descripteur de chaîne

descripteur de chaîne de (de) dans
(h1)
pointeur dans pile descripteur

amener paramètres chaîne

type ‘chaîne’, sinon ‘Type mismatch”

adresse du descripteur de chaînes

longueur dans a et b, adresse dans
de

début des chaînes
comparer hl <> de

début des chaînes

-III 240-

FCO8
FCOB
FCOC
FCOD
FCOE
FC11
FC13
FC16
FC17
FC18

2ASABO
2B

2B

2B
CDB8FF
2003
229AB0
EB

D1

C9

ld
dec
dec
dec
call
jr
ld
ex
pop
ret

BASIC

h1, (BO9A)
nl

hl

hl

FFB8
nz,FC16
(BO9A),h1
de,hl

de

HO HOMOHOHOHEHEHE HE DE HE DE EH HE DEHEHEHE MED EHEDEHEDEHEEE JE

FC19
FCIA
FC1B
FCIC
FC1D
FC20
FC21
FC24
FC25
FC26
FC27
FC28
FC29
FC2A
FC2B
FC2C

F5
C5
E5
F5
CDD1F5
F1
21BABO
77
23
73
23
72
E1
C1
F1
C9

push
push
push
push
call
pop
1d
ld
inc
ld
inc
Id
pop
pop
pop
ret

af

bc

hl

af
FSD1
af
h1,BOBA
(hl),a
hl
(hl),e
hl
(h1),d
hl

bc

af

LRSSLELELLLL ES ESS EL LL EL. LLLE.E,,)L)

FC2D
FC30
FC32
FC35
FC38
FC3B

CDUSFF
2006

CDDAFB
CD3EFC
CD28F6
C360FE

call
jr
call
call
call
jp

FF4S
nz,FC38
FBDA
FC3E
F628
FE60

LELS LES SSSR SELS LE LES SSL LS LLLLLLS ES)

FC3E
FC3F

C5
D5

push
push

bc
de

1.0

pointeur sur pile descripteur

comparer hl <> de

pointeur sur pile descripteur

réserver place, placer descripteur

longueur de chaîne
réserver place dans zone chaînes

descripteur de chaîne

longueur de chaîne

adresse de chaîne

fonction Basic FRE
tester si chaîne
non

Garbage Collection
calculer place libre en mémoire

Garbage Collection

-III 241-

FC4O

FC4T

FC44
FC47
FC4A
FC4D
FC50
FC53
FC56
FC59
FC5A
FC5B
FC5SD
FCSE
FCSF
FC60
FC61
FC62
FC63
FC65
FC68
FC69
FC6A
FC6B
FC6E
FC6F
FC70
FC71
FC72
FC73
FC74
FC75

FC77
FC78
FC79
FC7A

FC7B
FC7E

ES
2A8FB0
228DB0
210000
22BDB0
2A89AE
22BFB0
CD/BFC
2ABDBO
7C

B5
2814
56

2B

5E

ES

2B

LE
0600
2A8DB0
EB

09

2B
CDFSFF
13

ET

75

23

72

1B

EB
18CD

E1
Di
C1
C9

219CB0
EDSB9ABO

push
ld
1d
ld
ld
id
id
call
id
ld
or
Jr
ld
dec
1d
push
dec
ld
id
ld
ex
add
dec
call
inc
pop
1d
inc
id
dec
ex
Jr

Pop
pop
pop
ret

id
ld

BASIC 1.0

h]l

h1, (BO8F) fin des chaînes
(BO8D),h1 début des chaînes
h1,0000

(BOBD),h1

hl, (AE89) fin des tableaux
(BOBF),hl

FC7B

h1, (BOBD)

a,h

l

z,FC77

d,(h1)

hl

e,(h!)

hi

hl

c,(hl)

b, 00

h1, (BO8D) début des chaînes
de,hl

hl,bc

hl

FFFS lddr

de

hl

(hl),e

hl

(hl),d

de

de,hl

FC

hl

de
bc

h1,B09C
de, (BOGA) pointeur sur pile descripteur

- II] 242-

BASIC 1.0

FC82 CDB8FF call FFB8 comparer hi <> de
FC85 280F Jr z,FC96

FC87 7E ld a, (hl)

FC88 23 inc hi

FC89 4E id c,(h1)

FC8A 23 inc hl

FC8B 46 ld b, (h1)

FC8C ES push hl

FC8D EB ex de,hl

FC8E B7 or a

FC8F CAQCFC call nz,FC9C

FC92 El POP hl

FC93 23 inc hl

FC94 18E8 jr FC7E

FC96 119CFC ld de, FC9C

FC99 C374DA JP DA74

FC9C 2A8DB0 Id h1, (B08D) début des chaînes
FC9F  CDBEFF call FFBE comparer hl <> bc
FCA2 D8 ret C

FCA3 2ABFBO 1d h1, (BOBF)

FCA6G CDBEFF call FFBE comparer hl <> bc
FCA9 DO ret nc

FCAA EB ex de,hl

FCAB 22BDBO id (BOBD),h1

FCAE EDU3BFBO 1ld (BOBF),bc

FCB2 C9 ret

LÉLLÉSLSSIÉRSÉESSLSLSLLÉSSLLSLLSELLLS LE)

FCB3 CD2DFF call FF2D amener résultat numérique
FCB6 D252BD Jp nc,BD52 virgule flottante
FCB9 CDA3BD call BDA3 entier

FCBC 22C2B0 1d (BOC2),h]

FCBF 21C3B0 1d h1,BOC3

FCC2 C9 ret

FCC3 CDC2FE call FEC2 UNT

FCC6 21C3B0 ld h1,BOC3

FCC9 C3AG6BD jp BDA6

-II1 243-

BASIC

LÉLLSILLLLLSSSSELLLLLISSSS ESS SES SE ES)

FCCC CDISFE
FCCF 3009

FCD1 CDACBD
FCD4  DAODFF

FCD7 CDAFFE
FCDA CD58BD
FCDD D8

FCDE C3F3CA

cal]

Jr

cal!
JP

call
call
ret
Jp

FES

nc, FCDA

BDAC
c,FFOD

FEUF
BD58
€

CAF3

DE DE DE DEEE DEDE DD HD NEED DE DE DE DE DEEE DEEE DEEE HE

FCET CDISFE
FCE4 3009
FCE6 CDB2BD
FCE9 DAODFF
FCEC CD4FFE
FCEF  CDSEBD
FCF2 D8
FCF3 18E9

call
Jr
call
Jp
call
call
ret
jr

FE15
nc, FCEF
BDB2
c,FFOD
FEUF
BD5E

C

FCDE

HEDM DD HE HE DE DE DEEE HEDEDE DE DE DE HE DEDE DE DE DE HEDE DEEE DEN

FCFS CDISFE

call

FE15

1.0

opérateur Basic ‘+’
tester type des opérandes
virgule flottante ?
addition entiers hl :=h1l + de
pas de dépassement, accepter
résultat dans hl
convertir en virgule flottante
addition avec virgule flottante
pas de dépassement, ok
‘Overflon'

BASIC-Operator ‘-"

tester type des opérandes

virgule flottante ?

soustraction entiers hl := de - hl
pas de dépassement, résultat dans hl
convertir en format virgule flott.
soustraction virgule flottante

pas de dépassement, ok

’Overfion'

opérateur Basic ‘*’
tester type des opérandes

-IIT 244-

BASIC 1,0

FCF8 3009 Jr nc, FD03
FCFA  CDB5SBD call BDB5

FCFD  DAODFF Jp c,FFOD
FDOO CD4FFE call FEUF

FDO3 CD61BD call BD61

FDO6 D8 ret C

FDO7 18D5 jr FCDE

EEE EEE HE EME DE EH NE EN

FDO9 CDI5FE call FE15
FDOC DACHBD Jp c,BDC4
FDOF C36ABD jp BD6A

HHOHOMEME EME ME HE HE MEME DEEE DEMO DE HE EH HE DE HE DE EN NE

FD12 3AC1B0 ld a, (BOC1)
FD15 B1 or C

FDI16 FEO2 CP 02

FD18 2005 jr nz,FD1F
FD1A  CD4FFE call FEUF
FD1D 1803 jr FD22
FDIF CDI5FE call FE75
FD22 EB ex de,h}
FD23 DS push de

FD24 CD64BD call BD64
FD27 D1 pop de

FD28 F5 push af

FD29 010500 id bc,0005
FD2C CDF2FF call FFF2
FD2F F1 pop af

FD3G D8 ret (a

FD31. CAEACA JP Z, CAEA
FD34 C3F3CA jP CAF3

CELL LLLLLLSSILLSSSISSRSSRR SSSR NES S ES
FD37 CDSAFE call FE9A
FD3A EB ex de,hi
FD3B CDB8BD call BDB8

virgule flottante ?

multiplication entiers avec signe
pas de dépassement, accepter

résultat dans hl

convertir en virgule flottante

multiplication à virgule flottante

'Overflow’

comparaison arithmétique
tester type des opérandes
comparaison entiers

comparaison virgule flottante

opérateur Basic ‘/"

type de variable

opérandes entiers en virgule
flottante

tester type des opérandes

division virgule flottante

Idir

ok ?

‘Division by zero’
‘Overflon’

opérateur Basic ‘Backslash”

division entiers avec signe

-III 245-

FD3E
FD41
FD43
FD46

DAODFF
2810

210080
C360FE

Jp
jr
1d
Jp

BASIC 1.0

c,FFOD
z,FD53
h1,8000
FE60

EODEODEHDEDEDEHEDE DE MEDEHDEDEDDEDEDEDEDEDEDÉEEDEMEDÉDEEE E

FD49
FD4C
FD4D
FD50
FD55
FD55

CD9AFE
EB
CDBBBD
DAODFF
1E0B
C394CA

call
ex
call
jp
1d
JP

FE9A
de,hl
BDBB
c,FFOD
e,0B
CA94

PEN DEHEDE DE DD DEN DE DE DEEE DE DEEE DE DE JE HN DEEE Xe

FD58
FD5B
FDS5C
FD5D
FDSE
FD5F
FD60

CDSAFE
7B
A5
6F
7C
A2
C3OCFF

call
1d
and
ld
1d
and
JP

DE DE DE DE DE DE DEEE DEN DEDE DEEE DE DE DEEE DE DEEE DEEE

FD63
FD66
FD67
FD68
FD69
FD6A
FD6B

CDSAFE
7B

B5

6F

7A

Bu
18F3

call
ld
or
Id
1d
or
Jr

EEE DE DE DD DE DE DE D DE DE DEDE HE DEN DEN DE DEEE DE DE DE DE NE

FD6D
FD70
FD71
FD72
FD73
FD74
FD75

CDSAFE
7B

AD

6F

7C

AA
189

call
1d
xor
ld
id
xor
jr

accepter résultat dans hl
‘Division by zero’

opérateur Basic ‘MOD’

calcul MOD

accepter résultat dans hl
Division by zero’

sortir message d'erreur

opérateur Basic ‘AND’
h1 and de

opérateur Basic ‘OR’
hl or de

opérateur Basic ‘’XOR'

hl xor de

-IIT 246-

BASIC 1.0

EEE DEDENEHEE HE NE DEEE E HE NE DE DE MEME DÉMO XE JEJE NN

FD/77 E5 push hl
FD/8 CD8DFE call FE8D
FD/B 7D Id a, 1
FD7C 2F cpl a
FD/D 6F 1d 1,a
FD7E 7C ld a,h
FD/F 2F cpl a
FD80 CDOCFF call FFOC
FD83 El pop hl
FD84 C9 ret

LÉLLSLSELL LES LL LLLELLL LL LL LL, EL)

FD85 CDA3FD call FDA3

FD88 FO ret P
LÉELLILLLSS ESS LT ÉS RSS SL SLLELS ES ES)
FD89 E5 push hi

FD8A C5 push bc

FD8B CD2DFF call FF2D
FD8E 300D jr nc, FD9D
FD90 CDC7/BD call BDC7
FD93 22C2B0 id (BOC2),h1
FD96 D5 push de

FD97 DAGOFE call nc,FE60
FD9A D1 pop de

FD9B 1803 Jr FDAO

LÉLLILLLE LS ST SSL ÉIS SSD SI S LS LSL EEE ES)

FD9D CD6DBD call BD6D

FDAO C1 pop bc
FDA El pop hi
FDA2 C9 ret

DEEE DE DE DEEE MEN DEDE DE DEEE DEEE DE DE DE HE DEN DEEE

FDA3 CD2DFF call FF2D

FDA6 DACABD Jp c,BDCA
FDAY C5 push bc
FDAA CD/0BD call BD/70
FDAD C1 pop bc
FDAE C9 ret

opérateur Basic NOT
CINT

compléter Lo-Byte

compléter Hi-Byte

fonction Basic ABS
SGN

signe positif, terminé
inverser signe

amener résultat numérique
inversion de signe virgule flottante
inversion de signe entier

inversion de signe virgule flottante
inversion de signe virgule flottante

amener résultat numérique
SGN entier

SGN virgule flottante

-I11 247-

BASIC 1.0

PEER ROMOH EDEN NE DE DEEE DE DE DE DE DE DEEE HE NE

FDAF
FDBO
FDB1
FDB4
FDB5
FDB8
FDB9
FDBB
FDBC
FDBD

FDCO
FDC3

ES

79
CD4BFF
D1
CD2DFF
78
300B
B7

FO
CDGAFE

CDCEFD
C38DFE

push
ld
call
pop
call
Id
jr
or
ret
call

call
jp

hl

a,C
FF4B

de

FF2D
a,b

nc, FDC6
a

p

FE6A

FDCE
FE8D

HD DH HE HEDE DE DE HE HE HE DE DEN DE DEEE EEE

FDC6
FDC7
FDC9

FDCC

B7
2005
1149BD

1826

or
jr
ld

Jr

a
nz, FDCE
de,BD49

FDF4

HEHEHEDE DE HE DEHEDE HE DD DÉDEHE DEEE DD DÉDE HE DENON JE

FDCE
FDCF
FDDO
FDD1

FDD4

FDD7
FDD8
FDD9
FDDA
FDDC

FDDF
FDEO
FDE

D5

C5

78
CDS5BD

DCU9BD

78

C1

D1

3008
CD43BD

AF
90
C355BD

push
push
ld

call

call

ld
pop
pop
Jr
call

xor
SUD
Jp

de
bc
a,b
BD55
c,BD49
a,b
bc
de
nc, FDE4
BD43
a
b
BD55

arrondir nombre

accepter type et valeur de variable

aller chercher résultat numérique
chiffres d'arrondissement
valeur à virgule flottante ?

arrondi après la virgule? terminé
convertir valeur entière en virgule
flottante

arrondir nombre

CINT

arrondir nombre à virgule flottante

chiffres d'arrondissement

différent zéro, alors arrondir
convertir virgule flottante en

entier

chiffres d'arrondissement
multiplier nombre à virgule
flottante par 10°a
convertir virgule flottante en
entier

convertir entier en virgule
flottante
inverser chiffres d'arrondissement
correspond division

multiplier nombre à virgule
flottante par 10°a

-I11 248-

BASIC 1.0

FDEL EB ex de,hl
FDES C3UEFF jp FFHE

EEE HE HOHOEHE MEN EE HE EEE ME EEE EHESS
FDES 114CBD ld de, BD4C
FDEB 1803 ir FDFO
HE HE MH MEME HE DH DE HE DE HE HEHEHHEEGEN
FDED 114FBD Id de, BD4F

FDFO CD2DFF call FF2D

FDF3 D8 ret C
FDF4  CDFBFF call FFFB
FDF7 DO ret nc

FDF8 3AC1B0 1d a, (BOC1)
FDFB CDOGFE call FE06
FDFE D8 ret C

FDFF  CDIDFF call FF1D

FEO2 78 1d a,D

FEO3  C343BD jp BD43
FE06 79 1d a;C

FEO07 FEO3 CP 03

FEO9 DO ret nc

FEOA 7E 1d a,(hl)
FEOB 23 inc hl

FEOC 66 1d h, (h1)
FEOD 6F Id l,a

FEOE  CDA9BD call BDA9
FE11 DO ret nc

FE12 C3ODFF Jp FFOD
FE15 79 ld a,C

FE16 FE0O3 CP 03

FE18 2832 ir 2, FE4C
FETA 3AC1B0O ld a, (B0OC1)
FE1D FEO3 CP 03

FE1F 282B jr Z,FE4C
FE21 B9 CP C

fonction Basic FIX
fonction FIX

fonction Basic INT
fonction INT

amener résultat numérique
entier ?

jp (de)

type de variable
type de variable dans c, pointeur
dans hi
convertir entier en virgule

flottante

‘chaîne ?

si positif, accepter signe de D

accepter résultat dans h]

‘chaîne’ ?

oui, ‘Type mismatch'
type de variable
‘chaîne’

oui, ‘Type mismatch'

-111 249-

BASIC 1.0

FE22 2817 Jr z,FE3B

FE24 300C Jr nc, FE32

FE26 ES push hl

FE27 21C1B0 ld h1,B0OC1 type de variable

FE2A 71 ld (h1l),c

FE2B 23 inc hl

FE2C CD63FE call FE63 convertir nombre entier en virgule
flottante

FE2F D1 pop de

FE30 B7 or a

FE31 C9 ret

FE32  CD63FE call FE63 convertir nombre entier en virgule
flottante

FE35 EB ex de,hl

FE36 21C2B0 ld h1,BOC2

FE39 B7 or a

FE3A C9 ret

FE3B EEO2 xor 02

FE3D 2805 jr z,FE44

FE3F EB ex de,hl

FEUO 21C2B0 ld h1,BOC2

FE43 C9 ret

FEUY GE Id e,(hl)

FEUS 23 inc hl

FEU6 56 ld d,(h1l)

FE47 2AC2B0 Id h1, (BOC2)

FEUA 37 scf

FEUB C9 ret

FE4C C3UOFF jp FF4O "Type mismatch'

ARRELENIRINNERPENRENERNEN opérandes entiers en virgule flottante

FEUF  2AC2B0 ld hl,(BOC2) premier opérande

FES2 CDGAFE call FEGA convertir

FE55 2A8BB0 1d h1, (BO8B) pointeur de pile Basio, second
opérande

FES8 CD63FE call FE63 convertir

-II1 250-

FESB
FESC
FESF

FE60
FE61

RERDEHE EDEN HE HE DE DD DE DE D DE HE DE DE DE DE DE NE NE

FE63
FE64
FE65
FE66
FE67
FE68

EB
21C2B0
C9

AF
1808

SE
23
56
2B
7A
1808

ex
id
ret

xor
Jr

ld
inc
ld
dec
1d
jr

BASIC 1,0

de,hl
h1,B0OC2

a
FE6B

e,(h1l)
hl
d,(h1)
hl

a,d
FE72

convertir en virgule flottante

convertir nombre entier en virgule flottante

OO EREEHEREEEEEX Convertir nombre entier en virgule flottante

FE6A
FE6B
FE6C
FE6F
FE71
FE72
FE73
FE74
FE75

FE78
FE79

RHONE DEEE HE DEMO

FE7C
FE7F
FE80
FE83
FE86
FE88
FE89
FE8A

7C

EB
21C1B0
3605
23

EB

F5

B7

FCC7BD

F1
C34OBD

22C2B0
EB
22C4B0
21C1B0
3605
23

AF
C343BD

1d

ex

ld

ld

inc
ex
push
or

call

pop
JP

id
ex
ld
ld
id
inc
xor
JP

a,h
de,hl
h1,B0C1
(h1),05
hl
de,h]
af
a

m, BDC7

af
BD4O

type de variable
"Real'

si négatif, inversion de signe
entier

convertir nombre entier en virgule
flottante

convertir valeur 4 octets en virgule flottante

(BOC2),hl
de,h1
(BOC4),h1
h1,BOC1
(h1),05
hl

a

BD43

Lo-Word

Hi-Word

type de variable

Real’

pointeur sur valeur 4 octets

convertir en virgule flottante

-III 251-

BASIC 1.0

LÉELL ESS SSL LSLSLSLLSLSLSLLSLLSLSLLSS fonction Basic CINT

FE8D CD93FE call FE93

FE90 D8 ret C

FE91 183F Jr FED2 ’Overflow'

FE93 CDASFE call FEAS

FE96 22C2B0 1d (BOC2),h1

FE99 cg ret

FE9SA 79 ld a, C

FE9B CDACFE call FEAC

FESE EB ex de,hl

FESF DCASFE call c,FEAS

FEA2 D8 ret C

FEA3 182D Jr FED2 ’Overflow'

FEAS 21C1B0 ld h1,B0C1 type de variable

FEA8 7E ld a,(hl)

FEA9 3602 Id (h1),02 ‘Integer’

FEAB 23 inc hi

FEAC FEO3 CP 03 ‘chaîne’ ?

FEAE  380D jr c,FEBD

FEBO  CAUOFF jp z,FF40 Type mismatch’

FEB3 C5 push bc

FEB4 CD46BD call BD46 convertir nombre à virgule flottante
en entier

FEB7 47 ld b,a

FEB8  DCA9BD call c, BDA9 accepter signe b dans nombre entier
hl

FEBB C1 pop bc

FEBC C9 ret

LÉRSSSELLLLLS SELS ESS LL L ELLES SL LES: valeur entière (h1) dans hl

FEBD 7E 1d a,(hl)

FEBE 23 inc hl

FEBF 66 ld h,(h1)

FECO 6F 1d l,a

FECI C9 ret

END DEEE DD DEEE NE DEEE HE DE HE GED HE EE fonction Basic UNT

-I11 252-

BASIC 1.0

FEC2 CD2DFF call FF2D amener résultat numérique

FEC5 D8 ret (ol entier ?

FEC6  CD46BD call BD46 convertir nombre virgule flottante
en entier

FEC9 3007 Jr nc, FED2 ‘Overflon'

FECB 47 1d b,a

FECC  FCA9BD call m, BDA9 accepter signe b dans nombre entier
hi

FECF  DAODFF Jp c,FFOD accepter nombre entier dans hl

FED2 1E06 id e,06 ‘Overflow’

FED4 C394CA Jp CA94 sortir message d'erreur

FED7 ES push hl

FED8 D5 push de

FED9 C5 push bc

FEDA 21C1B0 ld h1,B0C1 type de variable

FEDD BE cp (h1)

FEDE CUESFE call nz, FÉES

FEET C1 Pop bc

FEE2 Di pop de

FEE3 E1 pop hl

FEE4 C9 ret

FEES D603 sub 03

FEE7 38A4 jr c,FE8D CINT

FEE9 CA3CFF jp Z,FF3C type ‘chaîne’, sinon ‘Type mismatch'

HOHOHEOHEOHEONOHEHE HE DEEE EH EHEHEHEE EU fonction Basic CREAL

FEEC CD2DFF call FF2D amener résultat numérique

FEEF  DAGAFE jp C,FE6A entier, alors convertir

FEF2 C9 ret

RENAN ER ER ERAENEAN AE fixer valeur virgule flottante sur zéro

FEF3 ES push hi

FEF4 210000 ld h1,0000

FEF7  22C2B0 Id (BOC2),h1

FEFA 22C4B0 Id (BOC4),hl

FEFD 22C5B0 1d (BOC5),h1

FFO0 E1 pop hl

FFO1 C9 ret

-IIT 253-

BASIC 1.0

MODE DE DE DE DD DEEE DE DE DE DE DE DE DE DE DE DE DE DEN DE NE DEE H fonction Basic SGN

FFO2 CDA3FD call FDA3 SGN

FFO5 6F ld 1,a

FF06 87 add a,a

FF07 9F sbc a,a

FFO8 1802 jr FFOC

EN RNNNENNNENNNERAENNREENENE accepter contenu accu comme nombre entier

FFOA 6F ld 1,a Lo-Byte

FFOB AF xor a annuler Hi-Byte

FFOC 67 ld h,a

LÉLLESLLLLLLLLLLSLLLLSLLILLLLLLLLE) accepter nombre entier dans h]

FFOD 22C2B0 id (BOC2),h1 nombre dans BOC2

FF10 3E02 ld a, 02 type sur ‘entier’

FF12 32C1B0 ld (BOC1),a type de variable

FF15 C9 ret

LRLELLRS SSL SLLLLSLSRESSSIRLSSLSILÉRLRES. type de variable sur virgule
flottante

FF16  21C2B0 ld hl,BOC2 pointeur sur nombre à virgule
flottante

FF19 3E05 ld a,05 type sur ‘Real’

FF1B 18F5 jr FF12

ERA mener type de variable, hl est pointé sur variable

FF1D 21C1B0 ld h1,B0OC1 type de variable

FF20 4E ld c,(hl) dans c

FF21 23 inc hl h1 pointé sur variable
FF22 C9 ret

LRLRELLLSSLLLLSLLILLLLLLLLLSLLLIL LS. amener type de variable
FF23 3AC1B0 ld a, (BOC1) type de variable

FF26 C9 ret

LÉRRLLLELSILLLELLILSLE LISE LLLLLLLLL ES) tester si chaîne

FF27  3AC1B0 ld a, (B0C1) type de variable

FF2A FEO3 cp 03 chaîne’ ?

FF2C C9 ret

-111 254-

BASIC 1.0

EEE HE DE HE DEEE DE GENE DEN EH EH EH ON

FF2D 3SAC1BO ld a, (BOC1)
FF30 FEO3 CP 03

FF32 280C jr z,FF40
FF34  2AC2B0 id hl, (BOC2)
FF37 D8 ret C

FF38 21C2B0 1d

FF3B C9 ret

RERO HONHE END HE DEEE DE DE DE DEEE DE DE HN DD DENON

FF3C CDASFF call FF45

FF3F C8 ret Z
FF40 1E0D 1d e,0D
FF42  C39UCA Jp CA94

MED DH DEEE NH ED D DE MÉDECIN

FF45  3AC1BO id a, (BOC1)
FF48 FE03 CP 03
FFHA C9 ret

LÉLSSEL SELLES LLS LL LL ELLE LLLTZ LE)

FF4B 32C1B0 Id (BOC1),a
FFUE 11C2B0  ld de, B0C2
FF51 1813 Jr FF66
HEDEDEDE DE DEMO DEEE DD HE HE MEE DE DÉEDDDDÉME E DE DE
FF53 D5 push de

FF54 ES push hi

FF55 3ACIBO Id a, (BOC1)
FF58 UF 1d c,a

FF59  CDBOFS5 call F5B0
FF5C  CD62FF call FF62

FF5F El pop hl

FF60 D1 pop de

FF61 C9 ret

ECHEDE NE DE D HE DD DE DE DE DE DE DIE DD DE DE DEN DE HE DE DEEE
FF62 EB ex de,hl

FF63 21C2B0 1d h1,BOC2

h1,BOC2

amener résultat numérique

type de variable

chaîne ?

oui, ‘Type mismatch'

charger valeur entière

pas virgule flottante, terminé
adresse du nombre à virgule

flottante

tester si chaîne

oui, Ok

‘Type mismatch'

sortir message d'erreur
tester si chaîne

type de variable
‘chaîne’ ?

type de variable

placer résultat sur pile Basic

type de variable

réserver place dans pile Basic
placer sur pile

copier variable dans (h1)

-II1 255-

BASIC 1,0

FF66 C5 push DC

FF67 3AC1B0 id a, (BOC1) type de variable

FF6A 4F ld c,a

FF6B 0600 ld b,00

FF6D EDBO ldir copier résultat

FF6F C1 pop bc

FF70 C9 ret

RONDE DEHEHEDE DEN DEEE DEEE DE DE ME ÉNEE DEEE EH tester Si lettre

FF71  CD8AFF call FF8A convertir minuscules en majuscules
FF74 FEU Cp 41 'A'

FF76 3F ccf

FF77 DO ret nc

FF78 FESB cp SB LEA

FF7A C9 ret

CELLLSLLLLLLILESSISLLSSSSSLSSSS LE) tester si caractères alphanumér iques
FF7B CD71FF call FF71 tester si lettres

FF7E D8 ret C oui

FF7F  FE2E CP 2E Fa

FF81 37 scf

FF82 C8 ret Z

FF83 FE30 cp 30 0"

FF85 3F ccf

FF86 DO ret nc

FF87 FE3A cp 34 g'+1

FF89 C9 ret

LÉRELEESS SSL S ES SIL LS SSL RE SL SEE S SES] conversion minuscules-majuscules
FF8A FE61 CP 61 ‘a’

FF8C D8 ret C

FF8D FE7B CP 7B z'+#1

FF8F DO ret nc

FF90 D620 SUD 20 'a'-'A'

FF92 C9 ret

LÉELLLLLLLLLSRSSRSLSSSLSLSLSESSEERS SRE SZ parcourir table suivante
FF93 F5 push af

FF94 C5 push bc

FF95 46 ld b, (hl) charger longueur de table

FF96
FF97

FF98
FF99
FF9A
FF9B
FF9C
FFE
FF9F
FFAI
FFA2
FFA3
FFAL
FFAS
FFA6
FFA7
FFA8
FFA9

23
E5

23
23
BE
23
2804
05
20F7
E3
F1
JE
23
66
6F
C1
F1
cg

inc
push

inc
inc
CP

inc

dec
jr
ex
Pop
Id
inc
1d
id
Pop
POP
ret

BASIC 1.0

pl
hl

nl
hl
(h1)
hl

jr
b
nz,FF98
(sp),h1
af
a, (hl)
hl
h,(h1)
1,a
bc
af

A HNDEHEHE DD ME DE ME DE DEHEDEHE HE HE HEHE DEMEURE DEN

FFAA

FFAB
FFAC
FFAD
FFAE
FFBO
FFB1
FFB2
FFB4
FFB5
FFB6
FFB7

C5

LF
7E
B/
2805
23
B9
20F8
37
79
C1
C9

push

Id
1d
or
jr
inc
CP
jr
scf
id
pop
ret

bc

hi

nz,FFAC

a,C
bc

EDEN EDMOND DEEE DE EEE DEEE

FFB8
FFB9
FFBA
FFBB

7C
92
CO
7D

1d
sub
ret
1d

a,h
d

nz
a, l

adresse de retour si rechercher
négative
pointeur sur valeur suivante table

comparer caractère
augmenter pointeur
z,FFA2 trouvé
diminuer compteur
table pas encore finie ?
charger adresse de retour

adresse dans hi

parcourir zone de mémoire (hl)
jusqu'à (hi) = a (c=1) ou (hl) = 0

(c=0)

a dans c

zéro ?

égale a originaire a ?

mettre carry

test hl = de ?

h-d

FFBC
FFBD

93

C9

Sub
ret

BASIC 1.0

DEEE HE DEN DE DO DE UE HE HE DE DD HE NE DE DEN DD HE DEEE

FFBE
FFBF
FFCO
FFCI
FFC2
FFC3

7C
90
co
7D
91
C9

ld
sub
ret
ld
sub
ret

a,h
b
nz
a, 1
c

HERMIONE DE DE DE DEH HD DEEE DE DE D DEEE HE DE HE DE DE DE DE DE DEN

FFC4
FFCS
FFC6
FFC7
FFC8
FFC9
FFCA
FFCB
FFCC
FFCD
FFCE

C5
47
7D
93
5F
7C
SA
57
78
C1
C9

push
ld
ld
sub
ld
ld
sbc
ld
ld
pop
ret

bc
b.a
1

a

€

€,
a,
a,
d
a
b

D

DS: 9: €: 7 0

,

C

EDEDEDENEDEDE DD DE DEMI DE DE DEEE DE DE DE DE DEEE DEEE

FFCF
FFDO
FFDI
FFD2
FFD3
FFD4
FFD5
FFD6
FFD7
FFD8
FFD9

C5
47
7D
93
6F
7€
SA
67
78
C1
C9

push
Id
1d
Sub
1d
1d
sbc
Id
1d
pop
ret

bc
b,a
1

,

,

a
€
l
a
à,
h
a
b

$

TO QT

,

C

EEE D DE DE DE DEN DE DE DE DE HE DEEE DE DE DE DE NE DE NE D Ne

FFDA
FFDB

ES
67

push
1d

hl
ha

test hl = bc ?

h-b

de := de -hl
sauver bc
sauver a

e:= |

d-h

ramener a
ramener bc

hi := hl - de
sauver bc
sauver à

h-d

ramener a
ramener bc

bc := hi - de
sauver hl
sauver a

BASIC 1,0

FFDC E3 ex (sp},hl
FFDD 7D ld a, |
FFDE 93 sub e

FFDF 4F 1d c,a
FFEO 7C id a,h
FFET 9A sbc a,d
FFE2 47 ld b,a
FFE3 E3 ex (sp),hl
FFE4  7C ld a,h
FFES E1 pop hl

FFE6 C9 ret

BEDHMNHDHE DH DEN DD HHEMEHE DE DEN DE DE DH DE HE DEN

FFE7 DS push de
FFE8 57 ld d,a
FFE9 7D id a, |
FFEA 91 sub c
FFEB 6F ld },a
FFEC 7C 1d a,h
FFED 98 sbc a,b
FFEE 67 ld ha
FFEF 7A id a,d
FFFO D1 pop de
FFF1 C9 ret

HDEHE DE HE DEEDE HONG DEN DEHEDEDE EH OHE HE DEDE NE

FFF2  EDBO ldir
FFF4 C9 ret
EDEN DER ENNEMI ME DE DEN DEEE EEE EEK
FFF5  EDBS lddr
FFF7 C9 ret

HORDE MEN DENON EE DEEE EH DEEE EH DE EEE

FFF8 E9 JP (h1)

END E HE DE DÉCHHEHEHEDE DE DEEE DE DEEE DE DEEE NE

FFF9 C5 push bc
FFFA C9 ret

rétablir hl

1 -e
dans c

h-d
dans b

ramener a

ramener hI

hl := hl - bc
sauver de
sauver à

h-b
ramener a

ramener de

transfert de bloc

transfert de bloc

saut dans (h1)

saut dans (bc)

idir

1ddr

BASIC 1,0

HD DD DE DEA DE HE DE HER DEEE DE DE DE DE DE DE DE DE DE DE DE DE DE DE saut dans (de)
FFFB DS push de

FFFC C9 ret

FFFD C7 rst 0

FFFE C7 rst 0

FFFF 54

LH ANNEXES
4,1 Les routines du système d'exploitation

Nous avons établi ici une liste des routines et des tableaux du système
d'exploitation, pour autant qu’elles nous soient connues.

Attention! N’essayez jamais d'appeler ces routines avec les adresses que
nous vous fournissons ici, si vous ne maîtrisez pas pleinement le
mécanisme de commutation de la configuration de la mémoire!

Utilisez plutôt les vecteurs qui vous sont fournis au chapitre 2.1.

Cette présentation a pour but principal de vous permettre de retrouver
aisément dans le listing les vecteurs portant les mêmes noms.

6030 RST 6 USER

0040 jusqu'ici, recopié dans la Ram
0044 Restore High Kernel Jumps
005C KL CHOKE OFF

0099 KL TIME PLEASE

0043 KL TIME SET

00B1 Scan Events

0153 Kick Event

0163 KL NEW FRAME FLY
0164 KL ADD FRAME FLY
0176 KL NEW FAST TICKER
017D KL ADD FAST TICKER
0183 Delete Fast Ticker
0189 Traiter Ticker Chain
01B3 KL ADD TICKER

01C5 Delete Ticker

01D2 KL INIT EVENT

01E2 KL EVENT

021A KL DO SYNC

0228 KL SYNC RESET

022F Ajouter Sync Event
0256 KL NEXT SYNC

0277 KL DONE SYNC

0285 KL DEL SYNCHRONOUS
028E KL DISARM EVENT

0295 KL EVENT DISABLE
029B KL EVENT ENABLE

O2A1 KL LOG EXT

02B2 KL FIND COMMAND

-IV 1-

0329
0332
0373
0382
03B2
O3CA
0401
O4OD
0413
0442
O4LA
0450
O4A
O4A7
OUBF
O4DB
OUES
OUEF
O4F9
0503
O50F
0514
051D
0533
0537
053D
0543
055€
O56D
0580
05B4
O5C4
OSDC
060B
065€
066D
0693
06EB
06F4
0727
0776
0786

KL ROM WALK

KL INIT BACK

Add Event

Delete Event

KL POLL SYNCHRONOUS
RST 7 INTERRUPT ENTRY CONT'D
EXT INTERRUPT ENTRY

KL LOW PCHL CONT'D

RST 1 LOW JUMP CONT'D
KL FAR PCHL CONT'D

KL FAR ICALL CONT'D
RST 3 LOW FAR CALL CONT'D
KL SIDE PCHL CONT'D
RST 2 LOW SIDE CALL CONT'D
RST 5 FIRM JUMP CONT'D
KL L ROM ENABLE

KL L ROM DISABLE

KL U ROM ENABLE

KL U ROM DISABLE

KL ROM RESTORE

KL ROM SELECT

KL PROBE ROM

KL ROM DESELECT

KL CURR SELECTION

KL LDIR

KL LDDR

Rom off & config, save
RAM LAM

RAM LAM (IX)

RESET CONT'D

Table 60Hz

Table 50Hz

MC BOOT PROGRAM

MC START PROGRAM
Démarrage à froid
Message après allumage
Message de copyright
Sortir messages
Message d'erreur de chargement
Noms de sociétés

MC SET MODE

MC CLEAR INKS

-IV 2-

0798 MC SET IAKS

O7AB Sortir couleur
O7BA MC WAIT FLYBACK
07C6 MC SCREEN OFFSET
07E6 MC RESET PRINTER
07F2 MC PRINT CHAR
07F8 MC WAIT PRINTER
0807 MC SEND PRINTER
081B MC BUSY PRINTER
0826 MC SOUND REGISTER
0846 Scan Keyboard
0888 JUMP RESTORE

O8AC Main Jump Adr.
0A28 Basic Jump Adr.
DA8A Move (h1+3)((h1+1)),cnt=(h1)
OAAO SCR INITIALISE
OAB1 SCR RESET

OACA SCR SET MODE

OAEC SCR GET MODE

OAF7 SCR MODE CLEAR
0B11 Charger masques bits
OB2E Bit Masks Mode 2
0B36 Bit Masks Mode 1
OB3A Bit Masks Mode 0
OB3C SCR SET OFFSET
0B45 SCR SET BASE

OB5O SCR GET LOCATION
0B57 SCR CHAR LIMITS
OB64 SCR CHAR POSITION
OBAQ9 SCR DOT POSITION
OBF9 SCR NEXT BYTE
0CO5 SCR PREV BYTE
0C13 SCR NEXT LINE
OC2D SCR PREV LINE
OC49 SCR ACCESS

0C68 SCR WRITE

0C6B SCR PIXELS (FORCE Mode)
0C72 XOR Mode

0C77 AND Mode

0C7D OR Mode

0C82 SCR READ

0C86 SCR INK ENCODE

—IV 3-

OCAO SCR INK DECODE

OCD2 Reset couleurs

OCE4 SCR SET FLASHING

OCE8 SCR GET FLASHING

OCEC SCR SET INK

OCF1 SCR SET BORDER

OCF2 Set Colour

ODOA Aller chercher entrée matrice de couleur
0D14 SCR GET INK

0D19 SCR GET BORDER

OD1A Get Colour

OD2F Aller chercher adr. ink
OD5SB Set Inks on Frame Fly

0D6D Flash Inks

0D81 Aller chercher params de Jeu de couleurs actuel
0D93 Matrice de couleur E
ODB3 SCR FILL BOX

ODB7 SCR FLOOD BOX

ODDF SCR CHAR INVERT

ODF2 Adresser la mémoire couleur
ODFA SCR HW ROLL

OE3E SCR SH ROLL

OEF3 SCR UNPACK

OF49 SCR REPACK

OFC4 SCR HORIZONTAL

102F SCR VERTICAL

104D Couleur par défaut

1078 TXT INITIALISE

1088 TXT RESET

1043 Reset Params (toutes les fenêtres)
10E8 TXT STR SELECT

1107 TXT SWAP STREAMS

1122 Idir cnt=15

112A Adr. params fenêtres => de
113D Fixer params TXT par défaut
115E TXT SET COLUMN

1169 TXT SET ROW

1174 TXT SET CURSOR

1180 TXT GET CURSOR

118A Fenêtre act. haut,gauche+hl
1197 Fenêtre act, haut, gauche-h1l
1148 move Cursor

=IV 4-

11CE TXT VALIDATE

11DA h1 dans limites fenêtre?
120€ TXT WIN ENABLE

1256 TXT GET WINDOW

1263 TXT DRAW/UNDRAW CURSOR
1268 TXT PLACE/REMOVE CURSOR
1279 TXT CUR ON

1281 TXT CUR OFF

1289 TXT CUR ENABLE

128B Cur Enable Cont'd

129A TXT CUR DISABLE

129€ Cur Disable Cont'’d

12A9 TXT SET PEN

12AE TXT SET PAPER

12BD TXT GET PEN

12C3 TXT GET PAPER

12C9 TXT INVERSE

12D3 TXT GET MATRIX

12F1 TXT SET MATRIX

12FD TXT SET M TABLE

132A TXT GET M TABLE

1334 TXT WR CHAR

134A TXT WRITE CHAR

137A TXT SET BACK

1387 TXT GET BACK

13A7 TXT SET GRAPHIC

13AB TXT RD CHAR

13C0 TXT UNKRITE

1400 TXT OUTPUT

140C TXT OUT ACTION

144B TXT VDU DISABLE

1451 TXT VDU ENABLE

146B Sauts caractères de commande par défaut
14CB TXT GET CONTROLS

14D8 07 Bip

14E3 16 Transparentmode mis/éteint
14E8 1C =INK (instruction)
14F1 1D =BORDER (instruction)
14F8 1A définir fenêtre

1504 19 =SYMBOL (instruction)
1504 08 CRSR LEFT

150F 09 CRSR RGHT

“IV 5-

1514 OA CRSR DOWN

1519 OB CRSR UP

152A 1E CRSR HOME

1530 OD CRSR sur début de ligne

1538 1F =LOCATE (instruction)

1540 TXT CLEAR WINDOW

154F 10 supprimer caractère sur CRS Pos
1556 14 vider fenêtre à partir de CRS Pos
156D 13 vider fenêtre Jusqu'à CRS Pos
1584 12 supprimer ligne à partir de CRS Pos
158 11 supprimer ligne Jusqu'à CRS Pos
15B0 GRA INITIALISE

15DF GRA RESET

15F1 GRA MOVE RELATIVE

15FC GRA ASK CURSOR

1612 GRA GET ORIGIN

161A aller chercher position de départ physique
161D aller chercher position objet et fixer Cur
1657 Add coord. act. + coord. rel.

1734 GRA WIN WIDTH

1779 GRA WIN HEIGHT

17A6 GRA GET W WIDTH

17BC GRA GET W HEIGHT

17C5 GRA CLEAR WINDOW

17F6 GRA SET PEN

17FD GRA SET PAPER

1804 GRA GET PEN

180A GRA GET PAPER

1810 GRA PLOT RELATIVE

1813 GRA PLOT ABSOLUTE

1816 GRA PLOT

1824 GRA TEST RELATIVE

1827 GRA TEST ABSOLUTE

182A GRA TEST

1836 GRA LINE RELATIVE

1839 GRA LINE ABSOLUTE

183C GRA LINE

1945 GRA WR CHAR

19E0 KM INITIALISE

1A1E KM RESET

1A3C KM WAIT CHAR

1A42 KM READ CHAR

-IV 6-

1481 KM EXP BUFFER CONT'D
1AB3 Default Exp String
1ABD KM SET EXPAND

TAES nettoyer Exp Buffer
1B22 Place pour nouvelle Exp String?
1B2E KM GET EXPAND

1B3E Adr, Exp String dans de
1B56 KM WAIT KEY

1B5C KM READ KEY

1BB3 KM GET STATE

1BB7 Update Key State Map
1C2F KM TEST BREAK

1C5C KM GET JOYSTICK

1C69 KM GET DELAY

1C6D KM SET DELAY

1C71 KM ARM BREAK

1C82 KM DISARM BREAK

1C90 KM BREAK EVENT

1CBD KM TEST KEY

1CCD aller chercher bit correspondant à touche #
1CES Bit Masks

1D3E KM GET TRANSLATE
1D43 KM GET SHIFT

1D48 KM GET CONTROL

1D4B Get Key Table

1D52 KM SET TRANSLATE
1D57 KM SET SHIFT

1D5C KM SET CONTROL

1D5F Set Key Table

1D69 Key Translation Table
1DB9 Key SHIFT Table

1E09 Key CTRL Table

1E68 SOUND RESET

1ECB SOUND HOLD

1EE6 SOUND CONTINUE

1F03 Sound Event

1F61 Scan Sound Queues
1F9F SOUND QUEUE

204A SOUND RELEASE

206C SOUND CHECK

2089 SOUND ARM EVENT

2273 fixer volume

-IV 7-

2338 SOUND AMPL ENVELOPE

233D SOUND TONE ENVELOPE

2340 copier courbe d’enveloppe
2349 SOUND A ADDRESS

234E SOUND T ADDRESS

2351 aller chercher adresse courbe d'enveloppe
2370 CAS INITIALISE

237F CAS SET SPEED

238E CAS NOISY

2392 CAS IN OPEN

23AB CAS OUT OPEN

23AF CAS Open

23FC CAS IN CLOSE

2401 CAS IN ABANDON

2415 CAS OUT CLOSE

242E CAS OUT ABANDON

2435 CAS IN CHAR

245B CAS OUT CHAR

248B Check Input Buffer Status
248E Check Buffer Status

2496 CAS TEST EOF

2U9A CAS RETURN

24AB CAS IN DIRECT

2UEA CAS OUT DIRECT

2528 CAS CATALOG

253F lire File Header

271F sortir message CAS (# dans b)
2780 sortir message CAS (1 caractère)
27C5 messages cassette

2836 CAS READ

283F CAS WRITE

2851 CAS CHECK

2873 allumer moteur et ouvrir clavier
29CD CAS Input RD DATA & Test ESC
2437 CAS Output WR DATA

2A4B CAS START MOTOR

2AUF CAS STOP MOTOR

2A51 CAS RESTORE MOTOR

2A98 EDIT

2AC6 EDIT exécuter saut

2AE0 EDIT Table de saut 1

2B1C EDIT Table de saut 2

-IV 8-

2B2B BIP

2B2F CRSR UP

2B33 CRSR DHN

2B37 CRSR RGHT

2B3B CRSR LEFT

2B42 ESC

2B61 message *BREAK*

2B69 ENTER

2B75 CRSR RGHT (Buffer)

2B7E CRSR DWN (Buffer)

2B89 CTRL & CRSR RGHT

2B92 CTRL & CRSR DWN

2BAA CRSR LEFT (Buffer)

2BB3 CRSR UP (Buffer)

2BBD CTRL & CRSR LEFT

2BC7 CTRL & CRSR UP

2BF9 CTRL & TAB (Flip Insert)

2001 insérer caractère

2C3D DEL

2C4A CLR

2098 SHFT & CRSR RGHT

2C9D SHFT & CRSR LEFT

2CA2 SHFT & CRSR UP

2CA7 SHFT & CRSR DWN

2CEA COPY

2DD9 caractère du clavier

2DF6 aller chercher adr, de saut EDIT
2E18 FLO copier variable de (de) => (hl)
2E29 FLO Int => Flo

2E55 FLO valeur 4 octets => Flo
2E5E FLO valeur 4 octets * 256 => Flo
2E66 FLO Flo => Int

2E8E FLO Flo => Int

2EA1 FLO FIX

2EAC FLO INT

2EB6 FLO

2F94 FLO RND Init

2FA1 FLO set RND seed

2FB7 FLO RND

2FD1 FLO multiplier nombre par 10°a
2FE6 FLO aller chercher dernière valeur RND
300F FLO L0610

-IV 9-

3014 FLO LOG

3090 FLO EXP

3104 FLO SGR

310D FLO élévation à la puissance
3143 FLO PI

31AE FLO DEG/RAD

31B2 FLO COS

31BC FLO SIN

3231 FLO TAN

3241 FLO ATN

3337 FLO soustraction

333B FLO soustraction

333F FLO addition

3415 FLO multiplication

349E FLO division

3578 FLO multiplier le chiffre par 2°a
359A FLO comparer

35E8 FLO SGN

35F8 FLO inverser signe

3708 INT

370€ INT

3715 INT accepter signe dans b
3728 INT addition

3730 INT soustraction

3731 INT soustraction

3739 INT multiplication avec signe
3750 INT multiplication sans signe
377A INT division avec signe

3781 INT MOD

378C INT division sans signe

37D4 INT inversion de signe

37E0 INT SGN

3/E9 INT comparer

-IV 10-

4,2 References à la Ram système

Vous trouverez ci-dessous des références croisées aux endroits où elles
sont utilisées pour toutes les adresses de la Ram qui apparaissent dans
le listing de la Rom du système d'exploitation.

C'est très utile lorsque vous manipulez les contenus de ces adresses avec
vos propres programmes et que vous y trouvez soudain une autre valeur que
celle que vous attendiez,

Vous pouvez donc consulter la table suivante qui vous indique quelles
routines accèdent à une adresse déterminée.

B100: 0066 00F2 011D 0127 061€
B101: OCEC O61F

B102: O0F5 OOFE 0102

B104: O0E2 00F8 0114 0132 0142 O3E1
B105: 010A OT4E

B187: O09E OOAC 00B1 010€

B189: 009A O0A8

B18B: OOA5

B18C: O0BF 0164 0170

B18E: O0C7 017D 0183

B190: O0DC 0189 O1BF 01C5

B192: 00D2

B193: 0257 026F 0288 03B9

B194: 022B 03B2

B195: 0264 026C 0277 0295 029B 03C3
B196: 0231 02B2 O30A

B1A6: 02A2 02A6 O2BF

B1A8: 0080 O34B 0467 0499 0529 0533
B1A9: 0060 0086 0096

BIAA: 0348

B1AB: OOSD 0083 04B9

B1C8: OAEC 0B28

B1C9: OB4O OB50 OB84 OBDD OE24 O0E37
B1CA: OB00

B1CB: OAA8 0B47 OB53 OB8D OBEG OE2C
B1CC: 0C61

B1CD: OC64

B1CF: 0B20 OBF1 OC8E OCA2 OFO08 0F18 0F32 0F66 OF7D OFAT 1015
B1D7: OCE4 OCE8 OD8F

B1D8: OD88

B1D9: OCD5 OD8C

-IV 11-

OD84

10EA

1163
1577

1197
119F
1259
1573

1291

126E
12AE
1387

13E9
145C

14CB
1637
164E
1658
165E
16DA
16E8
1644
1691
181D
1804
1911

1107

116E

11F3
11E1
1549
1588

12A2

12A9
1203

16DE
16F1
16BD
16B3
190A
19D8
1936

1110

1174

122D
11E6
155€

12BD
13D3

1700
170Â
1720
1716
192F

1180 11AB 11B1 133F 13B1

1256 152A 1543 1559 1570
1533 1593

12C9 12CF 1391 139F 13C0
1566 157D 1597

1758 17A6 1/E2
175C 17AÀ

178A 17BC 17D9
178E 17C0 17D5

-IV 12-

B340: 18BE 18CD 18E1
B342: 1841 184E 1859 185D 18A2 18A6 18F7 18FD 1927 193A
B344: 1845 1860 1864 1872 18A9 18AF 1903 1915 191A 1920
B346: 18BA 18F1

B43C: 19EF

Buu6: 1A24

BUDE: 1A4C 1A6D

B4DF: 1AAF 1ADA

BUEU: 1A43 1A77

BUET: 1A8E 1B44

BUE3: 1A8A 1B05

B4ES: AAC 1B00 1B11 1B1C 1B22
BUEG: 1B27

B4E7: 19EC 1B8D 1BA6 1BB3
B4E8: 1B76

B4E9: 1C15 1C69 1C6D
BUEA: 1C4F

BUEB: 1A0F 1BCE 1BFD 1CC5
BHED: 1BC6 1CBE

B4F1: 1C5C

BuF3: 1C2F

BuF4: 1C62

B4F5: 1BBA

BUFF: 1BB7 1BCB

B501: 1BCO

B509: 1BF1 1C09 1018
B50A: 1BF6 1C23

B50B: 19/7

B50C: 1C7E 1C84 1C90
B5SOD: 1C74

B51D: 1E9D 1EEB 1F12 1F48 1FAD 1FD2 2052
B520: 206F

B522: 1F74

B539: 208D

B53C: 1CEE 1CFE 1D26
B53E: 1D0F 1D15

B540: 1COD 1D0B 1D22
B541: 1A01 1D3E 1D52
B543: 19FD 1D43 1D57
B545: 19F9 1D48 1D5C
B547: 19F5 1C02 1CA6 1CAE
B550: 1F05 20B2

-IV 13-

B551:
B552:
B554:
B555:
B55C:
B59B:
B5DA:
B60A:
B619:
B6FA:
B800:
B801:
B802:
B803:
B805:
B807:
B817:
B818:
B819:
B81A:
B81C:
B81E :
B81F:
B847:
B8u8:
B84A:
B8uC:
B85C:
B85D:
B&5E :
B85F:
B861:
B863:
B864 :
B866:
B88C:
B89D:
B89F:
B84A3:
B846:
B&CC:
B8CD:

1E6D
1E6À
1F5B
1E70
1E80
212D
2135
219A
1E7D
233D
238E
2694
2392
24CF
2451
25D6
258A
253F
23A6
243F
239E
2594
23A2
23AB
2504
247F
261€
265B
241F
24F9
2469
2632
2624
24FC
2500
254C
258E
2567
25D0
24CA
240C
2873

1EE6
1ECB
1F97

2125
2150
2148
2338
2292
234E
2695
2705
23FC
2530
2456
25€ 1

244A
24B2
25CA
2415
251B
2484
2636
264D
2478

2660

25DE

2597

2673
295D

201F
1F61

2349

2760
279F
2401
257D
24A2
25F3

2U4E

24B9

242E
2620
262F

247C

25F6

2973

20F5
2283

248B

24A6

249B
24C1

245F

2507

2692

2528 256€ 25A9 27BF

2580

249F 24BC 24D6 2594
24D2 256B

2HED 2667

2514 2644 2658

-IV 14-

B8CE: 2956 29B3

B8D0: 2A08 2A1B

B8D1: 238A

B8D2: 2A0C

B8D3: 28B1 2990 29A2 29A6

B8DC: 2C1E 2C35 2C5B 2C67 2DCE

B8DD: 2AAS 2BF9 2BFD 2C04

B8DE: 2C72 2C76 2C83 2C94 2CAC 2CC4 2CD5 2CFO 2CFE
2D11 2D1A 2D36

-IV 15-

4,3 Les routines de la Rom Basic

C006 initialisation du Basic

CO3F ‘BASIC 1,0’ LF,LF

CO52 instruction Basic EDIT

CO64 mode READY

COCC ‘Ready’, LF

COD3 supprimer mode AUTO

COD6 fixer mode AUTO

CODF instruction Basic AUTO

C12B instruction Basic NEW

C132 instruction Basic CLEAR

C13E supprimer programme et variables
C18C supprimer variables

C1DO aller chercher numéro stream
C1E3 tester si numéro stream

C20A instruction Basic PAPER

C212 instruction Basic PEN

C221 instruction Basic BORDER

C22A instruction Basic INK

C23C aller chercher argument(s) < 32
C24C aller chercher argument < 16
C24F instruction Basic MODE

C25A instruction Basic CLS

C262 fonction Basic VPOS

C276 fonction Basic POS

C290 aller chercher position PRINT act.
C2D2 instruction Basic LOCATE

C2E1 instruction Basic WINDOW

C2FD WINDOW SWAP

C312 aller chercher argument < 8

C319 instruction Basic TAG

C320 instruction Basic TAGOFF

C327 aller chercher 2 valeurs 8 bits non nulles
C337 sortir chaîne sur stream zéro
C341 sortir chaîne

C34E sortir linefeed

C386 initialiser l'écran

C348 sortir CR & LF

C3B5 sortir caractère sur imprimante
C3DF aller chercher actuelle position imprimante
C3E3 instruction Basic WIDTH

-IV 16-

cm7
C424
C42C
C439
C453
CUSE
C46F
C48c
C4C6
CUCB
C4DO
C4DS
CHE9
CHEE
C505
C5OA
C51A
C529
C5SFB
C632
C6C7
C6E8
C6ED
C70F
C72E
C747
C776
C7E3
C807
C8CB
C8&1
C8E7
C8ED
C940
C95D
971
c979
C99F
C9B1
C9cs
CA18
CA3B

variable réservée EOF

aller chercher un caractère sur canal d'entrée
attendre un caractère du clavier
lire clavier

autoriser interruption par ‘Break’
routine Break-Event

attendre frappe d’une touche après ‘ESC’
instruction Basic ORIGIN
instruction Basic DRAW

Instruction Basic DRAWR
instruction Basic PLOT

instruction Basic PLOTR

Fonction Basic TEST

fonction Basic TESTR

instruction Basic MOVE

instruction Basic MOVER

aller chercher 2 arguments entiers
instruction Basic FOR

instruction Basic NEXT

chercher boucle FOR-NEXT ouverte
instruction Basic IF

instruction Basic GOTO

instruction Basic GOSUB
instruction Basic RETURN
instruction Basic chercher GOSUB sur pile Basic
instruction Basic WHILE
instruction Basic WEND

instruction Basic ON

Traitement Event

instruction Basic ON BREAK
instruction Basic DI

instruction Basic EI

Reset SOUND et Event

instruction Basic ON SQ

calculer adresse de la Sound Queue
instruction Basic AFTER
instruction Basic EVERY
instruction Basic REMAIN

calculer adresse du bloc Event
chercher NEXT correspondant
chercher WEND correspondant

aller chercher ligne d'entrée

-IV 17-

CAU43 éditer ligne

CA4C aller chercher ligne d'entrée dans cassette
CA84 annuler numéro et ligne d'erreur

CA8F instruction Basic ERROR

CA94 sortir message d'erreur

CB23 ‘Undefined line’

CBUF ‘Break’, ‘in’

CB5SA instruction Basic STOP

CB65 instruction Basic END

CBCO instruction Basic CONT

CBES ON ERROR

CBF8 ON ERROR GOTO 0

CCO3 instruction Basic RESUME

CC4S fixer pointeur sur message d'erreur

CC5B messages d'erreur

CE67 aller chercher valeur 8 bits

CE6D aller chercher valeur 8 bits non nulle

CE7C aller chercher valeur 16 bits entre O0 et 32767
CE86 aller chercher valeur entière entre -32768 et +32767
CE91 aller chercher valeur 16 bits entre -32768 et +65535
CESF aller chercher expression chaîne, préparer paramètres
CEAS aller chercher expression chaîne

CEBO aller chercher zone de numéros de ligne
CEET aller chercher numéro de ligne dans de

CEFB aller chercher expression

CFO7 aller chercher terme

CF30 opérateurs arithmétiques

CF59 opérateurs de comparaison

CF81 table des opérateurs Basic

CFAQ comparaison arithmétique

CFB9 signe négatif

CFC2 opérateur Basic NOT

CFCE aller chercher expression

DOOD aller chercher variable

DO2C aller chercher constante numérique

D0O70 aller chercher expression entre parenthèses
DO80 calcul de fonction

DOCA adresses des variables réservées

DODC variable réservée ERR

DCS variable réservée TIME

DOEE variable réservée ERL

DOF4 variable réservée HIMEM

-IV 18-

DOFA
D107
D10F
D117
D130
D190
DIEA
DIEE
D219
D246
D256
D25F
D298
D2A1
D2AD
D2C0
D30D
D31E
D329
D341
D3UE
D385
D3FF
D409
D423
D439
D456
D494
D4AB
D4C3
DADB
DHE7
DUEB
DEF
D4F4
D519
D520
D525
D52A
D52F
D534
D539

pointeur de variable, ‘arobas’
variable réservée XPOS
variable réservée YPOS
instruction Basic DEF

fonction Basic FN

adresses des fonctions Basic
fonction Basic MIN

fonction Basic MAX

fonction Basic ROUND
instruction Basic CAT
instruction Basic OPENOUT
instruction Basic OPENIN
instruction Basic CLOSEIN
instruction Basic CLOSEOUT
interrompre 1/0 cassette
instruction Basic SOUND

aller chercher valeur 8 bits, s'il y en a une
instruction Basic RELEASE
fonction Basic SQ

aller chercher argument entre -128 et +127
instruction Basic ENV
instruction Basic ENT

aller chercher argument entre O0 et 4095
fonction Basic INKEY

fonction Basic JOY

instruction Basic KEY

KEY DEF

instruction Basic SPEED

SPEED KEY & INK

SPEED WRITE

variable réservée pi
instruction Basic DEG
instruction Basic RAD

fonction Basic SGR

opérateur Basic ‘”°'

appeler fonctions arithmétiques
fonction Basic EXP

fonction Basic LOG10

fonction Basic LOG

fonction Basic SIN

fonction Basic COS

fonction Basic TAN

-IV 19-

D53E
D559
D584
DSAE
DSD2
DSEA
D5FC
D614
D618
D61C
D654
D67D
D686
D690
D/708
D7B5
D7DB
D906
D97F
D999
D9C0
D9cc
DAF8
DB1A
DB2B
DB47
DB77
DBAD
DCD9
DCEB
DD37
DD3F
DD4A
DD55
DD61
DD71
DDAB
DDD2
DDD6
DDE2
DDE6
DDEB

fonction Basic ATN

instruction Basic RANDOMIZE

fonction Basic RND

restaurer pointeur de variable

annuler flag pour FN

calculer adresse de table pour tableau
types de variable A-Z sur ’Real'
instruction Basic DEFSTR

instruction Basic DEFINT

instruction Basic DEFREAL

instruction Basic LET

instruction Basic DIM

chercher variable Basic

aller chercher adresse de variable
chercher tableau

dimensionnement de variable

tester si variable dimensionnée

aller chercher nom de variable
déterminer type de variable

actualiser table de tableaux
instruction Basic ERASE

supprimer un tableau

instruction Basic LINE

aller cherch entrée sur appareil actif
instruction Basic INPUT

aller chercher entrée et convertir
’?Redo from start’

aller chercher entrée du clavier
instruction Basic RESTORE

instruction Basic READ

tester si encore un caractère

ignorer les espaces

tester si fin de l'instruction

tester si prochain caractère est une virgule
ignorer espace, TAB et LF

boucle de l'interpréteur

exécuter instruction Basic

aller chercher actuelle adresse de ligne
aller chercher actuelle adresse de ligne et tester si mode direct
instruction Basic TRON

instruction Basic TROFF

routine TRACE

-IV 20-

DEO1
DEE1
DFDC
EOF7
ETOD
ET45
E155
E163
E277
E27D
E288
E2A3
E2AE
E2C8
E354
E388
E64B
E676
E69D
E6BC
E6D2
E728
E767
E79A
E7DF
E8c1
E8EF
E8F3
E9BD
E9F6
EA3C
EAAG6
ECO9
EC3D
EC5C
EC87
EE61
EE79
EE82
EE9D
EE9F
F114

adresses des instructions Basic

aller chercher caractère dans buffer d'entrée
table des instructions Basic avec numéro de ligne
instruction Basic LIST

lister lignes Basic bc-de

sortir un caractère du clavier

sortie sur écran

lister ligne Basic dans buffer

sortir nombre à un octet

sortir nombre sur deux octets

sortir numéro de ligne

sortir nombre binaire

sortir nombre hexa

sortir nombre à virgule flottante

adresses des mots instructions Basic

table des mots instructions Basic

table des opérateurs Basic

annuler pointeur de programme

remplacer adresse de ligne par numéro de ligne
convertir ligne d'entrée en code interpréteur
convertir instruction en code interpréteur
instruction Basic DELETE

aller chercher adresse de ligne

chercher ligne Basic

instruction Basic RENUM

tester si variable indicée

instruction Basic DATA

instructions Basic ELSE, REM et
instruction Basic RUN

instruction Basic LOAD

instruction Basic CHAIN

instruction Basic MERGE

instruction Basic SAVE

SAVE, P

SAVE, B

SAVE, A

Convertir chiffre ASCII en binaire

sortir nombre entier h1

convertir nombre entier en ASCII

convertir nombre en ASCII

formater nombre

conversion en binaire

-IV 21-

F119 conversion en hexa

F158 fonction Basic PEEK

F15F instruction Basic POKE

F16D fonction Basic INP

F177 instruction Basic OUT

F17D instruction Basic WAIT

F194 aller chercher valeurs 16 bits et 8 bits
FAO chercher extension d'instruction Basic
F1BA instruction Basic CALL

F1F2 initialiser tabulations

F1F6 instruction Basic ZONE

F1FD instruction Basic PRINT

F25C PRINT,

F277 PRINT SPC

F280 PRINT TAB

F2A0 aller chercher valeur entière entre parenthèses
F2C4 PRINT USING

F3BA tester si caractère de formatage

F47B instruction Basic WRITE

F4C4 configurer la mémoire

FUEF instruction Basic MEMORY

F501 faire de la place pour le programme à charger
F51D calculer longueur de la zone des chaînes
F52C augmenter pointeurs de programme et de variable de bc
F58 initialiser pile Basic

F5AO libérer place dans pile Basic

F5BO réserver place dans pile Basic

F5D1 réserver place pour chaîne

FSE6 tester si place dans zone de chaînes
F5SF8 réserver place dans zone variables

F618 tester si place dans zone variables

F628 calculer place libre en mémoire

F69D instruction Basic SYMBOL

F6CD SYMBOL AFTER

F7CB lire chaîne

F828 sortir chaîne

F834 fonction Basic LOWHERS$

F839 conversion majuscules en minuscules
F842 fonction Basic UPPER$

F863 addition de chaîne

F897 comparaison de chaîne

F8BA fonction Basic BIN$

-IV 22-

F8Cu
F8CE
F8E A
F9TE
F93C
F943
F9uB
F993
FSE9
F9FB
FAOA
FA10
FA16
FA24
FA36
FA57
FA70
FA77
FA92
FAAI
FBB3
FBDA
FC19
FC2D
FC3E
FCCC
FCEI
FCF5
FDO9
FD12
FD37
FD49
FD58
FD63
FD6D
FD85
FD89
FDE8
FDED
FE4F
FE6A
FE8D

fonction Basic HEX$

aller chercher arguments pour BIN$ et HEXS$
fonction Basic DECS

fonction Basic STR$

fonction Basic LEFT$

fonction Basic RIGHTS

fonction Basic MID$

instruction Basic MID$

aller chercher chaîne et valeur 8 bits
aller chercher 3ème argument pour MID$
fonction Basic LEN

fonction Basic ASC

fonction Basic CHR$

variable réservée INKEYS

fonction Basic STRING$

fonction Basic SPACES

aller chercher code ASCII

fonction Basic VAL

conversion en entier et test < 256
fonction Basic INSTR

initialiser pile descripteur

aller chercher paramètres de chaîne
réserver place, placer descripteur
fonction Basic FRE

Garbage Collection

opérateur Basic ‘+’

opérateur Basic ‘-’

opérateur Basic ’*’

comparaison arithmétique

opérateur Basic ‘’/’

opérateur Basic ‘Backslash'

opérateur Basic MOD

opérateur Basic AND

opérateur Basic OR

opérateur Basic XOR

fonction Basic ABS

inverser signe

fonction Basic FIX

fonction Basic INT

convertir opérandes entiers en virgule flottante
convertir nombre entier en virgule flottante
fonction Basic CINT

—IV 23-

FEC2
FEEC
FF02
FFOA
FFOD
FF16
FF23
FF27
FF2D
FF3C
FF45
FF53
FF62
FF71
FF8A
FF93
FFAA
FFB8
FFBE
FFC4
FFCF
FFDA
FFE7
FFF2
FFF5
FFF8
FFF9
FFFB

fonction Basic UNT

fonction Basic CREAL

fonction Basic SGN

accepter contenu accu comme nombre entier
accepter nombre entier en hi

fixer type de variable sur virgule flottante
aller chercher type de variable

tester si chaîne

aller chercher résultat numérique
tester si chaîne, sinon ‘Type mismatch'”
tester si chaîne

placer résultat sur pile Basic

copier variable dans (hl)

tester si lettre

convertir minuscules en majuscules
parcourir table

parcourir table

comparer hl <> de

comparer hl <> bc

de:=de-h]

hl:=hl-de

bc:=hl-de

hl:=h1-bc

transfert de bloc Idir

transfert de bloc lddr

Jp (hl)

Jp (bc)

Jp (de)

-IV 24-

4,4 Les tokens Basic

00
01
02
03
04
OD
0E
OF
10
11
12
13
14
15
16
17
19
1A
1B
1C
1D
Î1E
1F
80
81
82
83
84
85
86
87
88
89
8A
8B
8C
8D
8E
8F
90
91
92
93
94
95
96
97

Fin de ligne

1,1
‘

, fin de l'instruction
variable entière ‘’*’
variable chaîne '$’
variable réelle ’!"

variable sans marque

constante 0
constante
constante
constante
constante
constante
constante
constante
constante
constante

w © “NN OÙ  U1 EE 1N ND —

valeur sur un octet

valeur deux octets,
valeur deux octets,
valeur deux octets,
adresse de ligne
numéro de ligne

valeur à virgule flottante

AFTER
AUTO
BORDER
CALL
CAT
CHAIN
CLEAR
CLG
CLOSEIN
CLOSEOUT
CLS
CONT
DATA
DEF
DEFINT
DEFREAL
DEFSTR
DEG
DELETE
DIM
DRAW
DRAWR
EDIT
ELSE

décimal
binaire

hexa

-IV 25-

END
ENT

ENV
ERASE
ERROR
EVERY
FOR
GOSUB
GOTO

IF

INK
INPUT
KEY

LET

LINE
LIST
LOAD
LOCATE
MEMORY
MERGE
MIDS$
MODE
MOVE
MOVER
NEXT
NEW

ON

ON BREAK
ON ERROR GOTO 0
ON SQ
OPENIN
OPENOUT
ORIGIN
OUT
PAPER
PEN
PLOT
PLOTR
POKE
PRINT

RAD
RANDOMIZE
READ
RELEASE
REM
RENUM

RESTORE
RESUME
RETURN
RUN
SAVE
SOUND
SPEED
SYMBOL
TAG
TAGOFF
TRON
TROFF
WAIT
WEND
WHILE
WIDTH
WINDOW
ZONE

"Backslash'

547

AND
MOD
OR

XOR
NOT
Funktion
STOP

-IV 26-

Le token &FF précède une
suivants:

ABS
ASC
ATN
CHR$
CINT
cos
CREAL
EXP
FIX

FRE
INKEY
INP

INT
JOY
LEN
LOG
LOG10
LOWER$
PEEK
REMAIN
SGN
SIN
SPACES
sa

SOR
STR$
TAN
UNT
UPPERS$
VAL
EOF
ERR
HIMEM
INKEY$
PI

RND
TIME
XPOS
YPOS

fonction

-IV 27-

. 1]

peut être suivi

BIN$
DECS$
HEXS
INSTR
LEFT$
MAX
MIN

POS
RIGHTS
ROUND
STRINGS
TEST
TESTR
‘Improper argument’
VPOS

des tokens

AMSTRAD OUVRE-TOt

Le Don départ avec le CPC 464!

(Ce livre vous apporte les prinçie

pates informations sur l'utilise

hon, les possibilités Ge con-
‘.nexions du CPC 464 nt les

| nateurs avec le CPC 464

oe  -
Rai M0

fous les programmes sont prèts
& étre lapes et abondamment
commentés.

Pix: 420F TC
Ref: ML 110

92500 RUEIL-MALMAISON
‘147, av. Paul Doumer
.:(1)7329258
 Telex : MA 205944 F.

NUMÉRO 1 - MAI 85 - Prix 15 F. Belgique 120 FB. Suisse 4 FS.

I Nous aimerions que l'information circule dans les deux sens !
, Cette page vous est réservée.

Matériel utilisé :
Date d'Achat :
Extension/Périphérique :

Logiciel préféré :

Li
| ] Etes-vous satisfait des logiciels existant ? Oui Non
Si oui, lesquels ?

Si non, quels sont les logiciels que vous aimeriez trouver ?

Que pensez-vous des logiciels MICRO APPLICATION ?

Que pensez-vous des livres MICRO APPLICATION ?

1
1
l
4
r—
J
|
I
1
I Que pensez-vous de la revue ?

j J Votre rubrique personnelle :

Attention Micro Info n'étant tiré qu’à 10 000 exemplaires.
re Réservez dès à présent le numéro spécial Rentrée 85 plus les 3 prochains
ra pour la somme de 60 FF,

LU ent par chèque bancaire ou CCP uniquement.

Bulletin d'abonnement :

NOM

Pr Éno à
Adresse :

Code Postal : Ville :

Li :
Z
Z
[e)
Fe
Le.

Nos petites Annonces gratuites seront réservées en priorité aux abonnés.

Achevé d'imprimer en juin 1985
sur les presses de l'imprimerie Laballery et C*
58500 Clamecy
Dépôt légal : juin 1985
N° d'impression : 505064

PLANS DÉTAILLÉS
DE LA MACHINE

CPC464- CIRCUIT DIAGRAM

SOUAD CLOCK € 147)
AS
Al4
—|2 à

> 2 cme se Aï4
> S SAR - A8 Rs 1 ps
> ? FIRE! : 47
> 6 FIRE 2 PADDLE
> 4 RIGHT PORT as
> pa LEFT 8 PIN *D° ne æ
> 2 DONN CONNECTOR 07 7 D?
> Hœ D6 De
> pt m6 Le
> 6 CON A6 E Ds
> ns D4 D4
> 50
> v3 LE vs
> ve De
> L A10 D.
> #2 pa s148 2 > 2 YIO x1 x2 x9 x4 n5 6 x7 x8 x9 V.DeU va vo
> a 7 DECODER 8 CS CONTROLLER
> COST) 7 12 Ye :
> 6 > 13 7
> s 14  _Y6 ET
> 4 D 15  YS
> 1c107 3 p 18 Ye CONNECTER [RE L PEN
> ere 2 7 ” (20 MAY 2.58 IN LINE ) ne”
> 1 - 4
> 0 18  Y1 CLRSOR
> A 3 el 1c110
> 2 RESET 7ALS132
> 10 a
> ET É<] a s 100
> si DH HOTUR 1OVR
> 32 5 > 6 SOUD TAPE
> ___ + ES SRE k PORT 1 FE
> 54 : DE RD DATA = > TRE
> à se 5 mu [LU 7 a L
> É PE +5 2.94 ) SE
> ——+ è ee CPOCt 6 Dé PRINTER
> a 0 S 3 PORT
> 36 HAY
> 4 æ D. 1 "EDGE
> 6 3 D! CON
> A 1 Do 2 Do
> ro SA 3 07
> _— 5 | A0 :
> 10 æ | © 11° BUSY
> 10m "mR 3 Es. ao
> 13
> Cd 10m m1 141619220021 222223»
> 12

+ + à CPAS A12 }_> 24:22: 27528638
    R10! 8 ë 2,2% 10110
    2508 ÿ 7ALS132 jet10
    ÿ 1 à Pan ee
    ET É eu
    co 2 5
    1007 o SE HS
    ne San
    A ri me A4
    Lies R110
    As 2,2%
    ae eq A1
    ati BUSRA à
    A0 Eu R120
    A8 NI . 1
    8 READY = "7
    pa ë A
    5 nn
    BUEXK TMM 232561 150
    #3
    ne] HALT
    2
    A
    __ A0
    Lu ES
    LL
    RFSA
    8
    8 8 11
    ” , CRT
    D6 Do 5 101. à
    Ds YSx D 8
    Dé F2 a Le]
    : #5
    READY READY
    D A140 100 = PULL Fa

# 45 EM ; OT] RE&T Cd RESET vo Ris 129

; ZDG12L 12
jan Ri4i 6 C> É 28 hr <PASSIVE vect 12 _
CONECTOR «2 “A PULL vert
cpooi L ME d =

- Er du x ne
  COS TIRE RD Éd RD PU us ;
  ges 28 TORd d Too ap
  5 Tr 10 Q) TRTEREET FR
  Jo (O.C.K PULL WP) ose
  DÉCOUPL NS
  TORD ROMEN 2
  EXPANSION_CONNECTOR STMAY 0.1*EDGE CUNN 1

FO WR IURQ

10 11 12 13 14 15 18 17 18 19 20 21 2s 24

,

SOUND AIS A1!3 A1
BD A4 AIR

LE A7 4s LC At
A10 8 #6 M LOU A0 LS œ% me Do

3
CI

LNH

DS ot #

\ oo
NES FH RO HAT M Bs

28 30 31
Li

16

icttt
Z80 DATA BUS

EXPANSION BUS

2N 7400

12 4
6

E À

13

VIDEO

OUTPUT
CONNECTER
J101 C GPIN DIN )

R143
©

# c03

16.047 22P NPO

Xx101

PCB302

vrac LEO CPC464 CASSETTE DECK CIRCUIT DIAGRAM

PCB301

MTOOO À PC-/

1-2558

IMPRIMÉ EN FRANCE

LA BIBLE DE L'AMSTRAD CPC est une aide indispensa-
ble pour les programmeurs en BASIC et le MUST
absolu pour les programmeurs en assembleur. Cet
ouvrage de référence qui révèle vraiment tous les
secrets du CPC, est le fruit d'un travail minutieux de
plusieurs mois.
Contenu :
e organisation de la mémoire
e le processeur
e particularité du Z 80, du CPC
e GATE ARRAY
ele contrôleur vidéo
ela ROM vidéo
ele CHIP sonore
eles interfaces
e les systèmes d'exploitation
e Utilisation des routines avec l'exemple
du HARD COPY
ele générateur de caractères
e l'interpréteur BASIC
e BASIC et langage machine |
«le listing de la ROM SN: 2:86299-01-8
e eÏc. Réf. : ML 122
