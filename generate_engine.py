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
    duration = 2.0  # 2 second loop
    total_samples = int(sample_rate * duration)
    samples = [0.0] * total_samples
    
    for i in range(total_samples):
        t = i / sample_rate
        # Low rumble: 40Hz
        freq = 40.0
        wave_val = math.sin(2 * math.pi * freq * t)
        
        # Add some variation
        noise = random.uniform(-1.0, 1.0)
        
        # Modulate amplitude slightly to sound like rotating blades/engines
        mod = 0.5 + 0.5 * math.sin(2 * math.pi * 10 * t) # 10Hz wobble
        
        mixed = (wave_val * 0.7 + noise * 0.3) * mod
        samples[i] = mixed * 0.4
        
    return samples

os.makedirs('Assets/Resources/Sounds', exist_ok=True)
save_wav('Assets/Resources/Sounds/Engine.wav', generate_engine())
print("Engine sound generated")
