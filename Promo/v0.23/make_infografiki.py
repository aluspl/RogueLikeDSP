"""Infografiki v0.23 (1080x1350): 1 nowości (z Kawką), 2 bronie, 3 co ma gra (przegląd) – styl jak Promo/v0.22. Zrzuty: zrzuty/*.png (Godot, pion, --screenshot --scene ...).
Liczby z GBA/data/game.json v0.23.0: 45 broni (12 domyślnych, 25 do znalezienia, 8 artefaktów), 3 typy ciosu,
4 rzadkości, 5 profitów, sufit krytu 35%."""
from PIL import Image, ImageDraw, ImageFont, ImageFilter
V=(107,78,255); O=(255,122,61); INK=(20,18,40); BG=(246,245,248); W=(255,255,255); DIM=(99,99,102)
AV='/System/Library/Fonts/Avenir Next.ttc'
def F(sz, idx): return ImageFont.truetype(AV, sz, index=idx)
BOLD=lambda s: F(s,0); HEAVY=lambda s: F(s,8); MED=lambda s: F(s,5)
# wycinki zrzutów 860x1864 (x0, y0, x1, y1)
CROP={'weapon-arc':(130,330,730,1030),'hero-weapons':(110,380,790,1173),'weapon-legend-found':(130,440,730,1140),
      'weapon-tip-kielnia':(260,1150,860,1850),'case-some':(0,150,860,1153),'weapon-perks':(0,120,860,1123),
      'hero-gear':(110,380,790,1173),'dog-quip':(220,420,820,975),
      # infografika 3 (Co ma gra): kadry ~1,8:1 (Godot 860 szer., zrzuty z ekranu Retina zmniejszone o połowę; boss-pokonany 540x960 z klipu reklamy; retro-kawka 480x320)
      'o-classselect':(0,110,860,588),'o-hero-weapons':(0,430,860,908),'o-case-some':(0,150,860,628),
      'o-schedule':(0,500,860,955),'o-boss-pokonany':(0,240,540,530),'o-training':(0,200,860,678),
      'o-daily':(0,195,860,673),'o-career':(0,245,860,723),'o-kwas-map':(0,480,860,958),'o-sekrety':(0,190,860,668),
      'o-dog-quip':(130,430,860,815),'o-retro-kawka':(0,0,480,254)}
def card(shot, w, h, r=26):
    """Wycinek zrzutu w zaokrąglonej ramce: skala „cover” (wypełnia ramkę), środek kadru. Klucz 'o-NAZWA' = inny kadr zrzutu NAZWA."""
    im=Image.open(f'zrzuty/{shot.removeprefix("o-")}.png').convert('RGB').crop(CROP[shot])
    s=max(w/im.width,h/im.height); im=im.resize((round(im.width*s),round(im.height*s)), Image.NEAREST if s>=1.5 else Image.LANCZOS)
    im=im.crop(((im.width-w)//2,(im.height-h)//2,(im.width-w)//2+w,(im.height-h)//2+h))
    pad=8; fr=Image.new('RGBA',(w+2*pad,h+2*pad),(0,0,0,0)); d=ImageDraw.Draw(fr)
    d.rounded_rectangle([0,0,w+2*pad-1,h+2*pad-1],radius=r+6,fill=(15,15,22))
    m=Image.new('L',(w,h),0); ImageDraw.Draw(m).rounded_rectangle([0,0,w-1,h-1],radius=r,fill=255)
    fr.paste(im,(pad,pad),m); return fr
def shadow(canvas, img, xy):
    m=40  # margines, żeby rozmycie nie ucinało się na krawędzi warstwy cienia
    sh=Image.new('RGBA',(img.size[0]+2*m,img.size[1]+2*m),(40,20,90,0))
    ImageDraw.Draw(sh).rounded_rectangle([m,m,m+img.size[0]-1,m+img.size[1]-1],radius=32,fill=(40,20,90,60))
    sh=sh.filter(ImageFilter.GaussianBlur(22)); canvas.alpha_composite(sh,(xy[0]-m,xy[1]+10-m)); canvas.alpha_composite(img,xy)
def stripes(d, y, w, h):
    d.rectangle([0,y,w,y+h],fill=(20,20,30))
    for x in range(-h, w+h, 36): d.polygon([(x,y+h),(x+18,y+h),(x+18+h,y),(x+h,y)],fill=O)
def center(d, text, font, y, fill, x0=0, w=1080):
    tw=d.textlength(text,font=font); d.text((x0+(w-tw)/2,y),text,font=font,fill=fill)
def fit(d, text, font_fn, size, maxw):
    f=font_fn(size)
    while d.textlength(text,font=f)>maxw: f=font_fn(f.size-1)
    return f

# ---------- 1: Nowości w wersji 0.23 ----------
c=Image.new('RGBA',(1080,1350),BG+(255,)); d=ImageDraw.Draw(c)
d.rectangle([0,0,1080,230],fill=V)
d.text((60,40),'PlanBudowlany ROGUELIKE',font=HEAVY(58),fill=W)
d.text((60,114),'Nowości w wersji 0.23: bronie',font=MED(36),fill=(230,225,255))
d.text((60,166),'Każda broń ma swój cios, rzadkość i Patent.',font=MED(28),fill=(210,200,255))
stripes(d,230,1080,18)
items=[('weapon-arc','Typy ciosu','obuch, cięcie, kłucie'),('hero-weapons','Rzadkość w rękach','4 poziomy, widać je na mapie'),
       ('weapon-tip-kielnia','Patent','ruch broni z odnowieniem'),('weapon-legend-found','Legendarny nóż','Nóż majstra seniora'),
       ('case-some','Gablota artefaktów','8 artefaktów do odkrycia'),('dog-quip','Kawka','pogłaszcz – oceni twoje postępy')]
cw,ch=316,292
for k,(sh,t,sub) in enumerate(items):
    x=22+(k%3)*346; y=268+(k//3)*396
    p=card(sh,cw,ch); shadow(c,p,(x,y)); d=ImageDraw.Draw(c)
    cy=y+p.size[1]+14
    f1=fit(d,t,BOLD,28,p.size[0]-6); center(d,t,f1,cy,INK,x,p.size[0])
    f2=fit(d,sub,MED,20,p.size[0]-6); center(d,sub,f2,cy+36,DIM,x,p.size[0])
y0=1062
d.rounded_rectangle([45,y0,1035,y0+186],radius=24,fill=W)
d.text((75,y0+16),'I jeszcze:',font=BOLD(26),fill=V)
for k,l in enumerate(['losowe profity broni  ·  pas monterski z kaburą  ·  sprzęt widać na postaci',
                      'problemy obchodzą przeszkody  ·  czytelne boczne przejścia  ·  krótki samouczek',
                      'trudniejszy Normalny  ·  rzadsze kryty (najwyżej 35%)  ·  ogłoszenia z przytupem']):
    d.text((75,y0+56+k*40),l,font=fit(d,l,MED,22,930),fill=INK)
d.rectangle([0,1260,1080,1350],fill=INK)
center(d,'Zagraj: planbudowlany.online',BOLD(34),1282,W)
c.convert('RGB').save('infografika_1_nowosci.png')

# ---------- 2: Bronie od podszewki ----------
c=Image.new('RGBA',(1080,1350),BG+(255,)); d=ImageDraw.Draw(c)
d.rectangle([0,0,1080,230],fill=INK)
d.text((60,44),'Bronie od podszewki',font=HEAVY(58),fill=W)
d.text((60,122),'Przeciwnikami są problemy budowy, nie ludzie.',font=MED(30),fill=(200,200,215))
stripes(d,230,1080,18)
d.text((45,276),'Rzadkość widać w rękach',font=BOLD(34),fill=INK)
p=card('hero-weapons',400,452); shadow(c,p,(45,330)); d=ImageDraw.Draw(c)
rar=[('Zwykła',(138,135,150),'bez profitów'),('Solidna',(92,180,240),'1 losowy profit, +1 obr.'),
     ('Markowa',(107,78,255),'2 profity, krótszy Patent'),('Mistrzowska',(217,164,0),'stały unikat'),
     ('Artefakt',(255,122,61),'własna mechanika, np. piorun')]
for k,(n,col,sub) in enumerate(rar):
    x=490; y=330+k*93
    d.rounded_rectangle([x,y,1035,y+80],radius=20,fill=W)
    d.rounded_rectangle([x,y,x+16,y+80],radius=8,fill=col)
    d.ellipse([x+36,y+22,x+72,y+58],fill=col)
    d.text((x+90,y+8),n,font=BOLD(30),fill=INK)
    d.text((x+90,y+46),sub,font=MED(21),fill=DIM)
d=ImageDraw.Draw(c)
d.text((45,830),'Typ ciosu kontra materiał problemu',font=BOLD(34),fill=INK)
hits=[('Obuch','ogłusza (20%)','×125% na cement'),('Cięcie','kryt +5%','×125% na drewno'),('Kłucie','omija 1 OBR','×125% na stal')]
for k,(n,a,b) in enumerate(hits):
    x=45+k*335; y=886
    d.rounded_rectangle([x,y,x+320,y+150],radius=24,fill=W)
    d.rounded_rectangle([x,y,x+320,y+58],radius=24,fill=V if k%2==0 else O); d.rectangle([x,y+34,x+320,y+58],fill=V if k%2==0 else O)
    center(d,n,BOLD(30),y+10,W,x,320)
    center(d,a,BOLD(26),y+70,INK,x,320)
    center(d,b,MED(22),y+108,DIM,x,320)
stats=[('45','broni'),('8','artefaktów'),('5','profitów'),('4','rzadkości')]  # v0.23.0: z GBA/data/game.json
y0=1066
for k,(v,l) in enumerate(stats):
    x=45+k*252
    d.rounded_rectangle([x,y0,x+230,y0+170],radius=24,fill=W)
    center(d,v,HEAVY(64),y0+18,V,x,230)
    center(d,l,MED(26),y0+104,INK,x,230)
d.rectangle([0,1260,1080,1350],fill=V)
center(d,'iPhone · Android · wersja retro   |   planbudowlany.online',BOLD(30),1286,W)
c.convert('RGB').save('infografika_2_bronie.png')

# ---------- 3: Co ma gra (przegląd całości, v0.23.0) ----------
c=Image.new('RGBA',(1080,1350),BG+(255,)); d=ImageDraw.Draw(c)
d.rectangle([0,0,1080,214],fill=V)
d.text((60,34),'PlanBudowlany ROGUELIKE',font=HEAVY(58),fill=W)
d.text((60,108),'Co jest w grze',font=MED(36),fill=(230,225,255))
d.text((60,156),'Zbuduj dom. Przeciwnikami są problemy budowy.',font=MED(28),fill=(210,200,255))
stripes(d,214,1080,18)
items=[('o-classselect','12 zawodów','każdy z własną bronią i Mocą'),('o-hero-weapons','45 broni','rzadkość, Patent, pas na drugą'),
       ('o-case-some','Artefakty i Gablota','8 artefaktów z własną mechaniką'),('o-schedule','Akty i etapy','błoto, porywy, pył – 10 etapów'),
       ('o-boss-pokonany','Bossowie','9 bossów, zapowiedź ciosu'),('o-training','Szkolenia i Respekt','postęp zostaje między budowami'),
       ('o-daily','Codzienna i Tydzień','nowa budowa co dzień i co tydzień'),('o-career','Kontrakty kariery','5 domów z własnymi etapami'),
       ('o-kwas-map','Filtry ekranu','Noir, Retro LCD, Neon nocy, Kwas'),('o-sekrety','Sekrety','10 wyzwań, ukryte magazyny'),
       ('o-dog-quip','Kawka','pogłaszcz – oceni twoje postępy'),('o-retro-kawka','Wersja retro','ta sama gra w pikselach')]
cw,ch=318,168
for k,(sh,tt,sub) in enumerate(items):
    x=24+(k%3)*346; y=252+(k//3)*252
    p=card(sh,cw,ch,r=20); shadow(c,p,(x,y)); d=ImageDraw.Draw(c)
    cy=y+p.size[1]+8
    f1=fit(d,tt,BOLD,26,p.size[0]-6); center(d,tt,f1,cy,INK,x,p.size[0])
    f2=fit(d,sub,MED,19,p.size[0]-6); center(d,sub,f2,cy+33,DIM,x,p.size[0])
d.rectangle([0,1260,1080,1350],fill=INK)
center(d,'iPhone · Android · wersja retro   |   planbudowlany.online',BOLD(30),1286,W)
c.convert('RGB').save('infografika_3_co_ma_gra.png')
print('ok')
