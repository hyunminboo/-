import wave
import struct
import math
import random
import os

def save_wav(filename, samples, sample_rate=44100):
    with wave.open(filename, 'w') as wav_file:
        wav_file.setnchannels(1)
        wav_file.setsampwidth(2)
        wav_file.setframerate(sample_rate)
        for s in samples:
            val = int(max(-1.0, min(1.0, s)) * 32767)
            wav_file.writeframesraw(struct.pack('<h', val))

# 1. Heavier Gunshot sound
def generate_gunshot():
    samples = []
    duration = 0.3
    sample_rate = 44100
    for i in range(int(sample_rate * duration)):
        t = i / sample_rate
        env = math.exp(-t * 15) # slower decay for heavier sound
        # lower frequency rumble mixed with noise
        freq = max(20, 100 - (t * 500))
        wave_val = math.sin(2 * math.pi * freq * t)
        noise = random.uniform(-1.0, 1.0)
        # Mix noise and sub-bass
        mixed = (noise * 0.4 + wave_val * 0.6)
        samples.append(mixed * env * 0.9)
    return samples

# 2. Footstep sound
def generate_footstep():
    samples = []
    duration = 0.08
    sample_rate = 44100
    for i in range(int(sample_rate * duration)):
        t = i / sample_rate
        env = math.exp(-t * 30)
        freq = 150 - (t * 1000)
        if freq < 20: freq = 20
        wave_val = math.sin(2 * math.pi * freq * t)
        noise = random.uniform(-0.1, 0.1)
        samples.append((wave_val + noise) * env * 0.5)
    return samples

# 3. Laser sound
def generate_laser():
    samples = []
    duration = 0.25
    sample_rate = 44100
    for i in range(int(sample_rate * duration)):
        t = i / sample_rate
        env = math.exp(-t * 15)
        freq = 1500 - (t * 4000)
        if freq < 100: freq = 100
        wave_val = 1.0 if math.sin(2 * math.pi * freq * t) > 0 else -1.0
        samples.append(wave_val * env * 0.3)
    return samples

# 4. Explosion (Grenade) sound
def generate_explosion():
    samples = []
    duration = 0.6
    sample_rate = 44100
    for i in range(int(sample_rate * duration)):
        t = i / sample_rate
        env = math.exp(-t * 5) # Long decay
        # Rumble frequency
        freq = max(10, 80 - (t * 100))
        wave_val = math.sin(2 * math.pi * freq * t)
        noise = random.uniform(-1.0, 1.0)
        # Mix a lot of noise with low bass rumble
        mixed = (noise * 0.7 + wave_val * 0.3)
        samples.append(mixed * env * 1.0)
    return samples

# 5. Sword Attack (Swish) sound
def generate_sword():
    samples = []
    duration = 0.2
    sample_rate = 44100
    for i in range(int(sample_rate * duration)):
        t = i / sample_rate
        env = math.exp(-t * 20)
        freq = 800 + (t * 2000)
        wave_val = math.sin(2 * math.pi * freq * t)
        noise = random.uniform(-1.0, 1.0)
        mixed = (noise * 0.7 + wave_val * 0.3)
        samples.append(mixed * env * 0.6)
    return samples

os.makedirs('Assets/Resources/Sounds', exist_ok=True)
save_wav('Assets/Resources/Sounds/Gunshot.wav', generate_gunshot())
save_wav('Assets/Resources/Sounds/Footstep.wav', generate_footstep())
save_wav('Assets/Resources/Sounds/Laser.wav', generate_laser())
save_wav('Assets/Resources/Sounds/Explosion.wav', generate_explosion())
save_wav('Assets/Resources/Sounds/Sword.wav', generate_sword())
print("Sound files generated successfully.")
