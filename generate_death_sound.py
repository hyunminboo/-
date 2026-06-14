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
        byte_data = bytearray(len(samples) * 2)
        for i, s in enumerate(samples):
            val = int(max(-1.0, min(1.0, s)) * 32767)
            struct.pack_into('<h', byte_data, i*2, val)
        wav_file.writeframesraw(byte_data)

def generate_player_death():
    sample_rate = 44100
    duration = 1.5
    total_samples = int(sample_rate * duration)
    samples = [0.0] * total_samples
    
    for i in range(total_samples):
        t = i / sample_rate
        # Frequency drops rapidly
        freq = 300 - (200 * t)
        if freq < 20: freq = 20
        wave_val = math.sin(2 * math.pi * freq * t)
        
        # Noise
        noise = random.uniform(-1.0, 1.0)
        
        # Envelope drops to 0
        env = math.exp(-t * 2)
        
        # Mix
        mixed = (wave_val * 0.7 + noise * 0.3)
        samples[i] = mixed * env * 0.8
        
    return samples

os.makedirs('Assets/Resources/Sounds', exist_ok=True)
save_wav('Assets/Resources/Sounds/PlayerDeath.wav', generate_player_death())
print("PlayerDeath sound generated")
