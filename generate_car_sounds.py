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

def generate_engine():
    sample_rate = 44100
    duration = 2.0
    total_samples = int(sample_rate * duration)
    samples = [0.0] * total_samples
    
    for i in range(total_samples):
        t = i / sample_rate
        # Low frequency rumble
        freq = 60 + math.sin(t * 10) * 5
        wave_val = math.sin(2 * math.pi * freq * t)
        
        # Add some grit
        noise = random.uniform(-1.0, 1.0)
        
        # Envelope constant for loop
        env = 1.0
        
        # Mix
        mixed = (wave_val * 0.8 + noise * 0.2)
        samples[i] = mixed * env * 0.5
        
    return samples

def generate_crash():
    sample_rate = 44100
    duration = 1.5
    total_samples = int(sample_rate * duration)
    samples = [0.0] * total_samples
    
    for i in range(total_samples):
        t = i / sample_rate
        noise = random.uniform(-1.0, 1.0)
        
        # Initial impact peak then decay
        env = math.exp(-t * 5)
        if t < 0.1:
            env = t * 10
            
        samples[i] = noise * env * 0.9
        
    return samples

os.makedirs('Assets/Resources/Sounds', exist_ok=True)
save_wav('Assets/Resources/Sounds/CarEngine.wav', generate_engine())
save_wav('Assets/Resources/Sounds/CarCrash.wav', generate_crash())
print("Car sounds generated")
