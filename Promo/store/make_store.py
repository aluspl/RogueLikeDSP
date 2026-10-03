from PIL import Image, ImageDraw, ImageFont, ImageFilter
V=(107,78,255); O=(255,122,61); INK=(20,18,40); W=(255,255,255)
AV='/System/Library/Fonts/Avenir Next.ttc'
F=lambda s,i: ImageFont.truetype(AV,s,index=i)
caps=[('title','Zbuduj dom','Roguelike, w którym walczysz z problemami budowy'),
 ('game','Każda budowa inna','Mgła, błoto, kałuże i ulewa'),
 ('boon-pick','Premia 1 z 3','po każdym etapie, z synergiami'),
 ('combo-shock','Mokry + prąd','kombinacje stanów i żywiołów'),
 ('boss','Bossowie aktów','unikaj zapowiedzi ciosu'),
 ('event-choices','Wydarzenia z wyborem','ryzyko albo pewny zysk'),
 ('recap-death','Porażka uczy','podsumowanie i postęp zostaje'),
 ('act0-stamps','Najpierw papiery','Akt 0: pozwolenie i przyłącza')]
def make(folder,W_,H_,out):
    import os; os.makedirs(out,exist_ok=True)
    for k,(sc,t,s) in enumerate(caps):
        c=Image.new('RGB',(W_,H_),V); d=ImageDraw.Draw(c)
        hb=int(H_*0.17)
        ft=F(int(W_*0.075),8); fs=F(int(W_*0.036),5)
        tw=d.textlength(t,font=ft); d.text(((W_-tw)/2,hb*0.22),t,font=ft,fill=W)
        tw=d.textlength(s,font=fs); d.text(((W_-tw)/2,hb*0.62),s,font=fs,fill=(230,225,255))
        im=Image.open(f'raw/{folder}/{sc}.png').convert('RGB')
        sw=int(W_*0.86); sh=int(im.height*sw/im.width)
        if hb+sh>H_-int(W_*0.04): sh=H_-hb-int(W_*0.04); sw=int(im.width*sh/im.height)
        im=im.resize((sw,sh),Image.LANCZOS)
        m=Image.new('L',(sw,sh),0); ImageDraw.Draw(m).rounded_rectangle([0,0,sw-1,sh-1],radius=int(W_*0.04),fill=255)
        x=(W_-sw)//2; y=hb
        sh_=Image.new('RGBA',(W_,H_),(0,0,0,0)); ImageDraw.Draw(sh_).rounded_rectangle([x,y+12,x+sw,y+sh+12],radius=int(W_*0.04),fill=(30,10,80,120))
        sh_=sh_.filter(ImageFilter.GaussianBlur(18)); c=Image.alpha_composite(c.convert('RGBA'),sh_).convert('RGB')
        c.paste(im,(x,y),m)
        d=ImageDraw.Draw(c)
        for xx in range(-40,W_+40,46): d.polygon([(xx,H_),(xx+22,H_),(xx+22+18,H_-18),(xx+18,H_-18)],fill=O)
        c.save(f'{out}/{k+1:02d}_{sc}.png')
make('play',1080,1920,'play_phone')
make('ios69',1320,2868,'ios_6.9')
make('ios65',1284,2778,'ios_6.5')
# feature graphic 1024x500
c=Image.new('RGB',(1024,500),V); d=ImageDraw.Draw(c)
g=Image.open('raw/play/game.png').convert('RGB').resize((380,676)); c.paste(g.crop((0,100,380,600)).resize((300,395)),(690,52))
d.text((50,120),'Plan Budowlany',font=F(64,8),fill=W); d.text((50,200),'ROGUELIKE',font=F(56,8),fill=O)
d.text((52,290),'Zbuduj dom. Przetrwaj budowę.',font=F(30,5),fill=(230,225,255))
for xx in range(-40,1064,46): d.polygon([(xx,500),(xx+22,500),(xx+40,482),(xx+18,482)],fill=O)
c.save('feature_graphic_1024x500.png')
Image.open('../../GODOT/godot/icon.png').convert('RGB').resize((512,512),Image.NEAREST).save('icon_512.png')
print('ok')
