import re

file_path = r'c:\Users\vince\Documents\My Games\Terraria\tModLoader\ModSources\ElementalHearts\Localization\en-US_Mods.ElementalHearts.hjson'

jokes = {
    'DirtHeart': 'the rarest block in the entire game',
    'CoralstoneHeart': 'Aquarus, the King of Atlantis',
    'IceHeart': 'arctic snow caps',
    'SlimeHeart': 'wobbly blue gelatin',
    'WoodHeart': 'splinters you get from punching trees',
    'FleshHeart': 'uncomfortably squishy walls',
    'CogHeart': 'overpriced Steampunker gears',
    'DemoniteHeart': 'spooky purple rocks',
    'RainbowHeart': 'skittles but dangerous',
    'ZenithHeart': 'literal protagonist plot armor',
    'HayHeart': 'allergies acting up',
    'GlassHeart': 'shattering immediately upon impact',
    'SpookyHeart': 'Halloween decorations left up until January',
    'IchorHeart': 'golden shower that we do not question',
    'EctoplasmHeart': 'ghostly goo from dead paladins',
    'StoneHeart': 'generic grey rocks',
    'MudHeart': 'dirty wet earth',
    'SandHeart': 'coarse, rough, irritating sand that gets everywhere',
    'SnowHeart': 'frozen water that makes you cold',
    'BorealWoodHeart': 'IKEA furniture',
    'RichMahoganyHeart': 'expensive tropical tables',
    'EbonwoodHeart': 'evil purple planks',
    'ShadewoodHeart': 'bloody red planks',
    'PearlwoodHeart': 'fairy tale trees',
    'PalmWoodHeart': 'beach vacations',
    'DynastyHeart': 'traveling merchant scams',
    'CactusHeart': 'prickly desert plants',
    'PumpkinHeart': 'spiced lattes',
    'GlowingMushroomHeart': 'psychedelic fungus',
    'GraniteHeart': 'kitchen countertops',
    'MarbleHeart': 'ancient greek statues',
    'CandyCaneHeart': 'peppermint cavities',
    'HoneyHeart': 'sticky bee vomit',
    'LesionHeart': 'decaying brain matter',
    'ObsidianHeart': 'lava meeting water',
    'CloudHeart': 'fluffy sky cotton',
    'RainCloudHeart': 'depressing weather',
    'SnowCloudHeart': 'blizzard warnings',
    'DesertFossilHeart': 'dinosaur bones',
    'SunplateHeart': 'floating island debris',
    'BubbleHeart': 'soapy spheres of air',
    'PixieDustHeart': 'annoying fairy glitter',
    'VertebraeHeart': 'chiropractor nightmares',
    'RottenChunkHeart': 'expired meat',
    'CursedFlameHeart': 'green fire that won\'t go out',
    'CrystalShardHeart': 'shiny pink rocks',
    'FallenStarHeart': 'wishes upon a shooting star',
    'DefenderMedalHeart': 'tower defense minigames',
    'CopperHeart': 'pennies',
    'TinHeart': 'canned beans',
    'IronHeart': 'anvils falling from the sky',
    'LeadHeart': 'radiation poisoning',
    'SilverHeart': 'second place medals',
    'TungstenHeart': 'lightbulb filaments',
    'GoldHeart': 'pirate booty',
    'PlatinumHeart': 'credit cards',
    'CrimtaneHeart': 'bloody red rocks',
    'MeteoriteHeart': 'space rocks crashing into your world',
    'HellstoneHeart': 'burning your feet on hot coals',
    'CobaltHeart': 'blue hardmode rocks',
    'PalladiumHeart': 'orange hardmode rocks',
    'MythrilHeart': 'green hardmode rocks',
    'OrichalcumHeart': 'pink hardmode rocks',
    'AdamantiteHeart': 'red hardmode rocks',
    'TitaniumHeart': 'grey hardmode rocks',
    'ChlorophyteHeart': 'aggressive jungle plants',
    'HallowedHeart': 'blinding holy light',
    'ShroomiteHeart': 'invisible mushroom ninjas',
    'SpectreHeart': 'spooky ghost metal',
    'LuminiteHeart': 'moon lord\'s glowing tears',
    'AmethystHeart': 'purple gems',
    'TopazHeart': 'yellow gems',
    'SapphireHeart': 'blue gems',
    'EmeraldHeart': 'green gems',
    'RubyHeart': 'red gems',
    'DiamondHeart': 'expensive engagement rings',
    'AmberHeart': 'fossilized tree sap',
    'SoulOfLightHeart': 'underground rainbows',
    'SoulOfNightHeart': 'underground nightmares',
    'SoulOfFlightHeart': 'annoying sky noodles',
    'SoulOfMightHeart': 'mechanical worm segments',
    'SoulOfSightHeart': 'mechanical laser eyes',
    'SoulOfFrightHeart': 'mechanical skulls',
    'MechanicalHeart': 'the mechanical terror trilogy',
    'EyeHeart': 'staring contests',
    'BrainHeart': 'massive headaches',
    'WormHeart': 'underground segmented nightmares',
    'HiveHeart': 'not the bees',
    'BoneHeart': 'spooky scary skeletons',
    'DeerclopsHeart': 'wintery cyclops deer',
    'WallOfFleshHeart': 'running for your life in hell',
    'QueenSlimeHeart': 'bouncy pink royalty',
    'PlantHeart': 'angry jungle flowers',
    'LihzahrdHeart': 'golem\'s power cell',
    'DukeFishronHeart': 'angry mutated pigs',
    'EmpressHeart': 'bullet hell dodges',
    'CultistHeart': 'lunatic rituals',
    'CelestialHeart': 'alien invasions',
    'MourningWoodHeart': 'burning spooky trees',
    'HorsemanHeart': 'pumpkins on horseback',
    'ElfHeart': 'santa\'s angry helpers',
    'SlaughterHeart': 'robot santas',
    'FlyingDutchmanHeart': 'ghost pirates',
    'MartianHeart': 'alien saucers',
    'BetsyHeart': 'angry dragons',
    'RoyalSlimeHeart': 'bouncy blue royalty',
    'SnotHeart': 'disgusting green slime',
    'SoaringHeart': 'flying sky worms',
    'TruffleHeart': 'fungal crab nightmares',
    'VolatileHeart': 'explosive underground meat',
    'XenoHeart': 'foreign alien invaders',
    'EbonstoneHeart': 'evil purple rocks',
    'PearlstoneHeart': 'hallowed pink rocks',
    'CrimstoneHeart': 'bloody red rocks',
    'MushroomHeart': 'glowing blue fungus',
    'PearlsandHeart': 'hallowed pink sand',
    'PinkIceHeart': 'hallowed pink ice',
    'PurpleIceHeart': 'evil purple ice',
    'RedIceHeart': 'bloody red ice',
    'EbonsandHeart': 'evil purple sand',
    'CrimsandHeart': 'bloody red sand'
}

def get_joke(name):
    if name in jokes: return jokes[name]
    base = name.replace('Heart', '')
    return f'the comedic essence of {base}'

with open(file_path, 'r', encoding='utf-8') as f:
    lines = f.readlines()

new_lines = []
in_items = False
current_item = None
inside_item_block = False

for line in lines:
    if line.startswith('Items: {'):
        in_items = True
    
    if in_items:
        match = re.match(r'^\t([a-zA-Z0-9_]+): \{$', line)
        if match:
            current_item = match.group(1)
            inside_item_block = True
            new_lines.append(line)
            continue
            
        if inside_item_block and line.strip() == '}':
            joke = get_joke(current_item)
            new_lines.append(f'\t\tElementalPower: "{joke}"\n')
            inside_item_block = False
            current_item = None
            
    if not (inside_item_block and 'ElementalPower:' in line):
        new_lines.append(line)
        
with open(file_path, 'w', encoding='utf-8') as f:
    f.writelines(new_lines)

print('Modified successfully.')
