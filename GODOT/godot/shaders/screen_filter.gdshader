// Filtry ekranu (v0.21.53) – jeden przebieg na teksturze ekranu, tryb z jednolitego uniformu, najwyżej kilka próbek
// na piksel (telefon w pionie). Tryby jak ScreenFilter.ModeOf:
// 1 Noir (czerń i biel, kontrast, ziarno, winieta; czerwień zagrożeń przygaszona, nie szara),
// 2 Retro LCD (4 odcienie zieleni z ditheringiem Bayera i kratką pikseli UI),
// 3 Neon nocy (róż i błękit, przesunięcie kanałów, linie, poświata jasnych miejsc, rzadkie drgnięcie taśmy),
// 4 Kwas (obrót barwy w czasie i po ekranie, falowanie, rozlana barwa),
// 5-7 daltonizacja (Protanopia, Deuteranopia, Tritanopia: błąd symulacji LMS przeniesiony na widoczne kanały),
// 8 Wysoki kontrast (nasycenie, krzywa kontrastu, ciemniejsze tło).
// motion = false (ograniczony ruch): bez falowania, drgnięć i animacji ziarna / barwy.
shader_type canvas_item;
render_mode unshaded, blend_disabled;

uniform sampler2D screen_tex : hint_screen_texture, filter_nearest;
uniform int mode = 0;
uniform float strength = 1.0;
uniform float px = 2.0;
uniform bool motion = true;

const float BAYER[16] = float[16](0.0, 8.0, 2.0, 10.0, 12.0, 4.0, 14.0, 6.0, 3.0, 11.0, 1.0, 9.0, 15.0, 7.0, 13.0, 5.0);

float hash(vec2 p) { return fract(sin(dot(p, vec2(12.9898, 78.233))) * 43758.5453); }
float luma(vec3 c) { return dot(c, vec3(0.299, 0.587, 0.114)); }
vec3 saturate3(vec3 c, float s) { return mix(vec3(luma(c)), c, s); }

vec3 hue_rot(vec3 c, float a) {
	const vec3 k = vec3(0.57735);
	float ca = cos(a);
	return c * ca + cross(k, c) * sin(a) + k * dot(k, c) * (1.0 - ca);
}

vec3 daltonize(vec3 c, int m) {
	vec3 lms = vec3(17.8824 * c.r + 43.5161 * c.g + 4.11935 * c.b,
					3.45565 * c.r + 27.1554 * c.g + 3.86714 * c.b,
					0.0299566 * c.r + 0.184309 * c.g + 1.46709 * c.b);
	vec3 s = lms;
	if (m == 5) s.x = 2.02344 * lms.y - 2.52581 * lms.z;
	else if (m == 6) s.y = 0.494207 * lms.x + 1.24827 * lms.z;
	else s.z = -0.395913 * lms.x + 0.801109 * lms.y;
	vec3 sim = vec3(0.0809444479 * s.x - 0.130504409 * s.y + 0.116721066 * s.z,
					-0.0102485335 * s.x + 0.0540193266 * s.y - 0.113614708 * s.z,
					-0.000365296938 * s.x - 0.00412161469 * s.y + 0.693511405 * s.z);
	vec3 e = c - sim;
	vec3 corr = m == 7 ? vec3(e.r + 0.7 * e.b, e.g + 0.7 * e.b, 0.0) : vec3(0.0, 0.7 * e.r + e.g, 0.7 * e.r + e.b);
	vec3 o = clamp(c + corr * 1.4, 0.0, 1.0);
	return clamp((o - 0.5) * 1.08 + 0.5, 0.0, 1.0);
}

void fragment() {
	vec2 uv = SCREEN_UV;
	vec2 upx = SCREEN_PIXEL_SIZE * px;
	float t = motion ? TIME : 0.0;
	vec3 src = texture(screen_tex, uv).rgb;
	vec3 o = src;
	if (mode == 1) {
		float l = clamp((luma(src) - 0.5) * 1.45 + 0.52, 0.0, 1.0);
		float red = clamp(src.r - max(src.g, src.b) * 1.1, 0.0, 1.0);
		o = mix(vec3(l), vec3(min(1.0, l * 1.15 + 0.18), l * 0.32, l * 0.3), smoothstep(0.1, 0.35, red) * 0.9);
		o += (hash(floor(FRAGCOORD.xy / px) + floor(t * 18.0)) - 0.5) * 0.09;
		vec2 d = uv - 0.5;
		o *= 1.0 - dot(d, d) * 0.95;
	} else if (mode == 2) {
		vec2 up = floor(FRAGCOORD.xy / px);
		vec2 q = mod(up, 4.0);
		float b = (BAYER[int(q.x) + int(q.y) * 4] + 0.5) / 16.0;
		float l = pow(luma(src), 0.85);
		float lv = clamp(floor(l * 3.0 + (b - 0.5) * 0.85 + 0.5), 0.0, 3.0);
		o = lv < 0.5 ? vec3(0.06, 0.22, 0.06) : lv < 1.5 ? vec3(0.19, 0.38, 0.19) : lv < 2.5 ? vec3(0.55, 0.67, 0.06) : vec3(0.61, 0.74, 0.06);
		vec2 f = fract(FRAGCOORD.xy / px);
		if (px >= 2.0 && (f.x < 1.0 / px || f.y < 1.0 / px)) o *= 0.86;
	} else if (mode == 3) {
		float burst = step(0.93, hash(vec2(floor(t * 7.0), 3.0)));
		uv.x += (hash(vec2(floor(uv.y * 140.0), floor(t * 30.0))) - 0.5) * 0.008 * burst;
		float ca = upx.x * 1.5;
		vec3 c = vec3(texture(screen_tex, uv + vec2(ca, 0.0)).r, texture(screen_tex, uv).g, texture(screen_tex, uv - vec2(ca, 0.0)).b);
		float l = luma(c);
		vec3 hot = mix(vec3(1.0, 0.16, 0.7), vec3(0.12, 0.95, 1.0), smoothstep(0.45, 0.85, l));
		vec3 grade = mix(vec3(0.15, 0.02, 0.28), hot, smoothstep(0.04, 0.5, l));
		o = mix(saturate3(c, 1.8), grade * (0.55 + l), 0.5);
		vec2 go = upx * 4.0;
		vec3 h = max(texture(screen_tex, uv + go).rgb, texture(screen_tex, uv - go).rgb);
		o += max(luma(h) - 0.62, 0.0) * vec3(1.0, 0.3, 0.9) * 0.9;
		if (mod(floor(FRAGCOORD.y / max(1.0, px * 0.5)), 2.0) < 1.0) o *= 0.8;
	} else if (mode == 4) {
		if (motion) {
			uv.x += sin(uv.y * 18.0 + t * 2.2) * 0.004 * strength;
			uv.y += sin(uv.x * 14.0 + t * 1.7) * 0.003 * strength;
		}
		vec3 c = texture(screen_tex, uv).rgb;
		vec3 c2 = texture(screen_tex, uv + vec2(upx.x * 3.0, 0.0)).rgb;
		c = mix(c, max(c, c2), 0.5);
		o = saturate3(hue_rot(c, t * 0.8 + (uv.x + uv.y) * 2.4), 1.5);
	} else if (mode >= 5 && mode <= 7) {
		o = daltonize(src, mode);
	} else if (mode == 8) {
		float l = luma(src);
		o = saturate3(src, 1.6);
		o = clamp((o - 0.5) * 1.35 + 0.5, 0.0, 1.0);
		o *= mix(0.55, 1.0, smoothstep(0.15, 0.5, l));
	}
	COLOR = vec4(mix(src, clamp(o, 0.0, 1.0), strength), 1.0);
}
