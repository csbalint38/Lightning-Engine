from common.common import SAMPLE_TYPES
import numpy as np

class Sampler:
    def __init__(self):
        self.count = 0

        self.delta = 0.05
        self.PI = np.pi

        self.theta_values = np.arange(0, 0.5 * self.PI, self.delta)
        self.phi_values = np.arange(0, 2 * self.PI, self.delta)

        self.x_points = []
        self.y_points = []
        self.z_points = []
        
        self.sample_iter = 0

    def sample(self, sample_type: str):
        if sample_type == SAMPLE_TYPES[0]:
            if(self.sample_iter == self.theta_values.size): return
            sin_theta = np.sin(self.theta_values[self.sample_iter])
            cos_theta = np.cos(self.theta_values[self.sample_iter])
            self._discrete(sin_theta, cos_theta)
        elif sample_type == SAMPLE_TYPES[1]:
            if(self.sample_iter == 126): return
            self._uniform()
        elif sample_type == SAMPLE_TYPES[2]:
            if(self.sample_iter == 126): return
            self._importance_sampling()
                
        self.sample_iter += 1
        
    def _discrete(self, sin_theta: float, cos_theta: float):
        for phi in self.phi_values:
            sin_phi = np.sin(phi)
            cos_phi = np.cos(phi)
    
            x = sin_theta * cos_phi
            y = sin_theta * sin_phi
            z = cos_theta
    
            self.x_points.append(x)
            self.y_points.append(y)
            self.z_points.append(z)
            
            self.count += 1
            
    def _uniform(self):
        sample_count = self.phi_values.size * self.theta_values.size

        for i in range(self.sample_iter * 32, (self.sample_iter + 1) * 32):
            x_i = self._Hammersley(i, sample_count)
            phi = 2 * self.PI * x_i[0]
            theta = 0.5 * self.PI * x_i[1]
            sin_theta = np.sin(theta)
            cos_theta = np.cos(theta)
            sin_phi = np.sin(phi)
            cos_phi = np.cos(phi)
            
            x = sin_theta * cos_phi
            y = sin_theta * sin_phi
            z = cos_theta
            
            self.x_points.append(x)
            self.y_points.append(y)
            self.z_points.append(z)

            self.count += 1
    
    def _importance_sampling(self):
        sample_count = self.phi_values.size * self.theta_values.size

        for i in range(self.sample_iter * 32, (self.sample_iter + 1) * 32):
            x_i = self._Hammersley(i, sample_count)
            phi = 2 * self.PI * x_i[0]
            sin_theta = np.sqrt(x_i[1])
            cos_theta = np.sqrt(1 - x_i[1])
            sin_phi = np.sin(phi)
            cos_phi = np.cos(phi)
            
            x = sin_theta * cos_phi
            y = sin_theta * sin_phi
            z = cos_theta
            
            self.x_points.append(x)
            self.y_points.append(y)
            self.z_points.append(z)

            self.count += 1
    
    def _radical_inverse_vdc(self, bits: int):
        bits = (bits << 16) | (bits >> 16)
        bits = ((bits & 0x55555555) << 1) | ((bits & 0xAAAAAAAA) >> 1)
        bits = ((bits & 0x33333333) << 2) | ((bits & 0xCCCCCCCC) >> 2)
        bits = ((bits & 0x0F0F0F0F) << 4) | ((bits & 0xF0F0F0F0) >> 4)
        bits = ((bits & 0x00FF00FF) << 8) | ((bits & 0xFF00FF00) >> 8)
        
        return bits * 2.3283064365386963e-10
    
    def _Hammersley(self, i: int, n: int):
        return i / n, self._radical_inverse_vdc(i)
    
    def clear(self):
        self.x_points.clear()
        self.y_points.clear()
        self.z_points.clear()
        
        self.sample_iter = 0