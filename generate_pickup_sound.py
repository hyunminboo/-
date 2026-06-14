import wave
import struct
import math
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

def generate_pickup():
    sample_rate = 44100
    duration = 0.3
    total_samples = int(sample_rate * duration)
    samples = [0.0] * total_samples
    
    for i in range(total_samples):
        t = i / sample_rate
        # Quick ping: frequency rises fast
        freq = 400 + (t * 2000)
        wave_val = math.sin(2 * math.pi * freq * t)
        
        # Envelope drops to 0
        env = math.exp(-t * 15)
        
        # Mix
        samples[i] = wave_val * env * 0.7
        
    return samples

os.makedirs('Assets/Resources/Sounds', exist_ok=True)
save_wav('Assets/Resources/Sounds/ItemPickup.wav', generate_pickup())
print("Pickup sound generated")
