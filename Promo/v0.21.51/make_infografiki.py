from PIL import Image, ImageDraw, ImageFont, ImageFilter
V=(107,78,255); O=(255,122,61); INK=(20,18,40); BG=(246,245,248); W=(255,255,255); DIM=(99,99,102)
AV='/System/Library/Fonts/Avenir Next.ttc'
def F(sz, idx): return ImageFont.truetype(AV, sz, index=idx)
BOLD=lambda s: F(s,0); HEAVY=lambda s: F(s,8); MED=lambda s: F(s,5)
def phone(shot, w):
    im=Image.open(f'zrzuty/{shot}.png').convert('RGB'); h=int(im.height*w/im.width)
    im=im.resize((w,h), Image.LANCZOS)
    pad=10; fr=Image.new('RGBA',(w+2*pad,h+2*pad),(0,0,0,0)); d=ImageDraw.Draw(fr)
    d.rounded_rectangle([0,0,w+2*pad-1,h+2*pad-1],radius=34,fill=(15,15,22))
    m=Image.new('L',(w,h),0); ImageDraw.Draw(m).rounded_rectangle([0,0,w-1,h-1],radius=26,fill=255)
    fr.paste(im,(pad,pad),m); return fr
def shadow(canvas, img, xy):
    sh=Image.new('RGBA',img.size,(0,0,0,0)); ImageDraw.Draw(sh).rounded_rectangle([0,0,img.size[0]-1,img.size[1]-1],radius=34,fill=(40,20,90,60))
    sh=sh.filter(ImageFilter.GaussianBlur(22)); canvas.alpha_composite(sh,(xy[0],xy[1]+10)); canvas.alpha_composite(img,xy)
def stripes(d, y, w, h):
    d.rectangle([0,y,w,y+h],fill=(20,20,30))
    for x in range(-h, w+h, 36): d.polygon([(x,y+h),(x+18,y+h),(x+18+h,y),(x+h,y)],fill=O)
def center(d, text, font, y, fill, W_=1080):
    tw=d.textlength(text,font=font); d.text(((W_-tw)/2,y),text,font=font,fill=fill)

# ---------- 1: Co nowego ----------
c=Image.new('RGBA',(1080,1350),BG+(255,)); d=ImageDraw.Draw(c)
d.rectangle([0,0,1080,250],fill=V)
d.text((60,48),'PlanBudowlany ROGUELIKE',font=HEAVY(58),fill=W)
d.text((60,122),'Nowości w wersji 0.21.50 i 0.21.51',font=MED(36),fill=(230,225,255))
d.text((60,176),'Każda budowa inna. Każda porażka czegoś uczy.',font=MED(28),fill=(210,200,255))
stripes(d,250,1080,18)
items=[('boon-pick','Premia 1 z 3','po każdym etapie, 8 synergii'),('combo-shock','Kombinacje stanów','mokry + prąd = porażenie'),
       ('elite-map','Elity','złota ramka, lepsza nagroda'),('event-choices','Wydarzenia z wyborem','SMS: ryzyko albo zysk')]
pw=226
for k,(sh,t,sub) in enumerate(items):
    x=22+k*262; y=292
    p=phone(sh,pw); shadow(c,p,(x,y)); d=ImageDraw.Draw(c)
    cy=y+p.size[1]+22
    f1=BOLD(26 if len(t)<18 else 23); tw=d.textlength(t,font=f1); d.text((x+(p.size[0]-tw)/2,cy),t,font=f1,fill=INK)
    tw=d.textlength(sub,font=MED(19)); d.text((x+(p.size[0]-tw)/2,cy+36),sub,font=MED(19),fill=DIM)
y0=960
d.rounded_rectangle([45,y0,1035,y0+150],radius=24,fill=W)
d.text((75,y0+22),'I jeszcze:',font=BOLD(26),fill=V)
d.text((75,y0+62),'ukryte magazyny  ·  ulepszanie narzędzi  ·  rozpiska obrażeń',font=MED(24),fill=INK)
d.text((75,y0+98),'podsumowanie budowy  ·  wyzwania tygodnia  ·  sekretne zlecenia',font=MED(24),fill=INK)
d.rectangle([0,1260,1080,1350],fill=INK)
center(d,'Zagraj: planbudowlany.online',BOLD(34),1282,W)
d.text((60,1140),'Premie, elity i kombinacje sprawiają, że żadna budowa',font=MED(28),fill=INK)
d.text((60,1180),'nie jest taka sama. Porażka? Respekt i doświadczenie zostają.',font=MED(28),fill=INK)
c.convert('RGB').save('infografika_1_nowosci.png')

# ---------- 2: Twoja budowa ----------
c=Image.new('RGBA',(1080,1350),BG+(255,)); d=ImageDraw.Draw(c)
d.rectangle([0,0,1080,230],fill=INK)
d.text((60,44),'Jak wygląda budowa',font=HEAVY(58),fill=W)
d.text((60,122),'Przeciwnikami są problemy budowy, nie ludzie.',font=MED(30),fill=(200,200,215))
stripes(d,230,1080,18)
steps=[('title','1','Wybierz fach','12 zawodów z mocami'),('game','2','Przejdź etap','mgła, błoto, kałuże'),
       ('boss','3','Pokonaj bossa','uniknij zapowiedzi ciosu'),('recap-death','4','Wyciągnij wnioski','podsumowanie budowy')]
for k,(sh,n,t,sub) in enumerate(steps):
    x=22+k*262; y=282
    d.ellipse([x+95,y,x+141,y+46],fill=V if k%2==0 else O); d=ImageDraw.Draw(c)
    tw=d.textlength(n,font=BOLD(28)); d.text((x+118-tw/2,y+5),n,font=BOLD(28),fill=W)
    p=phone(sh,226); shadow(c,p,(x,y+62)); d=ImageDraw.Draw(c)
    cy=y+62+p.size[1]+18
    tw=d.textlength(t,font=BOLD(25)); d.text((x+(p.size[0]-tw)/2,cy),t,font=BOLD(25),fill=INK)
    tw=d.textlength(sub,font=MED(19)); d.text((x+(p.size[0]-tw)/2,cy+34),sub,font=MED(19),fill=DIM)
acts=[('Akt 0','Papierologia'),('Akt I','Stan surowy'),('Akt II','Pod dachem'),('Akt III','Wykończenie')]
y1=930
for k,(a_,n) in enumerate(acts):
    x=45+k*252
    d.rounded_rectangle([x,y1,x+230,y1+80],radius=20,fill=V if k%2==0 else O)
    tw=d.textlength(a_,font=BOLD(24)); d.text((x+(230-tw)/2,y1+8),a_,font=BOLD(24),fill=W)
    tw=d.textlength(n,font=MED(22)); d.text((x+(230-tw)/2,y1+42),n,font=MED(22),fill=W)
    if k<3: d.polygon([(x+236,y1+30),(x+248,y1+40),(x+236,y1+50)],fill=INK)
stats=[('12','etapów'),('41','problemów'),('41','premii'),('12','zawodów')]
y0=1060
for k,(v,l) in enumerate(stats):
    x=45+k*252
    d.rounded_rectangle([x,y0,x+230,y0+150],radius=24,fill=W)
    tw=d.textlength(v,font=HEAVY(64)); d.text((x+(230-tw)/2,y0+14),v,font=HEAVY(64),fill=V)
    tw=d.textlength(l,font=MED(26)); d.text((x+(230-tw)/2,y0+98),l,font=MED(26),fill=INK)
d.rectangle([0,1260,1080,1350],fill=V)
center(d,'iPhone · Android · wersja retro   |   planbudowlany.online',BOLD(30),1286,W)
c.convert('RGB').save('infografika_2_budowa.png')
print('ok')
