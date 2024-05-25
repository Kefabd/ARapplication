# your_app/controllers/orb_detector.py

import cv2
import numpy as np
import os

# Initialisation de l'ORB et des paramètres de FLANN
MIN_MATCHES = 5
detector = cv2.ORB_create(nfeatures=5000)
FLANN_INDEX_KDTREE = 1
index_params = dict(algorithm=FLANN_INDEX_KDTREE, trees=5)
search_params = dict(checks=100)
flann = cv2.FlannBasedMatcher(index_params, search_params)

def load_reference_image(nparr):
    input_image = cv2.imdecode(nparr, cv2.IMREAD_COLOR)
    if input_image is None:
        raise FileNotFoundError(f"Could not load the image at input image")
    
    input_image = cv2.resize(input_image, (300, 400), interpolation=cv2.INTER_AREA)  
    gray_image = cv2.cvtColor(input_image, cv2.COLOR_BGR2GRAY)
    keypoints, descriptors = detector.detectAndCompute(gray_image, None)
    
    return gray_image, keypoints, descriptors

def compute_matches(descriptors_input, descriptors_output):
    if descriptors_input is not None and descriptors_output is not None:
        if len(descriptors_output) != 0 and len(descriptors_input) != 0:
            matches = flann.knnMatch(np.asarray(descriptors_input, np.float32), np.asarray(descriptors_output, np.float32), k=2)
            good = [m for m, n in matches if m.distance < 0.69 * n.distance]
            return good
    return []

def extract_marker_coordinates(input_keypoints, output_keypoints, matches):
    src_pts = np.float32([input_keypoints[m.queryIdx].pt for m in matches]).reshape(-1, 1, 2)
    dst_pts = np.float32([output_keypoints[m.trainIdx].pt for m in matches]).reshape(-1, 1, 2)

    M, mask = cv2.findHomography(src_pts, dst_pts, cv2.RANSAC, 5.0)

    # Transformez les points correspondants des marqueurs en utilisant la matrice d'homographie
    transformed_pts = cv2.perspectiveTransform(src_pts, M)

    # Retournez les coordonnées des marqueurs détectés
    marker_coordinates = []
    for point in transformed_pts:
        x, y = point.ravel()
        marker_coordinates.append((x, y))
    
    return marker_coordinates



def process_uploaded_image(nparr, input_descriptors, input_keypoints):
    frame = cv2.imdecode(nparr, cv2.IMREAD_COLOR)
    if frame is None:
        raise FileNotFoundError(f"Could not load the image")

    frame_bw = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
    output_keypoints, output_descriptors = detector.detectAndCompute(frame_bw, None)
    print("Nombre de keypoints détectés dans l'image de sortie:", len(output_keypoints))
    matches = compute_matches(input_descriptors, output_descriptors)
    print("Nombre de correspondances trouvées:", len(matches))
   

    if len(matches) > 10:
        marker_coordinates = extract_marker_coordinates(input_keypoints, output_keypoints, matches)
        return marker_coordinates
    else:
        return None

