"""Render Nightfall 3 audio locally. Piper and its model are in SourceAssets/Piper."""

import math
import random
import re
import struct
import sys
import wave
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
TOOLS = ROOT / "SourceAssets" / "Piper"
ASSETS = ROOT / "Assets" / "CyberShield" / "DataCenterMobile"
RATE = 22050


def write_audio(path, samples):
    path.parent.mkdir(parents=True, exist_ok=True)
    with wave.open(str(path), "wb") as output:
        output.setparams((1, 2, RATE, 0, "NONE", "not compressed"))
        output.writeframes(b"".join(
            struct.pack("<h", round(32767 * max(-1, min(1, value))))
            for value in samples
        ))


def chord_cue(path, pitches, spacing):
    duration = len(pitches) * spacing + 0.32
    def sample(t):
        result = 0
        for index, pitch in enumerate(pitches):
            age = t - index * spacing
            if age >= 0:
                frequency = 440 * 2 ** ((pitch - 69) / 12)
                envelope = min(1, age * 180) * math.exp(-age * 12)
                angle = math.tau * frequency * age
                result += envelope * (math.sin(angle) + .24 * math.sin(angle * 2)) * .28
        return result
    write_audio(path, (sample(i / RATE) for i in range(round(duration * RATE))))


def soundtrack():
    """Eight bars with kick, clap, bass, and syncopated lead."""
    beat_length = 60 / 108
    roots = [38, 34, 41, 36]
    lead = [12, 19, 15, 24, 12, 17, 19, 15]
    rng = random.Random(36)
    count = round(32 * beat_length * RATE)
    for i in range(count):
        t = i / RATE
        beat = t / beat_length
        beat_number = int(beat)
        age = (beat % 1) * beat_length
        subdivision = ((beat * 2) % 1) * beat_length / 2
        root = roots[beat_number // 8 % 4]
        frequency = 440 * 2 ** ((root - 69) / 12)
        bass_angle = math.tau * frequency * subdivision
        bass = .22 * (math.sin(bass_angle) + .2 * math.sin(3 * bass_angle))
        bass *= min(1, subdivision * 160) * math.exp(-subdivision * 9)
        kick = .3 * math.sin(math.tau * (43 * age + 1.5 * (1 - math.exp(-35 * age))))
        kick *= min(1, age * 240) * math.exp(-age * 17)
        noise = rng.uniform(-1, 1)
        drums = .035 * noise * math.exp(-subdivision * 80)
        if beat_number % 4 in (1, 3):
            drums += .12 * noise * min(1, age * 250) * math.exp(-age * 30)
        note = frequency * 2 ** (lead[int(beat * 2) % 8] / 12)
        phase = math.tau * note * subdivision
        melody = .085 * (math.sin(phase) + .18 * math.sin(2 * phase))
        melody *= min(1, subdivision * 100) * math.exp(-subdivision * 8)
        edge = min(1, i / 220, (count - i - 1) / 220)
        yield edge * (bass + kick + drums + melody)


def voices():
    sys.path.insert(0, str(TOOLS / "runtime"))
    from piper import PiperVoice, SynthesisConfig

    model = PiperVoice.load(TOOLS / "es_MX-ald-medium.onnx")
    script = (ROOT / "Assets/CyberShield/Scripts/DataCenterMobileMission.cs").read_text(encoding="utf-8")
    lines = re.findall(r'"([^"\n]+)"', script.split("string[] Lines =", 1)[1].split("};", 1)[0])
    if len(lines) != 9:
        raise ValueError(f"Expected nine mission lines, got {len(lines)}")
    for index, line in enumerate(lines):
        config = SynthesisConfig(length_scale=0.96 if index in (0, 1, 3, 7, 8) else 1.03)
        with wave.open(str(ASSETS / "Audio" / f"Mission_{index}.wav"), "wb") as output:
            model.synthesize_wav(line, output, syn_config=config)


if __name__ == "__main__":
    write_audio(ASSETS / "Audio" / "Nightfall.wav", soundtrack())
    chord_cue(ASSETS / "Resources/Nightfall/Intercept.wav", [67, 74, 79], .045)
    chord_cue(ASSETS / "Resources/Nightfall/Victory.wav", [60, 64, 67, 72, 76], .11)
    voices()
