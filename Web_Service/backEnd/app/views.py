from django.shortcuts import render
from django.http import HttpResponse, JsonResponse
from django.views.decorators.csrf import csrf_exempt
import os
import uuid
# import controller

def index(request):
    return HttpResponse("<h1>Hello, world. You're at the polls index.</h1>")
# Create your views here.

@csrf_exempt
def handle_frames(request):
    
    if request.method == 'POST' and request.FILES.get('image'):
         # Process the uploaded image
        image_file = request.FILES['image']
    
        """treatement for the image to be done here
        # Directory to save the uploaded images
        save_directory = 'frames/'

        # Ensure the directory exists, create it if not
        os.makedirs(save_directory, exist_ok=True)

        # Generate a unique filename for the image
        filename = 'frame_' + str(uuid.uuid4()) + '.png'
        save_path = os.path.join(save_directory, filename)
        
        # Save the image file
        with open(save_path, 'wb') as f:
            for chunk in image_file.chunks():
                f.write(chunk)"""
        # load_input()
        # compute_matches()
        return JsonResponse({'message': 'Image uploaded successfully'})
    else:
        return JsonResponse({'message': 'Image upload failed'}, status=400)

@csrf_exempt
def handle_image_target(request):
    
    if request.method == 'POST' and request.FILES.get('image'):
         # Process the uploaded image
        image_file = request.FILES['image']
    
        """treatement for the image to be done here"""
        # Directory to save the uploaded images
        save_directory = 'iamges_target/'

        # Ensure the directory exists, create it if not
        os.makedirs(save_directory, exist_ok=True)

        # Generate a unique filename for the image
        filename = 'target_' + str(uuid.uuid4()) + '.png'
        save_path = os.path.join(save_directory, filename)
        
        # Save the image file
        with open(save_path, 'wb') as f:
            for chunk in image_file.chunks():
                f.write(chunk)
        # load_input()
        # compute_matches()
        return JsonResponse({'message': 'Image uploaded successfully'})
    else:
        return JsonResponse({'message': 'Image upload failed'}, status=400)

